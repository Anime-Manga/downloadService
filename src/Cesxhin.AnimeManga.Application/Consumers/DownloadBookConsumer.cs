﻿using Cesxhin.AnimeManga.Modules.Exceptions;
using Cesxhin.AnimeManga.Modules.Generic;
using Cesxhin.AnimeManga.Modules.HtmlAgilityPack;
using Cesxhin.AnimeManga.Modules.NlogManager;
using Cesxhin.AnimeManga.Modules.Parallel;
using Cesxhin.AnimeManga.Domain.DTO;
using MassTransit;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Consumers
{
    public class DownloadBookConsumer : IConsumer<ChapterDTO>
    {
        //nlog
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //Instance Parallel
        private readonly ParallelManager<string> parallel = new();

        //api
        private readonly Api<ChapterDTO> chapterApi = new();
        private readonly Api<ChapterRegisterDTO> chapterRegisterApi = new();

        //download
        private readonly int MAX_DELAY = int.Parse(Environment.GetEnvironmentVariable("MAX_DELAY") ?? "5");
        private readonly int DELAY_RETRY_ERROR = int.Parse(Environment.GetEnvironmentVariable("DELAY_RETRY_ERROR") ?? "10000");


        public Task Consume(ConsumeContext<ChapterDTO> context)
        {
            //get body
            var chapter = context.Message;

            //chapterRegister
            ChapterRegisterDTO chapterRegister = null;
            try
            {
                chapterRegister = chapterRegisterApi.GetOne($"/chapter/register/chapterid/{chapter.ID}").GetAwaiter().GetResult();
            }
            catch (ApiNotFoundException ex)
            {
                _logger.Error($"Not found episodeRegister, details error: {ex.Message}");
            }
            catch (ApiGenericException ex)
            {
                _logger.Fatal($"Impossible error generic get episodeRegister, details error: {ex.Message}");
            }

            //chapter
            ChapterDTO chapterVerify = null;
            try
            {
                chapterVerify = chapterApi.GetOne($"/chapter/id/{chapter.ID}").GetAwaiter().GetResult();
            }
            catch (ApiNotFoundException ex)
            {
                _logger.Error($"Not found episodeRegister, details error: {ex.Message}");
            }
            catch (ApiGenericException ex)
            {
                _logger.Fatal($"Impossible error generic get episodeRegister, details error: {ex.Message}");
            }

            //check duplication messages
            if (chapterVerify != null && chapterVerify.StateDownload == "pending")
            {
                _logger.Info($"Start download manga {chapter.NameManga} of volume {chapter.CurrentVolume} chapter {chapter.CurrentChapter}");

                //create empty file
                for (int i = 0; i <= chapter.NumberMaxImage; i++)
                {
                    //check directory
                    var pathWithoutFile = Path.GetDirectoryName(chapterRegister.ChapterPath[i]);
                    if (Directory.Exists(pathWithoutFile) == false)
                        Directory.CreateDirectory(pathWithoutFile);

                    File.WriteAllBytes(chapterRegister.ChapterPath[i], new byte[0]);
                }

                //set start download
                chapter.StateDownload = "downloading";
                SendStatusDownloadAPIAsync(chapter);

                //set parallel
                var tasks = new List<Func<string>>();

                //step one check file
                for (int i = 0; i <= chapter.NumberMaxImage; i++)
                {
                    var currentImage = i;
                    var path = chapterRegister.ChapterPath[currentImage];
                    tasks.Add(new Func<string>(() => Download(chapter, path, currentImage)));
                }
                parallel.AddTasks(tasks);
                parallel.Start();

                while (!parallel.CheckFinish())
                {
                    //send status download
                    chapter.PercentualDownload = parallel.PercentualCompleted();
                    SendStatusDownloadAPIAsync(chapter);
                    Thread.Sleep(3000);
                }

                var result = parallel.GetResultAndClear();

                if (result.Contains("failed"))
                {
                    //send failed download
                    chapter.StateDownload = "failed";
                    chapter.PercentualDownload = 0;
                    SendStatusDownloadAPIAsync(chapter);

                    _logger.Error($"failed download {chapter.ID} v{chapter.CurrentVolume}-c{chapter.CurrentChapter}");
                    throw new Exception($"failed download {chapter.ID} v{chapter.CurrentVolume}-c{chapter.CurrentChapter}");
                }

                //get hash and update
                _logger.Info($"start calculate hash of chapter id: {chapter.ID}");
                List<string> listHash = new();
                for (int i = 0; i <= chapter.NumberMaxImage; i++)
                {
                    listHash.Add(Hash.GetHash(chapterRegister.ChapterPath[i]));
                }
                _logger.Info($"end calculate hash of episode id: {chapter.ID}");

                chapterRegister.ChapterHash = listHash.ToArray();

                try
                {
                    chapterRegisterApi.PutOne("/chapter/register", chapterRegister).GetAwaiter().GetResult();
                }
                catch (ApiNotFoundException ex)
                {
                    _logger.Error($"Not found episodeRegister id: {chapterRegister.ChapterId}, details error: {ex.Message}");
                }
                catch (ApiGenericException ex)
                {
                    _logger.Fatal($"Error generic put episodeRegister, details error: {ex.Message}");
                }

                //end download
                chapter.PercentualDownload = 100;
                chapter.StateDownload = "completed";
                SendStatusDownloadAPIAsync(chapter);

                _logger.Info($"Done download manga {chapter.NameManga} of volume {chapter.CurrentVolume} chapter {chapter.CurrentChapter}");
            }
            else
            {
                _logger.Info($"This episode is already work by another, episode id: {chapter.ID}");
            }

            return Task.CompletedTask;
        }

        private string Download(ChapterDTO chapter, string path, int currentImage)
        {
            byte[] imgBytes;
            int timeout = 0;
            while (true)
            {
                imgBytes = RipperBookGeneric.GetImagePage(chapter.UrlPage, currentImage, chapter);

                if (timeout >= MAX_DELAY)
                {
                    _logger.Error($"Failed download, details: {chapter.UrlPage}");
                    return "failed";
                }
                else if (imgBytes == null)
                {
                    _logger.Warn($"The attempts remains: {MAX_DELAY - timeout} for {chapter.UrlPage}");
                    Task.Delay(DELAY_RETRY_ERROR);
                    timeout++;
                }
                else
                    break;

            }

            File.WriteAllBytes(path, imgBytes);

            return "done";
        }

        private void SendStatusDownloadAPIAsync(ChapterDTO chapter)
        {
            try
            {
                chapterApi.PutOne("/book/statusDownload", chapter).GetAwaiter().GetResult();
            }
            catch (ApiNotFoundException ex)
            {
                _logger.Error($"Not found episode id: {chapter.ID}, details: {ex.Message}");
            }
            catch (ApiGenericException ex)
            {
                _logger.Error($"Error generic api, details: {ex.Message}");
            }
        }
    }
}
