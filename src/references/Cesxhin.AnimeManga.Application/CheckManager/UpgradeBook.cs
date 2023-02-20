using Cesxhin.AnimeManga.Application.CheckManager.Interfaces;
using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.HtmlAgilityPack;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using MassTransit;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cesxhin.AnimeManga.Application.CheckManager
{
    public class UpgradeBook : IUpgrade
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //variables
        private readonly string _folder = Environment.GetEnvironmentVariable("BASE_PATH") ?? "/";
        private readonly JObject schemas = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));

        //list
        private List<GenericBookDTO> listGenerics = new();
        private List<ChapterRegisterDTO> listChapterRegister;
        private List<ChapterRegisterDTO> blacklist;

        //api
        private readonly Api<GenericBookDTO> genericApi = new();
        private readonly Api<ChapterDTO> chapterApi = new();
        private readonly Api<ChapterRegisterDTO> chapterRegisterApi = new();

        //rabbit
        private readonly IBus _publishEndpoint;

        public UpgradeBook(IBus publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public void ExecuteUpgrade()
        {
            _logger.Info($"Start upgrade book");
            foreach (var item in schemas)
            {
                var schema = schemas.GetValue(item.Key).ToObject<JObject>();
                if (schema.GetValue("type").ToString() == "book")
                {
                    try
                    {
                        var query = new Dictionary<string, string>()
                        {
                            ["nameCfg"] = item.Key
                        };

                        listGenerics = genericApi.GetMore("/book/all", query).GetAwaiter().GetResult();
                    }
                    catch (ApiNotFoundException ex)
                    {
                        _logger.Error($"not found, details: " + ex.Message);
                    }

                    //step check on website if the anime is still active
                    foreach (var list in listGenerics)
                    {
                        var book = JObject.Parse(list.Book);

                        //get list episodes by name
                        List<ChapterDTO> checkChapters = null;
                        List<ChapterDTO> listChaptersAdd = null;

                        var urlPage = (string)book.GetValue("url_page");
                        var name_id = (string)book.GetValue("name_id");

                        _logger.Info("Check new episodes for manga: " + urlPage);

                        //check new episode
                        checkChapters = RipperBookGeneric.GetChapters(schema, urlPage, name_id, item.Key);

                        //check if null
                        if (checkChapters == null)
                        {
                            _logger.Error($"Can't download with this url, {urlPage}");
                            continue;
                        }

                        listChaptersAdd = new(checkChapters);
                        blacklist = new();

                        foreach (var checkChapter in checkChapters)
                        {
                            foreach (var chapter in list.Chapters)
                            {
                                if (chapter.CurrentChapter == checkChapter.CurrentChapter)
                                {
                                    blacklist.Add(list.ChapterRegister.Find(e => e.ChapterId == chapter.ID));
                                    listChaptersAdd.Remove(checkChapter);
                                    break;
                                }
                            }
                        }

                        if (listChaptersAdd.Count > 0)
                        {
                            _logger.Info($"There are new chapters ({listChaptersAdd.Count}) of {name_id}");

                            //insert to db
                            listChaptersAdd = chapterApi.PostMore("/chapters", listChaptersAdd).GetAwaiter().GetResult();

                            //create episodeRegister
                            listChapterRegister = new();

                            string pathDefault = null;
                            List<string> paths = new();

                            if (blacklist.Count > 0)
                                pathDefault = Path.GetDirectoryName(blacklist.FirstOrDefault().ChapterPath.First());

                            foreach (var chapter in listChaptersAdd)
                            {
                                //use path how others episodesRegisters
                                for (int i = 0; i <= chapter.NumberMaxImage; i++)
                                {
                                    if (pathDefault != null)
                                    {
                                        paths.Add($"{pathDefault}/{chapter.NameManga}/Volume {chapter.CurrentVolume}/Chapter {chapter.CurrentChapter}/{chapter.NameManga} s{chapter.CurrentVolume}c{chapter.CurrentChapter}n{i}.png");
                                    }
                                    else //default
                                    {
                                        paths.Add($"{_folder}/{chapter.NameManga}/Volume {chapter.CurrentVolume}/Chapter {chapter.CurrentChapter}/{chapter.NameManga} s{chapter.CurrentVolume}c{chapter.CurrentChapter}n{i}.png");
                                    }
                                }

                                listChapterRegister.Add(new ChapterRegisterDTO
                                {
                                    ChapterId = chapter.ID,
                                    ChapterPath = paths.ToArray()
                                });

                                paths.Clear();
                            }

                            chapterRegisterApi.PostMore("/chapters/registers", listChapterRegister).GetAwaiter();

                            //create message for notify
                            string message = $"💽UpgradeService say: \nAdd new chapter of {name_id}\n";

                            listChaptersAdd.Sort(delegate (ChapterDTO p1, ChapterDTO p2) { return p1.CurrentChapter.CompareTo(p2.CurrentChapter); });
                            foreach (var episodeNotify in listChaptersAdd)
                            {
                                message += $"- {episodeNotify.ID} Chapter: {episodeNotify.CurrentChapter}\n";
                            }

                            try
                            {
                                var messageNotify = new NotifyDTO
                                {
                                    Message = message,
                                    Image = (string)book.GetValue("cover")
                                };
                                _publishEndpoint.Publish(messageNotify).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"Cannot send message rabbit, details: {ex.Message}");
                            }


                            _logger.Info($"Done upgrade! of {name_id}");
                        }
                        //clear resource
                        listChaptersAdd.Clear();
                    }
                }
            }
            

            _logger.Info($"End upgrade book");
        }
    }
}
