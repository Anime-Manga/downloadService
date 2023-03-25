using Cesxhin.AnimeManga.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
