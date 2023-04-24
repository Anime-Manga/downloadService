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
    public class EpisodeRepository : IEpisodeRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<int> DeleteByNameAsync(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.DeleteAsync<Episode>(e => e.VideoId == id);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed DeleteByNameAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return rs;
                else
                    throw new ApiNotFoundException("Not found DeleteByNameAsync");
            }
        }

        //get episode by id
        public async Task<Episode> GetObjectByIDAsync(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<Episode> rs;

                try
                {
                    rs = await connection.QueryAsync<Episode>(e => e.ID == id);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetObjectByIDAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs.First();
                else
                    throw new ApiNotFoundException("Not found GetObjectByIDAsync");
            }
        }

        //get episodes by name
        public async Task<IEnumerable<Episode>> GetObjectsByNameAsync(string name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<Episode> rs;

                try
                {
                    rs = await connection.QueryAsync<Episode>(e => e.VideoId == name, orderBy: OrderField.Parse(new
                    {
                        numberepisodecurrent = Order.Ascending
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetObjectsByNameAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs;
                else
                    throw new ApiNotFoundException("Not found GetObjectsByNameAsync");
            }
        }

        //insert episode
        public async Task<Episode> InsertObjectAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                object rs = null;

                try
                {
                    rs = await connection.InsertAsync(episode);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertObjectAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return episode;
                else
                    throw new ApiNotFoundException("Not found InsertObjectAsync");
            }
        }

        public async Task<IEnumerable<Episode>> InsertObjectsAsync(List<Episode> episodes)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.InsertAllAsync(episodes);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertObjectsAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episodes;
                else
                    throw new ApiNotFoundException("Not found InsertObjectsAsync");
            }
        }

        //reset StatusDownlod to null
        public async Task<Episode> ResetStatusDownloadObjectByIdAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                episode.PercentualDownload = 0;
                episode.StateDownload = null;

                try
                {

                    rs = await connection.UpdateAsync(episode, e => e.ID == episode.ID);

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed ResetStatusDownloadObjectByIdAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episode;
                else
                    throw new ApiNotFoundException("Not found ResetStatusDownloadObjectByIdAsync");
            }
        }

        public async Task<IEnumerable<Episode>> ResetStatusDownloadObjectsByIdAsync(List<Episode> episodes)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    episodes.ForEach((episode) =>
                    {
                        episode.PercentualDownload = 0;
                        episode.StateDownload = null;
                    });

                    rs = await connection.UpdateAllAsync(episodes, e => e.StateDownload == "failed");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed ResetStatusDownloadObjectsByIdAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episodes;
                else
                    throw new ApiNotFoundException("Not found ResetStatusDownloadObjectsByIdAsync");
            }
        }

        //update percentualDownload
        public async Task<Episode> UpdateStateDownloadAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;

                try
                {
                    rs = await connection.UpdateAsync(episode);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed UpdateStateDownloadAsync, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return episode;
                else
                    throw new ApiNotFoundException("Not found UpdateStateDownloadAsync");
            }
        }
    }
}
