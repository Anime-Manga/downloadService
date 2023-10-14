using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("episodequeue")]
    public class EpisodeQueue
    {
        [Primary]
        [Map("url")]
        public string Url { get; set; }

        [Primary]
        [Map("namecfg")]
        public string NameCfg { get; set; }

        [Map("timerequest")]
        public long TimeRequest { get; set; }

        //convert GenericQueueDTO to EpisodeQueue
        public static EpisodeQueue GenericQueueDTOToEpisodeQueue(GenericQueueDTO queue)
        {
            return new EpisodeQueue
            {
                Url = queue.Url,
                NameCfg = queue.NameCfg,
                TimeRequest = queue.TimeRequest
            };
        }
    }
}
