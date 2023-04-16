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
                try
                {
                    var rs = await connection.DeleteAsync<Episode>(e => e.VideoId == id);
                    return rs;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetEpisodeByIDAsync, details error: {ex.Message}");
                    return 0;
                }
            }
        }

        //get episode by id
        public async Task<IEnumerable<Episode>> GetObjectsByIDAsync(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<Episode>(e => e.ID == id);
                    return ConvertGeneric<Episode>.ConvertIEnurableToListCollection(rs);
                }
                catch(Exception ex)
                {
                    _logger.Error($"Failed GetEpisodeByIDAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        //get episodes by name
        public async Task<IEnumerable<Episode>> GetObjectsByNameAsync(string name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<Episode>(e => e.VideoId == name);

                    //create list ienurable to list
                    var list = ConvertGeneric<Episode>.ConvertIEnurableToListCollection(rs);

                    //order by asc
                    list.Sort(delegate (Episode p1, Episode p2){ return p1.NumberEpisodeCurrent.CompareTo(p2.NumberEpisodeCurrent); });

                    return list;
                }
                catch(Exception ex)
                {
                    _logger.Error($"Failed GetEpisodesByNameAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        //insert episode
        public async Task<Episode> InsertObjectAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(episode);
                    return episode;
                }
                catch(Exception ex)
                {
                    _logger.Error($"Failed InsertEpisodeAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        //reset StatusDownlod to null
        public async Task<Episode> ResetStatusDownloadObjectByIdAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    int percentual = episode.PercentualDownload;
                    episode.PercentualDownload = 0;

                    await connection.UpdateAsync(episode, e=> e.StateDownload != "completed" && e.StateDownload != null && e.PercentualDownload == percentual && e.ID == episode.ID);
                    return episode;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetEpisodesByNameAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }

        //update percentualDownload
        public async Task<Episode> UpdateStateDownloadAsync(Episode episode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.UpdateAsync(episode);
                    return episode;
                }
                catch(Exception ex)
                {
                    _logger.Error($"Failed UpdateStateDownloadAsync, details error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
