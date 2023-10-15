using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("episodeblacklist")]
    public class EpisodeBlacklist
    {
        [Primary]
        [Map("videoid")]
        public string Name { get; set; }

        [Map("url")]
        public string Url { get; set; }

        [Primary]
        [Map("namecfg")]
        public string NameCfg { get; set; }

        //convert GenericBlackListDTO to EpisodeBlacklist
        public static EpisodeBlacklist GenericQueueDTOToEpisodeBlacklist(GenericBlackListDTO blackList)
        {
            return new EpisodeBlacklist
            {
                Url = blackList.Url,
                NameCfg = blackList.NameCfg,
                Name = blackList.Name
            };
        }
    }
}
