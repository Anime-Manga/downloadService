using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericBlackListDTO
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string NameCfg { get; set; }

        //convert EpisodeBlackList to GenericBlackListDTO
        public static GenericBlackListDTO EpisodeBlackListToGenericBlackListDTO(EpisodeBlacklist auth)
        {
            return new GenericBlackListDTO
            {
                Name = auth.Name,
                Url = auth.Url,
                NameCfg = auth.NameCfg,
            };
        }

        //convert ChapterBlackList to GenericBlackListDTO
        public static GenericBlackListDTO ChapterBlackListToGenericBlackListDTO(ChapterBlacklist auth)
        {
            return new GenericBlackListDTO
            {
                Name = auth.Name,
                Url = auth.Url,
                NameCfg = auth.NameCfg,
            };
        }
    }
}
