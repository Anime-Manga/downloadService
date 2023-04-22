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
    public class ChapterRegisterRepository : IChapterRegisterRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<ChapterRegister> GetObjectRegisterByObjectId(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<ChapterRegister> rs;
                try
                {
                    rs = await connection.QueryAsync<ChapterRegister>(e => e.ChapterId == id);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChapterRegisterByChapterId, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs.First();
                else
                    throw new ApiNotFoundException("Not found GetObjectsRegisterByObjectId");
            }
        }

        public async Task<IEnumerable<ChapterRegister>> InsertObjectsRegisterAsync(List<ChapterRegister> chapterRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;
                try
                {
                    rs = await connection.InsertAllAsync(chapterRegister);

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertChapterRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return chapterRegister;
                else
                    throw new ApiNotFoundException("Not found InsertObjectRegisterAsync");
            }
        }

        public async Task<ChapterRegister> InsertObjectRegisterAsync(ChapterRegister chapterRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                object rs = null;
                try
                {
                    rs = await connection.InsertAsync(chapterRegister);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertChapterRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return chapterRegister;
                else
                    throw new ApiNotFoundException("Not found InsertObjectRegisterAsync");
            }
        }

        public async Task<ChapterRegister> UpdateObjectRegisterAsync(ChapterRegister chapterRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;
                try
                {
                    rs = await connection.UpdateAsync(chapterRegister, e => e.ChapterId == chapterRegister.ChapterId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateEpisodeRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return chapterRegister;
                else
                    throw new ApiNotFoundException("Not found UpdateObjectRegisterAsync");
            }
        }
    }
}
