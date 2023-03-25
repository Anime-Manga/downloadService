using Cesxhin.AnimeManga.Domain.DTO;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IAccountService
    {
        public Task<AuthDTO> Login(string username, string password);
        public Task<AuthDTO> CreateAccount(string username, string password);
    }
}
