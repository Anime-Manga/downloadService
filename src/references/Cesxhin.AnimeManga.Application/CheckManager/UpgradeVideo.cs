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
    public class UpgradeVideo : IUpgrade
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //variables
        private readonly string _folder = Environment.GetEnvironmentVariable("BASE_PATH") ?? "/";
        private readonly JObject schemas = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));

        //list
        private List<GenericVideoDTO> listGenerics = new();
        private List<EpisodeRegisterDTO> listEpisodeRegister;
        private List<EpisodeRegisterDTO> blacklist;

        //api
        private readonly Api<GenericVideoDTO> genericApi = new();
        private readonly Api<EpisodeDTO> episodeApi = new();
        private readonly Api<EpisodeRegisterDTO> episodeRegisterApi = new();

        //rabbit
        private readonly IBus _publishEndpoint;

        public UpgradeVideo(IBus publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public void ExecuteUpgrade()
        {
            _logger.Info($"Start upgrade video");
            foreach (var item in schemas)
            {
                var schema = schemas.GetValue(item.Key).ToObject<JObject>();
                if (schema.GetValue("type").ToString() == "video")
                {
                    try
                    {
                        var query = new Dictionary<string, string>()
                        {
                            ["nameCfg"] = item.Key
                        };

                        listGenerics = genericApi.GetMore("/video/all", query).GetAwaiter().GetResult();
                    }
                    catch (ApiNotFoundException ex)
                    {
                        _logger.Error($"not found, details: " + ex.Message);
                    }
                    catch (ApiGenericException ex)
                    {
                        _logger.Fatal($"Error generic get all, details error: {ex.Message}");
                    }

                    //step check on website if the anime is still active
                    foreach (var list in listGenerics)
                    {
                        var video = JObject.Parse(list.Video);
                        string name_id = (string)video.GetValue("name_id");
                        string urlPage = (string)video.GetValue("url_page");

                        //get list episodes by name
                        List<EpisodeDTO> checkEpisodes = null;
                        List<EpisodeDTO> listEpisodesAdd = null;

                        _logger.Info($"Check new episodes for Anime: {name_id}");

                        //check new episode
                        checkEpisodes = RipperVideoGeneric.GetEpisodes(schema, urlPage, name_id, item.Key);

                        //check if null
                        if (checkEpisodes == null)
                        {
                            _logger.Error($"Can't download with this url, {urlPage}");
                            continue;
                        }

                        listEpisodesAdd = new(checkEpisodes);
                        blacklist = new();

                        foreach (var checkEpisode in checkEpisodes)
                        {
                            foreach (var episode in list.Episodes)
                            {
                                if (episode.NumberEpisodeCurrent == checkEpisode.NumberEpisodeCurrent)
                                {
                                    blacklist.Add(list.EpisodesRegister.Find(e => e.EpisodeId == episode.ID));
                                    listEpisodesAdd.Remove(checkEpisode);
                                    break;
                                }
                            }
                        }

                        if (listEpisodesAdd.Count > 0)
                        {
                            _logger.Info($"There are new episodes ({listEpisodesAdd.Count}) of {name_id}");

                            //insert to db
                            try
                            {
                                listEpisodesAdd = episodeApi.PostMore("/episodes", listEpisodesAdd).GetAwaiter().GetResult();
                            }
                            catch (ApiGenericException ex)
                            {
                                _logger.Fatal($"Error generic post episodes, details error: {ex.Message}");
                            }

                            //create episodeRegister
                            listEpisodeRegister = new();

                            string pathDefault = null;
                            string path = null;

                            if (blacklist.Count > 0)
                                pathDefault = Path.GetDirectoryName(blacklist.FirstOrDefault().EpisodePath);

                            foreach (var episode in listEpisodesAdd)
                            {
                                path = "";
                                //use path how others episodesRegisters
                                if (pathDefault != null)
                                {
                                    path = $"{pathDefault}/{episode.VideoId} s{episode.NumberSeasonCurrent.ToString("D2")}e{episode.NumberEpisodeCurrent.ToString("D2")}.mp4";
                                }
                                else //default
                                {
                                    path = $"{_folder}/{episode.VideoId}/Season {episode.NumberSeasonCurrent.ToString("D2")}/{episode.VideoId} s{episode.NumberSeasonCurrent.ToString("D2")}e{episode.NumberEpisodeCurrent.ToString("D2")}.mp4";
                                }

                                listEpisodeRegister.Add(new EpisodeRegisterDTO
                                {
                                    EpisodeId = episode.ID,
                                    EpisodePath = path
                                });
                            }

                            try
                            {
                                episodeRegisterApi.PostMore("/episodes/registers", listEpisodeRegister).GetAwaiter();
                            }
                            catch (ApiGenericException ex)
                            {
                                _logger.Fatal($"Error generic post registers, details error: {ex.Message}");
                            }

                            //create message for notify
                            string message = $"💽UpgradeService say: \nAdd new episode of {name_id}\n";

                            listEpisodesAdd.Sort(delegate (EpisodeDTO p1, EpisodeDTO p2) { return p1.NumberEpisodeCurrent.CompareTo(p2.NumberEpisodeCurrent); });
                            foreach (var episodeNotify in listEpisodesAdd)
                            {
                                message += $"- {episodeNotify.VideoId} Episode: {episodeNotify.NumberEpisodeCurrent}\n";
                            }

                            try
                            {
                                var messageNotify = new NotifyDTO
                                {
                                    Message = message,
                                    Image = (string)video.GetValue("cover")
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
                        listEpisodesAdd.Clear();
                    }
                }
            }

            _logger.Info($"End upgrade video");
        }
    }
}
