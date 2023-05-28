using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class AuthDTO
    {
        public string Username { get; set; }

        //convert Auth to AuthDTO
        public static AuthDTO AuthToAuthDTO(Auth auth)
        {
            return new AuthDTO
            {
                Username = auth.Username
            };
        }
    }
}
