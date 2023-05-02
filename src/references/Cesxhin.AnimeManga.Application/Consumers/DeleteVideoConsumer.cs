using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using MassTransit;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Consumers
{
    public class DeleteVideoConsumer : IConsumer<EpisodeRegisterDTO>
    {
        //nlog
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        public Task Consume(ConsumeContext<EpisodeRegisterDTO> context)
        {
            var episodeRegister = context.Message;

            try
            {
                File.Delete(episodeRegister.EpisodePath);
                _logger.Info($"Delete success! {episodeRegister.EpisodePath}");
            }
            catch (FileNotFoundException ex)
            {
                _logger.Warn($"File not found: {episodeRegister.EpisodePath}, details: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generic of file {episodeRegister.EpisodeId}, details: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
