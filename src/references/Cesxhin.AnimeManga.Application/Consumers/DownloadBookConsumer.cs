using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.HtmlAgilityPack;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Application.Parallel;
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
        //const
        const int LIMIT_TIMEOUT = 10;

        //nlog
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //Instance Parallel
        private readonly ParallelManager<string> parallel = new();

        //api
        private readonly Api<ChapterDTO> chapterApi = new();
        private readonly Api<ChapterRegisterDTO> chapterRegisterApi = new();

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
                for (int i = 0; i < chapter.NumberMaxImage; i++)
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
                for (int i = 0; i < chapter.NumberMaxImage; i++)
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
            }

            var result = parallel.GetResultAndClear();

            if (result.)
            {
                //send end download
                episode.StateDownload = "failed";
                episode.PercentualDownload = 0;
                SendStatusDownloadAPIAsync(episode, episodeDTOApi);

                _logger.Error($"failed download {episode.VideoId} s{episode.NumberSeasonCurrent}-e{episode.NumberEpisodeCurrent}");
                return;
            }

            //end download
            chapter.PercentualDownload = 100;
            chapter.StateDownload = "completed";
            SendStatusDownloadAPIAsync(chapter);

            //get hash and update
            _logger.Info($"start calculate hash of chapter id: {chapter.ID}");
            List<string> listHash = new();
            for (int i = 0; i < chapter.NumberMaxImage; i++)
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

            _logger.Info($"Done download manga {chapter.NameManga} of volume {chapter.CurrentVolume} chapter {chapter.CurrentChapter}");
            return Task.CompletedTask;
        }

        private string Download(ChapterDTO chapter, string path, int currentImage)
        {
            byte[] imgBytes;
            int timeout = 0;
            while (true)
            {
                imgBytes = RipperBookGeneric.GetImagePage(chapter.UrlPage, currentImage, chapter);

                if (timeout >= LIMIT_TIMEOUT)
                {
                    _logger.Error($"Failed download, details: {chapter.UrlPage}");
                    return "failed";
                }
                else if (imgBytes == null)
                {
                    _logger.Warn($"The attempts remains: {LIMIT_TIMEOUT - timeout} for {chapter.UrlPage}");
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
