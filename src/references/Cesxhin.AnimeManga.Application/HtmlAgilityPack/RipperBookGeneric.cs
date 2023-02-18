using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;

namespace Cesxhin.AnimeManga.Application.HtmlAgilityPack
{
    public static class RipperBookGeneric
    {
        //log
        private static readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //parallel
        private static readonly ParallelManager<ChapterDTO> parallel = new();

        public static JObject GetDescriptionBook(JObject schema, string urlPage)
        {
            _logger.Info($"Start download page book: {urlPage}");

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

                if (result == null)
                    descriptionDB.Add(nameField.Key, null);
                else if (int.TryParse(result, out intResult))
                    descriptionDB.Add(nameField.Key, intResult);
                else
                    descriptionDB.Add(nameField.Key, result.Trim().Replace(" +", ""));
            }

            descriptionDB["name_id"] = RipperSchema.RemoveSpecialCharacters(descriptionDB.GetValue("name_id").ToString());

            _logger.Info($"End download page book: {urlPage}");

            return descriptionDB;
        }

        public static List<ChapterDTO> GetChapters(JObject schema, string urlPage, string name)
        {
            List<ChapterDTO> chaptersList = new();

            _logger.Info($"Start download chapters manga: {urlPage}");

            //collection
            var collection = schema.GetValue("book").ToObject<JObject>().GetValue("collection").ToObject<JObject>();
            HtmlDocument doc = new HtmlWeb().Load(urlPage);

            var results = RipperSchema.GetValue(collection, doc);

            //procedure
            var procedure = schema.GetValue("book").ToObject<JObject>().GetValue("procedure").ToObject<JObject>();
            var resultBooks = new List<ChapterDTO>();

            var reverseCount = schema.GetValue("book").ToObject<JObject>().GetValue("collection").ToObject<JObject>().GetValue("reverseCount").ToObject<bool>();

            if (collection.GetValue("thread").ToObject<bool>() == true)
            {
                List<Func<ChapterDTO>> tasks = new();

                foreach (var item in results)
                {
                    var currentItem = item;
                    tasks.Add(new Func<ChapterDTO>(() => { return GetChapterRecursive(procedure, procedure.Count, 0, currentItem, name); }));
                }

                //when finish
                parallel.AddTasks(tasks);
                parallel.Start();
                parallel.WhenCompleted();
                resultBooks = parallel.GetResultAndClear();
            }
            else
            {
                foreach (var item in results)
                {
                    resultBooks.Add(GetChapterRecursive(procedure, procedure.Count, 0, item, name));
                }
            }

            _logger.Info($"End download page episode: {urlPage}");

            return resultBooks;
        }

        private static ChapterDTO GetChapterRecursive(JObject actualProcedure, int step, int current, string urlPage, string name)
        {

            var stepSelect = actualProcedure[current.ToString()].ToObject<JObject>();

            var doc = new HtmlWeb().Load(urlPage);
            var newUrlPage = RipperSchema.GetValue(stepSelect, doc, 0, 0, name);

            //set step
            current += 1;

            if (current == step)
                return newUrlPage;

            return GetChapterRecursive(actualProcedure, step, current, newUrlPage, name);
        }

        public static byte[] GetImagePage(string urlPage, int page, ChapterDTO chapter)
        {
            JObject _schema = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));
            var schema = _schema.GetValue(chapter.NameCfg).ToObject<JObject>();
            var download = schema.GetValue("download").ToObject<JObject>();

            var path = download.GetValue("path").ToString();

            download["path"] = path.Replace("{numberPage}", page.ToString());

            var doc = new HtmlWeb().Load(urlPage);

            var imgUrl = RipperSchema.GetValue(download, doc);

            using (var webClient = new WebClient())
            {
                try
                {
                    return webClient.DownloadData(imgUrl);
                }
                catch
                {
                    _logger.Error($"Error download image from this url {imgUrl}");
                    return null;
                }
            }
        }
    }
}
