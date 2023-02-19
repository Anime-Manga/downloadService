using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using RepoDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Persistence.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<int> DeleteByNameAsync(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.DeleteAsync<Chapter>(e => e.NameManga == id);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChapterByIDAsync, details error: {ex.Message}");
                    return 0;
                }
            }
        }

        public async Task<IEnumerable<Chapter>> GetObjectsByIDAsync(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<Chapter>(e => e.ID == id);
                    return ConvertGeneric<Chapter>.ConvertIEnurableToListCollection(rs);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChapterByIDAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<IEnumerable<Chapter>> GetObjectsByNameAsync(string nameManga)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<Chapter>(e => e.NameManga == nameManga);

                    //create list ienurable to list
                    var list = ConvertGeneric<Chapter>.ConvertIEnurableToListCollection(rs);

                    //order by asc
                    list.Sort(delegate (Chapter p1, Chapter p2) { return p1.CurrentChapter.CompareTo(p2.CurrentChapter); });

                    return list;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChaptersByNameAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<Chapter> InsertObjectAsync(Chapter chapters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(chapters);
                    return chapters;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertChapterAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<Chapter> ResetStatusDownloadObjectByIdAsync(Chapter chapter)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.UpdateAsync(chapter, e => e.StateDownload != "completed" && e.PercentualDownload == chapter.PercentualDownload && e.ID == chapter.ID);
                    return chapter;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed ResetStatusDownloadChaptersByIdAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<Chapter> UpdateStateDownloadAsync(Chapter chapter)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.UpdateAsync(chapter);
                    return chapter;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateStateDownloadAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
