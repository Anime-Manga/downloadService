using Cesxhin.AnimeManga.Modules.Exceptions;
using Cesxhin.AnimeManga.Modules.Generic;
using Cesxhin.AnimeManga.Modules.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using NLog;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Modules.CronJob
{
    public class SpaceDiskJob : IJob
    {
        private readonly string _folder = Environment.GetEnvironmentVariable("BASE_PATH") ?? "/";

        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());


        public Task Execute(IJobExecutionContext context)
        {
            Api<DiskSpaceDTO> checkDiskFreeSpaceApi = new();

            //check disk space free (byte to gigabyte)
            var freeGigabytes = new DriveInfo(_folder).AvailableFreeSpace / 1000000000;
            var totalGigabytes = new DriveInfo(_folder).TotalSize / 1000000000;

            try
            {
                checkDiskFreeSpaceApi.PutOne("/disk", new DiskSpaceDTO
                {
                    DiskSizeFree = freeGigabytes,
                    DiskSizeTotal = totalGigabytes,
                    Interval = 60000
                }).GetAwaiter().GetResult();

                _logger.Info("Ok send done status space disk");
            }
            catch (ApiGenericException ex)
            {
                _logger.Fatal($"Error send api DiskSpace, details error: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
