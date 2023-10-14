using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("chapterqueue")]
    public class ChapterQueue
    {
        [Primary]
        [Map("url")]
        public string Url { get; set; }

        [Primary]
        [Map("namecfg")]
        public string NameCfg { get; set; }

        [Map("timerequest")]
        public long TimeRequest { get; set; }


        //convert GenericQueueDTO to ChapterQueue
        public static ChapterQueue GenericQueueDTOToChapterQueue(GenericQueueDTO queue)
        {
            return new ChapterQueue
            {
                Url = queue.Url,
                NameCfg = queue.NameCfg,
                TimeRequest = queue.TimeRequest
            };
        }
    }
}
