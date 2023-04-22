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

        public static dynamic GetValue(JObject schema, HtmlDocument doc, int numberSeason = 0, int numberEpisode = 0, string name = null, string nameCfg = null)
        {
            var paths = schema.GetValue("path").ToObject<IEnumerable<string>>();
            int i = 0, childNodesSelect = 0;
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
                        .InnerHtml.Trim();
                    }
                    else if (types[i] == "number")
                    {
                        var result = doc.DocumentNode
                        .SelectNodes(path)
                        .First()
                        .ChildNodes[childNodesSelect]
                        .InnerHtml;

                        if (schema.ContainsKey("removeWords"))
                        {
                            var words = schema.GetValue("removeWords").ToObject<IEnumerable<string>>();

                            foreach (var word in words)
                            {
                                result = result.ToLower().Replace(word.ToLower(), "");
                            }

                            result = result.Trim();
                        }
                        dynamic resultParse;

                        if (schema.ContainsKey("parse") && schema.GetValue("parse").ToString() == "number")
                        {
                            try
                            {
                                resultParse = int.Parse(result);
                            }
                            catch (FormatException ex)
                            {
                                resultParse = null;
                            }
                        }
                        else
                        {
                            try
                            {
                                resultParse = float.Parse(result);
                            }
                            catch (FormatException ex)
                            {
                                resultParse = null;
                            }
                        }

                        if (schema.ContainsKey("startZero") && schema.GetValue("startZero").ToObject<bool>() == true)
                            return (resultParse + 1);

                        return resultParse;
                    }
                    else if (types[i] == "link") //Attributes.Value
                    {
                        var attributes = (string)schema.GetValue("attributes");

                        if (schema.ContainsKey("numberPage"))
                        {
                            return doc.DocumentNode
                            .SelectNodes(path.Replace("{numberPage}", (string)schema.GetValue("numberPage")))
                            .First()
                            .Attributes[attributes].Value;
                        }

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
                        return result;
                    }
                    else if (types[i] == "video/mp4")
                    {
                        var url = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes["src"].Value;

                        return DownloadMetadataEpisode(numberSeason, numberEpisode, url, name, true, nameCfg);
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

                        return DownloadMetadataEpisode(numberSeason, numberEpisode, url, name, false, nameCfg);
                    }
                    else if (types[i] == "array")
                    {
                        var attributes = (string)schema.GetValue("attributes");

                        if (attributes == null)
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
                    }
                    else if (types[i] == "book/link")
                    {
                        var attributes = (string)schema.GetValue("attributes");

                        var url = doc.DocumentNode
                            .SelectNodes(path)
                            .First()
                            .Attributes[attributes].Value;

                        if (schema.ContainsKey("addUrl"))
                        {
                            var addUrl = (string)schema.GetValue("addUrl");
                            if (!url.Contains(addUrl))
                            {
                                if (url.LastIndexOf('/') == (url.Trim().Length - 1))
                                    url = $"{url.Substring(0, url.LastIndexOf('/'))}/{addUrl}";
                                else
                                    url += addUrl;
                            }
                        }

                        return DownloadMetadataChapter(schema, url, name, nameCfg);
                    }
                    break;
                }
                catch (Exception e)
                {
                    _logger.Warn(e);

                    continue;
                }
                finally
                {
                    if ((i + 1) < types.Length)
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

        private static ChapterDTO DownloadMetadataChapter(JObject schema, string urlBook, string name, string nameCfg)
        {
            int attempt = 5;

            while (attempt > 0)
            {
                var doc = new HtmlWeb().Load(urlBook);

                //get maxImage
                var maxImageSchema = schema.GetValue("numberMaxImage").ToObject<JObject>();
                var maxImage = GetValue(maxImageSchema, doc);
                maxImage = maxImage[maxImage.Count - 1];
                maxImage = int.Parse(maxImage);

                if (maxImageSchema.ContainsKey("startZero") && maxImageSchema.GetValue("startZero").ToObject<bool>() == true)
                    maxImage += 1;

                //get number volume
                var numberVolumeSchema = schema.GetValue("getVolume").ToObject<JObject>();
                var numberVolume = GetValue(numberVolumeSchema, doc);

                if (numberVolume == null)
                    numberVolume = 1;

                //get number chapter
                var numberChapterSchema = schema.GetValue("getChapter").ToObject<JObject>();
                var numberChapter = GetValue(numberChapterSchema, doc);

                try
                {
                    return new ChapterDTO
                    {
                        ID = $"{name}-v{numberVolume}-c{numberChapter}",
                        CurrentChapter = numberChapter,
                        CurrentVolume = numberVolume,
                        NameManga = name,
                        UrlPage = urlBook,
                        NumberMaxImage = maxImage,
                        NameCfg = nameCfg
                    };
                }
                catch
                {
                    attempt--;
                    _logger.Warn($"Failed download url: {urlBook}, remaining attempts {attempt}");
                }
            }

            _logger.Fatal($"Impossible download this chapter, details: {urlBook}");
            return null;
        }

        private static EpisodeDTO DownloadMetadataEpisode(int numberSeason, int numberEpisode, string urlVideo, string name, bool mp4, string nameCfg)
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
                    endNumberBuffer = playerUrl.endNumberBuffer,
                    nameCfg = nameCfg
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
                    NumberSeasonCurrent = numberSeason,
                    nameCfg = nameCfg

                };
            }
        }
    }
}
