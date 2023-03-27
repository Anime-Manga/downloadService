using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
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

        private static bool validPassword(string password, string hasedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hasedPassword);
        }

        private static string cryptPasword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
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

        public async Task<WatchListDTO> InsertWatchList(WatchListDTO whiteListDTO)
        {
            if(!await _accountRepository.whiteListCheckByName(whiteListDTO.Name))
            {
                var result = await _accountRepository.InsertWhiteList(WatchList.WatchListDTOToWatchList(whiteListDTO));
                if (result != null)
                    return WatchListDTO.WatchListToWatchListDTO(result);
            }
            return null;
        }

        public async Task<WatchListDTO> DeleteWatchList(WatchListDTO whiteListDTO)
        {
            var result = await _accountRepository.DeleteWhiteList(WatchList.WatchListDTOToWatchList(whiteListDTO));

            if (result != null)
                return WatchListDTO.WatchListToWatchListDTO(result);
            return null;
        }

        public async Task<bool> WatchListCheckByName(string name)
        {
            return await _accountRepository.whiteListCheckByName(name);
        }

        public async Task<IEnumerable<WatchListDTO>> GetListWatchListByUsername(string username)
        {
            if(username != null)
            {
                var result = await _accountRepository.GetListWatchListByUsername(username);

                if(result != null)
                {
                    var resultArray = new List<WatchListDTO>();

                    foreach(var item in result)
                    {
                        resultArray.Add(WatchListDTO.WatchListToWatchListDTO(item));
                    }

                    return resultArray;
                }
            }
            return null;
        }
    }
}
