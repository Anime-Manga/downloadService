using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        public Task<Auth> CreateAccount(Auth auth);
        public Task<Auth> FindAccountByUsername(string username);

        //whitelist generic
        public Task<IEnumerable<WatchList>> GetListWatchListByUsername(string username);
        public Task<WatchList> InsertWhiteList(WatchList whiteList);
        public Task<WatchList> DeleteWhiteList(WatchList whiteList);
        public Task<bool> WhiteListCheckByName(string name);
    }
}
