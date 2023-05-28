using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.NlogManager;
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
    public class DeleteBookConsumer : IConsumer<ChapterRegisterDTO>
    {
        //nlog
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        public Task Consume(ConsumeContext<ChapterRegisterDTO> context)
        {
            var chapterRegister = context.Message;

            foreach (var singlePath in chapterRegister.ChapterPath)
            {
                try
                {
                    File.Delete(singlePath);
                    _logger.Info($"Delete success! {singlePath}");
                }
                catch (FileNotFoundException ex)
                {
                    _logger.Warn($"File not found: {singlePath}, details: {ex}");
                }
                catch (DirectoryNotFoundException ex)
                {
                    _logger.Warn($"File not found: {singlePath}, details: {ex}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error generic of file {singlePath}, details: {ex}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
