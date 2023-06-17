using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("whitelist")]
    public class WatchList
    {
        [Primary]
        [Map("name")]
        public string Name { get; set; }

        [Primary]
        [Map("username")]
        public string Username { get; set; }

        [Primary]
        [Map("namecfg")]
        public string NameCfg { get; set; }

        //convert AuthDTO to Auth
        public static WatchList WatchListDTOToWatchList(WatchListDTO watchListDTO)
        {
            return new WatchList
            {
                Name = watchListDTO.Name,
                Username = watchListDTO.Username,
                NameCfg = watchListDTO.NameCfg,
            };
        }
    }
}
