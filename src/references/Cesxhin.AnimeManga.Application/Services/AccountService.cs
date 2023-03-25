using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class AccountService : IAccountService
    {
        public readonly IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AuthDTO> CreateAccount(string username, string password)
        {
            if(await _accountRepository.findAccountByUsername(username) == null)
            {
                var auth = new Auth
                {
                    Username = username,
                    Password = password
                };

               auth.Password = cryptPasword(auth.Password);
               var authResult = await _accountRepository.CreateAccount(auth);

               return AuthDTO.AuthToAuthDTO(authResult);
            }

            return null;
        }

        public async Task<AuthDTO> Login(string username, string password)
        {
            var findUser = await _accountRepository.findAccountByUsername(username);

            if (findUser != null && validPassword(password, findUser.Password))
                return AuthDTO.AuthToAuthDTO(findUser);

            return null;
        }

        private static bool validPassword(string password, string hasedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hasedPassword);
        }

        private static string cryptPasword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
