using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        public Task<Auth> CreateAccount(Auth auth);

        public Task<Auth> findAccountByUsername(string username);
    }
}
