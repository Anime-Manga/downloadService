using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("whitelist")]
    public class WatchList
    {
        [Identity]
        [Map("name")]
        public string Name { get; set; }

        [Map("username")]
        public string Username { get; set; }

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
