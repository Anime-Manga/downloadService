using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using RepoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Persistence.Repositories
{
    public class ProgressChapterRepository : IProgressChapterRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<ProgressChapter> CheckProgress(string name, string username, string nameCfg)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<ProgressChapter> rs;
                try
                {
                    rs = await connection.QueryAsync<ProgressChapter>(e => e.Name == name && e.Username == username && e.NameCfg == nameCfg);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CheckProgress, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs.First();
                else
                    throw new ApiNotFoundException("Not found CheckProgress");
            }
        }

        public async Task<ProgressChapter> CreateProgress(ProgressChapter progress)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                object rs = null;

                try
                {
                    rs = await connection.InsertAsync(progress);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CreateProgress, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return progress;
                else
                    throw new ApiNotFoundException("Not found CreateProgress");
            }
        }

        public async Task<ProgressChapter> UpdateProgress(ProgressChapter progress)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.UpdateAsync(progress, e => e.Name == progress.Name && e.Username == progress.Username && e.NameCfg == progress.NameCfg);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateProgress, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return progress;
                else
                    throw new ApiNotFoundException("Not found UpdateProgress");
            }
        }
    }
}
