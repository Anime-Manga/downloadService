using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using RepoDb;
using RepoDb.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Persistence.Repositories
{
    public class EpisodeRegisterRepository : IEpisodeRegisterRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        //get all episodesRegisters
        public async Task<EpisodeRegister> GetObjectRegisterByObjectId(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<EpisodeRegister> rs;
                try
                {
                    rs = await connection.QueryAsync<EpisodeRegister>(e => e.EpisodeId == id);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetEpisodeRegisterByEpisodeId, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs.First();
                else
                    throw new ApiNotFoundException("Not found GetObjectsRegisterByObjectId");
            }
        }

        //insert
        public async Task<IEnumerable<EpisodeRegister>> InsertObjectsRegisterAsync(List<EpisodeRegister> episodeRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.InsertAllAsync(episodeRegister);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertEpisodeRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episodeRegister;
                else
                    throw new ApiNotFoundException("Not found InsertObjectRegisterAsync");
            }
        }

        public async Task<EpisodeRegister> InsertObjectRegisterAsync(EpisodeRegister episodeRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                object rs = null;

                try
                {
                    rs = await connection.InsertAsync(episodeRegister);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertEpisodeRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return episodeRegister;
                else
                    throw new ApiNotFoundException("Not found InsertObjectRegisterAsync");
            }
        }

        //update
        public async Task<EpisodeRegister> UpdateObjectRegisterAsync(EpisodeRegister episodeRegister)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.UpdateAsync(episodeRegister, e => e.EpisodeId == episodeRegister.EpisodeId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateEpisodeRegisterAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episodeRegister;
                else
                    throw new ApiNotFoundException("Not found UpdateObjectRegisterAsync");
            }
        }

        public async Task<IEnumerable<EpisodeRegister>> GetObjectsRegistersByListObjectId(List<Episode> listEpisodes)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<EpisodeRegister> rs;

                try
                {
                    rs = await connection.QueryAsync<EpisodeRegister>(new QueryField("episodeid", Operation.In, listEpisodes.Select(e => e.ID)));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetObjectsRegistersByListObjectId, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs;
                else
                    throw new ApiNotFoundException("Not found GetObjectsRegistersByListObjectId");
            }
        }
    }
}
