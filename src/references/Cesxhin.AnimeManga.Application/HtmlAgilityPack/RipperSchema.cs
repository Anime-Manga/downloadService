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
using System.Text.RegularExpressions;

namespace Cesxhin.AnimeManga.Application.HtmlAgilityPack
{
    public static class RipperSchema
    {
        //log
        private static readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

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
                    if (types[i] == "string")
                    {
                        return doc.DocumentNode
                        .SelectNodes(path)
                        .First()
                        .ChildNodes[childNodesSelect]
                        .InnerHtml;
                    }else if (types[i] == "number")
                    {
                        float intResult;

                        var result = doc.DocumentNode
                        .SelectNodes(path)
                        .First()
                        .ChildNodes[childNodesSelect]
                        .InnerHtml;

                        if (schema.ContainsKey("removeWords")){
                           var words = schema.GetValue("removeWords").ToObject<IEnumerable<string>>();

                            foreach (var word in words)
                            {
                                result = result.ToLower().Replace(word.ToLower(), "");
                            }

                            result = result.Trim();
                        }

                        if (float.TryParse(result, out intResult))
                        {
                            if (schema.ContainsKey("startZero") && schema.GetValue("startZero").ToObject<bool>() == true)
                                return intResult + 1;

                            return intResult;
                        }
                        else
                            return -1;
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
                        var attributes = (string)schema.GetValue("attributes");

                        if(attributes == null)
                        {
                            return doc.DocumentNode
                                .SelectNodes(path)
                                .ToList();
                        }

                        var resultArray = new List<string>();
                        var list = doc.DocumentNode
                        .SelectNodes(path)
                        .ToList();

                        foreach (var item in list)
                        {
                            resultArray.Add(item.Attributes[attributes].Value);
                        }

                        return resultArray;
                    }else if (types[i] == "book/link")
                    {
                        var attributes = (string)schema.GetValue("attributes");

                        var url = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes[attributes].Value;

                        if (schema.ContainsKey("addUrl")){
                            var addUrl = (string)schema.GetValue("addUrl");
                            if (!url.Contains(addUrl)){
                                if (url.LastIndexOf('/') == (url.Trim().Length - 1))
                                    url = $"{url.Substring(0, url.LastIndexOf('/'))}/{addUrl}";
                                else
                                    url += addUrl;
                            }
                        }

                        return DownloadMetadataChapter(schema, url, name);
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


        public static string RemoveSpecialCharacters(string str)
        {
            //remove character special
            return Regex.Replace(str, "[^a-zA-Z0-9_() ]+", "", RegexOptions.Compiled);
        }

        private static ChapterDTO DownloadMetadataChapter(JObject schema, string urlBook, string name)
        {
            var doc = new HtmlWeb().Load(urlBook);

            //get maxImage
            var maxImageSchema = schema.GetValue("numberMaxImage").ToObject<JObject>();
            var maxImage = GetValue(maxImageSchema, doc);
            maxImage = maxImage[maxImage.Count - 1];
            maxImage = int.Parse(maxImage);

            if(maxImageSchema.ContainsKey("startZero") && maxImageSchema.GetValue("startZero").ToObject<bool>() == true)
                maxImage += 1;

            //get number volume
            var numberVolumeSchema = schema.GetValue("getVolume").ToObject<JObject>();
            var numberVolume = GetValue(numberVolumeSchema, doc);

            //get number chapter
            var numberChapterSchema = schema.GetValue("getChapter").ToObject<JObject>();
            var numberChapter = GetValue(numberChapterSchema, doc);

            return new ChapterDTO
            {
                ID = $"{name}-v{numberVolume}-c{numberChapter}",
                CurrentChapter = numberChapter,
                CurrentVolume = numberVolume,
                NameManga = name,
                UrlPage = urlBook,
                NumberMaxImage = maxImage
            };
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
