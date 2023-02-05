using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using HtmlAgilityPack;
using m3uParser;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Cesxhin.AnimeManga.Application.HtmlAgilityPack
{
    public static class RipperSchema
    {
        //log
        private static readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        public static JObject readFile(string nameCfg)
        {
            var schemasFile = System.IO.File.ReadAllText(".\\schemas.json");
            var schema = JObject.Parse(schemasFile);

            if (schema.ContainsKey(nameCfg))
            {
                return schema.GetValue(nameCfg).ToObject<JObject>();
            }
            else
                throw new Exception("Not exsists cfg: "+nameCfg);
        }

        public static dynamic GetValue(JObject schema, HtmlDocument doc, int numberSeason = 0, int numberEpisode = 0, string name = null)
        {
            var paths = schema.GetValue("path").ToObject<IEnumerable<string>>();
            int i = 0, childNodesSelect=0;
            var types = schema.GetValue("type").ToObject<IEnumerable<string>>().ToArray();

            //check nodes
            if (schema.ContainsKey("child_nodes"))
                childNodesSelect = (int)schema.GetValue("child_nodes");

            //array for alternative paths
            foreach (var path in paths)
            {
                try
                {
                    //InnerHtml
                    if (types[i] == "string" || types[i] == "number")
                    {
                        return doc.DocumentNode
                        .SelectNodes(path)
                        .First()
                        .ChildNodes[childNodesSelect]
                        .InnerHtml;
                    }
                    else if (types[i] == "link") //Attributes.Value
                    {
                        var attributes = (string)schema.GetValue("attributes");

                        return doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes[attributes].Value;
                    }
                    else if (types[i] == "image") //Attributes.Value
                    {
                        var attributes = (string)schema.GetValue("attributes");
                        string result = null;

                        result = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes[attributes].Value;

                        if (schema.ContainsKey("download") && (bool)schema.GetValue("download") == true)
                        {
                            try
                            {
                                using (var webClient = new WebClient())
                                {
                                    return Convert.ToBase64String(webClient.DownloadData(result));
                                }
                            }
                            catch
                            {
                                _logger.Error($"Error download from this url {result}");
                            }
                        }
                    }
                    else if (types[i] == "video/mp4")
                    {
                        var url = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes["src"].Value;

                        return DownloadMetadataEpisode(numberSeason, numberEpisode, url, name, true);
                    }
                    else if (types[i] == "video/m3u8/script")
                    {
                        var script = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .InnerText;

                        var startWord = schema.GetValue("startSearch").ToString();
                        var endWord = schema.GetValue("endSearch").ToString();

                        var start = script.IndexOf(startWord);

                        script = script.Substring(start + startWord.Length);

                        var end = script.IndexOf(endWord);

                        var url = script.Substring(0, end);

                        return DownloadMetadataEpisode(numberSeason, numberEpisode, url, name, false);
                    }
                    else if (types[i] == "array")
                    {
                        var resultArray = new List<string>();
                        var list = doc.DocumentNode
                        .SelectNodes(path)
                        .ToList();

                        foreach (var item in list)
                        {
                            resultArray.Add(item.Attributes["href"].Value);
                        }

                        return resultArray;
                    }
                    break;
                }
                catch(Exception e)
                {
                    _logger.Warn(e);
                    continue;
                }
                finally
                {
                    if((i + 1) < types.Length)
                        i++;
                }
            }

            _logger.Error("Error get value by schema");
            return null;
        }



        private static EpisodeDTO DownloadMetadataEpisode(int numberSeason, int numberEpisode, string urlVideo, string name, bool mp4)
        {
            PlayerUrl playerUrl = null;

            if (mp4 == false)
            {
                _logger.Debug($"Start download url with buffer: {urlVideo}");

                playerUrl = new PlayerUrl
                {
                    Playlist = urlVideo
                };

                playerUrl.BaseUrl = playerUrl.Playlist.Replace("/playlist.m3u8", "");

                //download source files
                WebClient client = new();
                var bytes = client.DownloadData(playerUrl.Playlist);
                var sourceFiles = System.Text.Encoding.UTF8.GetString(bytes);

                var contentM3u = M3U.Parse(sourceFiles);
                string file = contentM3u.Warnings.First();

                playerUrl.PlaylistSources = file.Substring(file.LastIndexOf("./") + 1);
                playerUrl.Resolution = playerUrl.PlaylistSources.Substring(1, playerUrl.PlaylistSources.IndexOf("p"));

                //get list bytes for file
                bytes = client.DownloadData(playerUrl.BaseUrl + playerUrl.PlaylistSources);
                sourceFiles = System.Text.Encoding.UTF8.GetString(bytes);
                contentM3u = M3U.Parse(sourceFiles);
                playerUrl.endNumberBuffer = contentM3u.Medias.Count() - 1; //start 0 to xx

                _logger.Debug($"Done download url with buffer: {urlVideo}");
            }

            if (playerUrl != null)
            {
                return new EpisodeDTO
                {
                    ID = $"{name}-s{numberSeason}-e{numberEpisode}",
                    VideoId = name,
                    NumberEpisodeCurrent = numberEpisode,
                    BaseUrl = playerUrl.BaseUrl,
                    Playlist = playerUrl.Playlist,
                    PlaylistSources = playerUrl.PlaylistSources,
                    Resolution = playerUrl.Resolution,
                    NumberSeasonCurrent = numberSeason,
                    endNumberBuffer = playerUrl.endNumberBuffer
                };
            }
            else
            {
                return new EpisodeDTO
                {
                    ID = $"{name}-s{numberSeason}-e{numberEpisode}",
                    VideoId = name,
                    UrlVideo = urlVideo,
                    NumberEpisodeCurrent = numberEpisode,
                    NumberSeasonCurrent = numberSeason

                };
            }
        }
    }
}
