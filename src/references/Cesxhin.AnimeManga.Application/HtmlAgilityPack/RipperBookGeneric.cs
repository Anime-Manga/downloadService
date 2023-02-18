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

            /*
            foreach (var volume in volumes)
            {
                HtmlNode[] chapters = null;
                try
                {
                    chapters = volume.SelectNodes("div[2]/div[@class='chapter']").ToArray();
                }
                catch (ArgumentNullException)
                {
                    chapters = volume.SelectNodes("div[@class='chapter pl-2']").ToArray();
                }


                var numberCurrentVolume = volume
                    .SelectNodes("div")
                    .First().InnerText;

                int numberVolume = 0;
                if (!numberCurrentVolume.Contains("Capitolo"))
                    numberVolume = int.Parse(numberCurrentVolume.Split(new char[] { ' ', '\n' })[1]);

                foreach (var chapter in chapters)
                {
                    var link = chapter
                        .SelectNodes("a")
                        .First()
                        .Attributes["href"].Value;
                    var currentChapter = chapter
                        .SelectNodes("a/span")
                        .First()
                        .InnerText;

                    var numberCurrentChapter = float.Parse(currentChapter.Split(new char[] { ' ', '\n' })[1]);

                    if (link.Contains("style=list"))
                        link = link.Replace("?style=list", "?style=pages");

                    var numberMaxImages = GetNumberMaxImage(link);

                    chaptersList.Add(new ChapterDTO
                    {
                        ID = $"{manga.Name}-v{numberVolume}-c{numberCurrentChapter}",
                        UrlPage = link,
                        NameManga = manga.Name,
                        CurrentChapter = numberCurrentChapter,
                        CurrentVolume = numberVolume,
                        NumberMaxImage = numberMaxImages
                    });
                }
            }
            return chaptersList;*/
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

        private static int GetNumberMaxImage(string urlPage)
        {
            /*
            var doc = GetMangaHtml(urlPage);
            var select = doc.DocumentNode
                .SelectNodes("//div/div/div/div/div/select[@class='page custom-select']")
                .First();

            var maxPage = select
                .SelectNodes("option")
                .Last()
                .Attributes["value"].Value;

            return int.Parse(maxPage);*/
            return 0;
        }

        public static byte[] GetImagePage(string urlPage, int page)
        {   /*
            //get image
            var webClient = new WebClient();

            if (urlPage.Contains("style=list"))
                urlPage = urlPage.Replace("?style=list", "?style=pages");

            string completeUrl;

            if (urlPage.Contains("?style="))
                completeUrl = urlPage.Replace("?style=", "/" + page + "?style=");
            else
                completeUrl = urlPage + "/" + page;

            var doc = GetMangaHtml(completeUrl);

            var imgUrl = doc.DocumentNode
                .SelectNodes("//div/div/img")
                .First()
                .Attributes["src"].Value;

            try
            {
                return webClient.DownloadData(imgUrl);
            }
            catch
            {
                _logger.Error($"Error download image from this url {imgUrl}");
                return null;
            }*/
            return null;
        }

        public static List<GenericUrl> GetMangaUrl(string name)
        {
            List<GenericUrl> listUrlManga = new();

            HtmlDocument doc;
            HtmlNode[] listManga;

            string url, imageUrl = null, urlPage = null, nameManga = null;

            var page = 1;
            while (true)
            {
                try
                {
                    url = $"https://www.mangaworld.in/archive?keyword={name}&page={page}";
                    doc = new HtmlWeb().Load(url);

                    listManga = doc.DocumentNode
                        .SelectNodes("//div/div/div/div[2]/div[@class='entry']")
                        .ToArray();

                    foreach (var manga in listManga)
                    {
                        //get image cover
                        imageUrl = manga
                            .SelectNodes("a/img")
                            .First()
                            .Attributes["src"].Value;

                        //url page
                        urlPage = manga
                            .SelectNodes("a")
                            .First()
                            .Attributes["href"].Value;

                        //name
                        nameManga = manga
                            .SelectNodes("div/p")
                            .First().InnerText;

                        listUrlManga.Add(new GenericUrl
                        {
                            Name = nameManga,
                            Url = urlPage,
                            UrlImage = imageUrl,
                            TypeView = "manga"
                        });
                    }
                }
                catch
                {
                    //not found other pages
                    return listUrlManga;
                }

                page++;
            }
        }
    }
}
