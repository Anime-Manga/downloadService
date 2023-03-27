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
    public class AccountRepository : IAccountRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

        public async Task<Auth> CreateAccount(Auth auth)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(auth);
                    return auth;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CreateAccount, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<WatchList> DeleteWhiteList(WatchList whiteList)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.DeleteAsync(whiteList);

                    if (rs > 0)
                        return whiteList;
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed DeleteWhiteList, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<Auth> findAccountByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<Auth>(e => e.Username == username);

                    if (rs != null && rs.Count() > 0)
                        return rs.First();
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed findAccountByUsername, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<IEnumerable<WatchList>> GetListWatchListByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<WatchList>(e => e.Username == username);

                    if (rs != null && rs.Count() > 0)
                        return rs;
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetListWatchListByUsername, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<WatchList> InsertWhiteList(WatchList whiteList)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.InsertAsync(whiteList);
                    return whiteList;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertWhiteList, details error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<bool> whiteListCheckByName(string name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var rs = await connection.QueryAsync<WatchList>(e => e.Name == name);

                    if(rs != null && rs.Count() > 0)
                        return true;
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertWhiteList, details error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
