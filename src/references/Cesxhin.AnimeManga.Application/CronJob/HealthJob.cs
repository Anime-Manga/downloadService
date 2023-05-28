using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using NLog;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.CronJob
{
    public class HealthJob : IJob
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        public Task Execute(IJobExecutionContext context)
        {
            Api<HealthDTO> api = new();

            try
            {
                api.PutOne("/health", new HealthDTO
                {
                    NameService = context.JobDetail.Key.Name.ToLower(),
                    LastCheck = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds(),
                    Interval = 60000
                }).GetAwaiter().GetResult();

            }
            catch (ApiGenericException ex)
            {
                _logger.Fatal($"Error api, error details: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
