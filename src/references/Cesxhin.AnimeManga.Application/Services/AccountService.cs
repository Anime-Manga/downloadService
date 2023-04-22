using Cesxhin.AnimeManga.Application.Exceptions;
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

        private static bool ValidPassword(string password, string hasedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hasedPassword);
        }

        private static string CryptPasword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<AuthDTO> CreateAccount(string username, string password)
        {
            try
            {
                await _accountRepository.FindAccountByUsername(username);
                throw new ApiConflictException("Conflict CreateAccount");
            }
            catch (ApiNotFoundException)
            {
                var auth = new Auth
                {
                    Username = username,
                    Password = password
                };

                auth.Password = CryptPasword(auth.Password);
                var authResult = await _accountRepository.CreateAccount(auth);

                return AuthDTO.AuthToAuthDTO(authResult);
            }
        }

        public async Task<AuthDTO> Login(string username, string password)
        {
            Auth findUser;
            try
            {
                findUser = await _accountRepository.FindAccountByUsername(username);
            }
            catch (ApiNotFoundException)
            {
                throw new ApiNotAuthorizeException("Not Authorize Login");
            }

            if (findUser != null && ValidPassword(password, findUser.Password))
                return AuthDTO.AuthToAuthDTO(findUser);
            else
                throw new ApiNotAuthorizeException("Not Authorize Login");

        }

        public async Task<WatchListDTO> InsertWatchList(WatchListDTO whiteListDTO)
        {
            try
            {
                await _accountRepository.WhiteListCheckByName(whiteListDTO.Name);
                throw new ApiConflictException("Conflict InsertWatchList");
            }
            catch (ApiNotFoundException)
            {
                var result = await _accountRepository.InsertWhiteList(WatchList.WatchListDTOToWatchList(whiteListDTO));
                return WatchListDTO.WatchListToWatchListDTO(result);
            }
        }

        public async Task<WatchListDTO> DeleteWatchList(WatchListDTO whiteListDTO)
        {
            var result = await _accountRepository.DeleteWhiteList(WatchList.WatchListDTOToWatchList(whiteListDTO));
            return WatchListDTO.WatchListToWatchListDTO(result);
        }

        public async Task<bool> WatchListCheckByName(string name)
        {
            return await _accountRepository.WhiteListCheckByName(name);
        }

        public async Task<IEnumerable<WatchListDTO>> GetListWatchListByUsername(string username)
        {
            var result = await _accountRepository.GetListWatchListByUsername(username);

            var resultArray = new List<WatchListDTO>();

            foreach (var item in result)
            {
                resultArray.Add(WatchListDTO.WatchListToWatchListDTO(item));
            }

            return resultArray;
        }
    }
}
