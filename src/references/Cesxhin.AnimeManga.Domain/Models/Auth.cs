using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("account")]
    public class Auth
    {
        [Primary]
        [Map("username")]
        public string Username { get; set; }

        [Map("password")]
        public string Password { get; set; }

        //convert AuthDTO to Auth
        public static Auth AuthDTOToAuth(AuthDTO auth)
        {
            return new Auth
            {
                Username = auth.Username
            };
        }
    }
}
