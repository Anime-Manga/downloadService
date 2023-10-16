using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("chapterblacklist")]
    public class ChapterBlacklist
    {
        [Primary]
        [Map("namemanga")]
        public string Name { get; set; }

        [Map("url")]
        public string Url { get; set; }

        [Primary]
        [Map("namecfg")]
        public string NameCfg { get; set; }

        //convert GenericBlackListDTO to ChapterBlacklist
        public static ChapterBlacklist GenericQueueDTOToChapterBlacklist(GenericBlackListDTO blackList)
        {
            return new ChapterBlacklist
            {
                Url = blackList.Url,
                NameCfg = blackList.NameCfg,
                Name = blackList.Name
            };
        }
    }
}
