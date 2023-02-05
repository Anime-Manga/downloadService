using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using HtmlAgilityPack;
using m3uParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Cesxhin.AnimeManga.Application.HtmlAgilityPack
{
    public static class RipperVideoGeneric
    {
        //log
        private static readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //parallel
        private static readonly ParallelManager<EpisodeDTO> parallel = new();
        private static readonly ParallelManager<string> _parallel = new();

        public static JObject GetAnime(JObject schema, string urlPage)
        {
            _logger.Info($"Start download page anime: {urlPage}");

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

            descriptionDB["name_id"] = RemoveSpecialCharacters(descriptionDB.GetValue("name_id").ToString());

            _logger.Info($"End download page anime: {urlPage}");

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
        public static List<GenericUrl> GetAnimeUrl(string name)
        {
            _logger.Info($"Start download lsit anime, search: {name}");
            //get page
            HtmlDocument doc = new HtmlWeb().Load("https://www.animesaturn.cc/animelist?search=" + name);

            //get number find elements
            string results = doc.DocumentNode
                .SelectNodes("//div/div/span/span/b[2]")
                .First().InnerText;

            //int numberAnime = int.Parse(results);

            //get animes
            var animes = doc.DocumentNode
                .SelectNodes("//ul/li/div")
                .ToList();

            List<GenericUrl> animeUrl = new();
            foreach (var anime in animes)
            {
                try
                {
                    //get link page
                    var linkPage = anime
                        .SelectNodes("a")
                        .First()
                        .Attributes["href"].Value;

                    //get anime
                    var nameAnime = anime
                        .SelectNodes("div/h3/a")
                        .First().InnerText;

                    //get url
                    var linkCopertina = anime
                        .SelectNodes("a/img[2]")
                        .First()
                        .Attributes["src"].Value;

                    animeUrl.Add(new GenericUrl
                    {
                        Name = RemoveSpecialCharacters(nameAnime),
                        Url = linkPage,
                        UrlImage = linkCopertina,
                        TypeView = "anime"
                    });

                }
                catch { /*ignore other link a */ }
            }

            _logger.Info($"End download lsit anime, search: {name}");
            return animeUrl;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            //remove character special
            return Regex.Replace(str, "[^a-zA-Z0-9_() ]+", "", RegexOptions.Compiled);
        }
    }
}
