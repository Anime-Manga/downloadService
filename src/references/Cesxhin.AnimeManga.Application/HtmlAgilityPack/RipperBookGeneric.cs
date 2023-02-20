using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;

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
                descriptionDB.Add(nameField.Key, result);
            }

            descriptionDB["url_page"] = urlPage;
            descriptionDB["name_id"] = RipperSchema.RemoveSpecialCharacters(descriptionDB.GetValue("name_id").ToString());

            _logger.Info($"End download page book: {urlPage}");

            return descriptionDB;
        }

        public static List<ChapterDTO> GetChapters(JObject schema, string urlPage, string name, string nameCfg)
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
                    var itemThread = item;
                    tasks.Add(new Func<ChapterDTO>(() => { return GetChapterRecursive(procedure, procedure.Count, 0, itemThread, name, nameCfg); }));
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
                    resultBooks.Add(GetChapterRecursive(procedure, procedure.Count, 0, item, name, nameCfg));
                }
            }

            _logger.Info($"End download page episode: {urlPage}");

            return resultBooks;
        }

        private static ChapterDTO GetChapterRecursive(JObject actualProcedure, int step, int current, string urlPage, string name, string nameCfg)
        {

            var stepSelect = actualProcedure[current.ToString()].ToObject<JObject>();

            var doc = new HtmlWeb().Load(urlPage);
            var newUrlPage = RipperSchema.GetValue(stepSelect, doc, 0, 0, name, nameCfg);

            //set step
            current += 1;

            if (current == step)
                return newUrlPage;

            return GetChapterRecursive(actualProcedure, step, current, newUrlPage, name, nameCfg);
        }

        public static byte[] GetImagePage(string urlPage, int page, ChapterDTO chapter)
        {
            JObject _schema = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));
            var schema = _schema.GetValue(chapter.NameCfg).ToObject<JObject>();
            var downloadSchema = schema.GetValue("book").ToObject<JObject>().GetValue("download").ToObject<JObject>();

            if(downloadSchema.ContainsKey("startZero") && downloadSchema.GetValue("startZero").ToObject<bool>() == true)
                downloadSchema["numberPage"] = page + 1;
            else
                downloadSchema["numberPage"] = page;

            var doc = new HtmlWeb().Load(urlPage);

            var imgUrl = RipperSchema.GetValue(downloadSchema, doc);

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

        public static List<GenericUrl> GetBookUrl(JObject schema, string name)
        {
            _logger.Info($"Start download list book, search: {name}");

            List<GenericUrl> listUrlBook = new();

            HtmlDocument doc;

            string url, prefixPage = null, prefixSearch, imageUrl = null, urlPage = null, nameBook = null;
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
                        nameBook = RipperSchema.GetValue(nameBookSchema, docBook);

                        listUrlBook.Add(new GenericUrl
                        {
                            Name = RipperSchema.RemoveSpecialCharacters(nameBook),
                            Url = urlPage,
                            UrlImage = imageUrl,
                            TypeView = "book"
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

            _logger.Info($"End download list book, search: {name}");

            return listUrlBook;
        }
    }
}
