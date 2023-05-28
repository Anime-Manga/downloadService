using Cesxhin.AnimeManga.Application.CheckManager.Interfaces;
using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using MassTransit;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cesxhin.AnimeManga.Application.CheckManager
{
    public class UpdateBook : IUpdate
    {
        //interface
        private readonly IBus _publishEndpoint;

        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        private readonly string _folder = Environment.GetEnvironmentVariable("BASE_PATH") ?? "/";
        private readonly JObject schemas = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));

        //Instance Parallel
        private readonly ParallelManager<object> parallel = new();

        //Istance Api
        private readonly Api<GenericBookDTO> bookApi = new();
        private readonly Api<ChapterDTO> chapterApi = new();
        private readonly Api<ChapterRegisterDTO> chapterRegisterApi = new();

        //download api
        private List<GenericBookDTO> listBook = null;

        public UpdateBook(IBus publicEndpoint)
        {
            _publishEndpoint = publicEndpoint;
        }

        public void ExecuteUpdate()
        {
            _logger.Info($"Start update book");

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

                        listBook = bookApi.GetMore("/book/all", query).GetAwaiter().GetResult();
                    }
                    catch (ApiNotFoundException ex)
                    {
                        _logger.Error($"Not found get all, details error: {ex.Message}");
                    }
                    catch (ApiGenericException ex)
                    {
                        _logger.Fatal($"Error generic get all, details error: {ex.Message}");
                    }

                    //if exists listManga
                    if (listBook != null && listBook.Count > 0)
                    {
                        var tasks = new List<Func<object>>();
                        //step one check file
                        foreach (var book in listBook)
                        {

                            //foreach chapters
                            foreach (var chapter in book.Chapters)
                            {
                                tasks.Add(new Func<object>(() => Checkchapter(book, chapter, chapterApi, chapterRegisterApi)));
                            }
                        }
                        parallel.AddTasks(tasks);
                        parallel.Start();
                        parallel.WhenCompleted();
                        parallel.ClearList();
                    }
                }

            }

            _logger.Info($"End update book");
        }

        private object Checkchapter(GenericBookDTO book, ChapterDTO chapter, Api<ChapterDTO> chapterApi, Api<ChapterRegisterDTO> chapterRegisterApi)
        {
            var chapterRegister = book.ChapterRegister.Find(e => e.ChapterId == chapter.ID);
            if (chapterRegister == null)
            {
                _logger.Warn($"not found chapterRegister by chapter id: {chapter.ID}");
                return null;
            }

            for (int i = 0; i < chapterRegister.ChapterPath.Length; i++)
            {
                _logger.Debug($"check {chapterRegister.ChapterPath[i]}");

                //check integry file
                if (chapter.StateDownload == null || chapter.StateDownload == "failed" || (chapter.StateDownload == "completed" && chapterRegister.ChapterHash == null))
                {
                    ConfirmStartDownload(chapter, chapterApi);
                }
                else if ((!File.Exists(chapterRegister.ChapterPath[i]) && chapter.StateDownload != "pending"))
                {
                    var found = false;
                    string newHash;

                    foreach (string file in Directory.EnumerateFiles(_folder, "*.png", SearchOption.AllDirectories))
                    {
                        newHash = Hash.GetHash(file);
                        if (newHash == chapterRegister.ChapterHash[i])
                        {
                            _logger.Info($"I found file (chapter id: {chapter.ID}) that was move, now update information");

                            //update
                            chapterRegister.ChapterPath[i] = file;
                            try
                            {
                                chapterRegisterApi.PutOne("/chapter/register", chapterRegister).GetAwaiter().GetResult();

                                _logger.Info($"Ok update chapter id: {chapter.ID} that was move");

                                //return
                                found = true;
                            }
                            catch (ApiNotFoundException ex)
                            {
                                _logger.Error($"Not found chapterRegister id: {chapterRegister.ChapterId} for update information, details: {ex.Message}");
                            }
                            catch (ApiConflictException ex)
                            {
                                _logger.Error($"Error conflict put chapterRegister, details error: {ex.Message}");
                            }
                            catch (ApiGenericException ex)
                            {
                                _logger.Fatal($"Error generic put chapterRegister, details error: {ex.Message}");
                            }

                            break;
                        }
                    }

                    //if not found file
                    if (found == false)
                        ConfirmStartDownload(chapter, chapterApi);
                }
            }

            return null;
        }

        private async void ConfirmStartDownload(ChapterDTO chapter, Api<ChapterDTO> chapterApi)
        {
            //set pending to 
            chapter.StateDownload = "pending";

            try
            {
                //set change status
                await chapterApi.PutOne("/book/statusDownload", chapter);

                await _publishEndpoint.Publish(chapter);
                _logger.Info($"this file ({chapter.NameManga} volume: {chapter.CurrentVolume} chapter: {chapter.CurrentChapter}) does not exists, sending message to DownloadService");
            }
            catch (ApiNotFoundException ex)
            {
                _logger.Error($"Impossible update chapter becouse not found chapter id: {chapter.ID}, details: {ex.Message}");
            }
            catch (ApiGenericException ex)
            {
                _logger.Fatal($"Error update chapter, details error: {ex.Message}");
            }
        }
    }
}
