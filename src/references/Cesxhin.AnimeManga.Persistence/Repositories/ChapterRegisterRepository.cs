using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using RepoDb;
using System.Threading.Tasks;
using Cesxhin.AnimeManga.Application.Generic;

namespace Cesxhin.AnimeManga.Persistence.Repositories
{
    public class ChapterRegisterRepository : IChapterRegisterRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<List<ChapterRegister>> GetObjectsRegisterByObjectId(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<ChapterRegister>(e => e.ChapterId == id);
                    return ConvertGeneric<ChapterRegister>.ConvertIEnurableToListCollection(rs);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChapterRegisterByChapterId, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<ChapterRegister> InsertObjectRegisterAsync(ChapterRegister chapterRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(chapterRegister);
                    return chapterRegister;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertChapterRegisterAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<ChapterRegister> UpdateObjectRegisterAsync(ChapterRegister chapterRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.UpdateAsync(chapterRegister, e => e.ChapterId == chapterRegister.ChapterId);

                    //check update
                    if (rs > 0)
                        return chapterRegister;
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateEpisodeRegisterAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
