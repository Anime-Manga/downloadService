using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using RepoDb;
using System;
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
                try
                {
                    var result = await connection.QueryAsync<ProgressChapter>(e => e.Name == name && e.Username == username && e.NameCfg == nameCfg);

                    if (result == null || result.Count() <= 0)
                        return null;
                    return result.First();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CheckProgress, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<ProgressChapter> CreateProgress(ProgressChapter progress)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(progress);
                    return progress;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CreateProgress, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<ProgressChapter> UpdateProgress(ProgressChapter progress)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var result = await connection.UpdateAsync(progress, e => e.Name == progress.Name && e.Username == progress.Username && e.NameCfg == progress.NameCfg);

                    if (result <= 0)
                        return null;
                    return progress;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CheckProgress, details error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
