using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class WatchListDTO
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string NameCfg { get; set; }

        //convert Auth to AuthDTO
        public static WatchListDTO WatchListToWatchListDTO(WatchList watchList)
        {
            return new WatchListDTO
            {
                Name = watchList.Name,
                Username = watchList.Username,
                NameCfg = watchList.NameCfg
            };
        }
    }
}
