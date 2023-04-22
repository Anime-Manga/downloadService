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
                object rs = null;
                try
                {
                    rs = await connection.InsertAsync(auth);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed CreateAccount, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return auth;
                else
                    throw new ApiNotFoundException("Not found CreateAccount");
            }
        }

        public async Task<WatchList> DeleteWhiteList(WatchList whiteList)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                int rs = 0;
                try
                {
                    rs = await connection.DeleteAsync(whiteList);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed DeleteWhiteList, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs > 0)
                    return whiteList;
                else
                    throw new ApiNotFoundException("Not found DeleteWhiteList");
            }
        }

        public async Task<Auth> FindAccountByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<Auth> rs;
                try
                {
                    rs = await connection.QueryAsync<Auth>(e => e.Username == username);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed findAccountByUsername, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs.First();
                else
                    throw new ApiNotFoundException("Not found findAccountByUsername");
            }
        }

        public async Task<IEnumerable<WatchList>> GetListWatchListByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<WatchList> rs;
                try
                {
                    rs = await connection.QueryAsync<WatchList>(e => e.Username == username);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetListWatchListByUsername, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return rs;
                else
                    throw new ApiNotFoundException("Not found GetListWatchListByUsername");
            }
        }

        public async Task<WatchList> InsertWhiteList(WatchList whiteList)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                object rs = null;
                try
                {
                    rs = await connection.InsertAsync(whiteList);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertWhiteList, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && !string.IsNullOrEmpty(rs.ToString()))
                    return whiteList;
                else
                    throw new ApiNotFoundException("Not found InsertWhiteList");
            }
        }

        public async Task<bool> WhiteListCheckByName(string name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                IEnumerable<WatchList> rs;
                try
                {
                    rs = await connection.QueryAsync<WatchList>(e => e.Name == name);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed InsertWhiteList, details error: {ex.Message}");
                    throw new ApiGenericException(ex.Message);
                }

                if (rs != null && rs.Any())
                    return true;
                else
                    throw new ApiNotFoundException("Not found InsertWhiteList");
            }
        }
    }
}
