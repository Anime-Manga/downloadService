using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;

namespace Cesxhin.AnimeManga.Application.HtmlAgilityPack
{
    public static class RipperVideoGeneric
    {
        //log
        private static readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //parallel
        private static readonly ParallelManager<EpisodeDTO> parallel = new();
        private static readonly ParallelManager<string> _parallel = new();

        public static JObject GetDescriptionVideo(JObject schema, string urlPage)
        {
            _logger.Info($"Start download page video: {urlPage}");

            //get page
            HtmlDocument doc = new HtmlWeb().Load(urlPage);

            //create empty for save to db
            var descriptionDB = JObject.Parse("{}").ToObject<JObject>();

            //get dynamic fields
            var fields = schema.GetValue("description").ToObject<JObject>();
            foreach (var nameField in fields)
            {
                var result = RipperSchema.GetValue(fields.GetValue(nameField.Key).ToObject<JObject>(), doc);
                int intResult;
                if (int.TryParse(result, out intResult))
                    descriptionDB.Add(nameField.Key, intResult);
                else
                    descriptionDB.Add(nameField.Key, result.Trim().Replace(" +", ""));
            }

            descriptionDB["url_page"] = urlPage;
            descriptionDB["name_id"] = RipperSchema.RemoveSpecialCharacters(descriptionDB.GetValue("name_id").ToString());

            _logger.Info($"End download page video: {urlPage}");

            return descriptionDB;
        }

        public static dynamic GetEpisodesRecursive(JObject actualProcedure, int step, int current, string urlPage, int numberSeason, int numberEpisode, string name)
        {
            var stepSelect = actualProcedure[current.ToString()].ToObject<JObject>();

            //array for alternative paths
            var doc = new HtmlWeb().Load(urlPage);
            var newUrlPage = RipperSchema.GetValue(stepSelect, doc, numberSeason, numberEpisode, name);

            //set step
            current += 1;

            if (current == step)
                return newUrlPage;

            return GetEpisodesRecursive(actualProcedure, step, current, newUrlPage, numberSeason, numberEpisode, name);
        }

        public static List<EpisodeDTO> GetEpisodes(JObject schema, string urlPage, string name)
        {
            //set variable
            List<EpisodeDTO> episodes = new();
            int numberSeason = 1; //default
            int numberEpisode = 1;

            _logger.Info($"Start download page episode: {urlPage}");

            //collection
            var collection = schema.GetValue("video").ToObject<JObject>().GetValue("collection").ToObject<JObject>();
            HtmlDocument doc = new HtmlWeb().Load(urlPage);

            var resultCollection = RipperSchema.GetValue(collection, doc);

            //procedure
            var procedure = schema.GetValue("video").ToObject<JObject>().GetValue("procedure").ToObject<JObject>();
            var resultVideos = new List<EpisodeDTO>();

            if (collection.GetValue("thread").ToObject<bool>() == true)
            {
                List<Func<EpisodeDTO>> tasks = new();

                foreach (var item in resultCollection)
                {
                    var currentItem = item;
                    tasks.Add(new Func<EpisodeDTO>(() => { return GetEpisodesRecursive(procedure, procedure.Count, 0, currentItem, numberSeason, numberEpisode, name); }));
                    numberEpisode += 1;
                }

                //when finish
                parallel.AddTasks(tasks);
                parallel.Start();
                parallel.WhenCompleted();
                resultVideos = parallel.GetResultAndClear();
            }
            else
            {
                foreach (var item in resultCollection)
                {
                    resultVideos.Add(GetEpisodesRecursive(procedure, procedure.Count, 0, item, numberSeason, numberEpisode, name));
                    numberEpisode += 1;
                }
            }

            _logger.Info($"End download page episode: {urlPage}");

            return resultVideos;
        }

        //get list anime external
        public static List<GenericUrl> GetVideoUrl(JObject schema, string name)
        {
            _logger.Info($"Start download list video, search: {name}");

            List<GenericUrl> listUrlVideo = new();

            HtmlDocument doc;

            string url, prefixPage = null, prefixSearch, imageUrl = null, urlPage = null, nameVideo = null;
            var docBook = new HtmlDocument();

            var page = 1;
            while (true)
            {
                try
                {
                    url = schema.GetValue("url_search").ToString();
                    prefixSearch = schema.GetValue("prefixSearch").ToString();

                    if (schema.ContainsKey("prefixPage"))
                        prefixPage = schema.GetValue("prefixPage").ToString();

                    var isPage = schema.GetValue("page").ToObject<bool>();

                    if (isPage && prefixPage != null)
                        url = $"{url}{prefixSearch}={name}&{prefixPage}={page}";
                    else
                        url = $"{url}{prefixSearch}={name}";

                    doc = new HtmlWeb().Load(url);


                    var collection = schema.GetValue("collection").ToObject<JObject>();

                    var listBook = RipperSchema.GetValue(collection, doc);

                    var description = schema.GetValue("description").ToObject<JObject>();

                    foreach (var manga in listBook)
                    {
                        docBook.LoadHtml(manga.InnerHtml);

                        //get image cover
                        var imageUrlSchema = description.GetValue("imageUrl").ToObject<JObject>();
                        imageUrl = RipperSchema.GetValue(imageUrlSchema, docBook);

                        //url page
                        var urlPageSchema = description.GetValue("urlPage").ToObject<JObject>();
                        urlPage = RipperSchema.GetValue(urlPageSchema, docBook);

                        //name
                        var nameBookSchema = description.GetValue("name").ToObject<JObject>();
                        nameVideo = RipperSchema.GetValue(nameBookSchema, docBook);

                        listUrlVideo.Add(new GenericUrl
                        {
                            Name = RipperSchema.RemoveSpecialCharacters(nameVideo),
                            Url = urlPage,
                            UrlImage = imageUrl,
                            TypeView = "video"
                        });
                    }

                    if (!isPage)
                        break;
                }
                catch
                {
                    //not found other pages
                    break;
                }

                page++;
            }

            _logger.Info($"End download list video, search: {name}");
            return listUrlVideo;
        }
    }
}
