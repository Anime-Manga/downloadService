using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.Models;
using NLog;
using Npgsql;
using RepoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    _logger.Error($"Failed GetChapterRegisterByChapterId, details error: {ex.Message}");
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

                    if (rs != null)
                        return rs.First();
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed GetChapterRegisterByChapterId, details error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
