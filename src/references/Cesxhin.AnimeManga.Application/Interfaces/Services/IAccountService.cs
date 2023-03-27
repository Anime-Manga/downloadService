using Cesxhin.AnimeManga.Domain.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IAccountService
    {
        public Task<AuthDTO> Login(string username, string password);
        public Task<AuthDTO> CreateAccount(string username, string password);

        //whitelist generic
        public Task<IEnumerable<WatchListDTO>> GetListWatchListByUsername(string username);
        public Task<WatchListDTO> InsertWatchList(WatchListDTO whiteListDTO);
        public Task<WatchListDTO> DeleteWatchList(WatchListDTO whiteListDTO);
        public Task<bool> WatchListCheckByName(string name);
    }
}
