using Cesxhin.AnimeManga.Domain.Models;
using System;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericQueueDTO
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string NameCfg { get; set; }
        public long TimeRequest { get; set; } = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();

        //convert ChapterQueue to GenericQueueDTO
        public static GenericQueueDTO ChapterQueueToGenericQueueDTO(ChapterQueue queue)
        {
            return new GenericQueueDTO
            {
                Name = queue.Name,
                Url = queue.Url,
                NameCfg = queue.NameCfg,
                TimeRequest = queue.TimeRequest
            };
        }

        //convert EpisodeQueue to GenericQueueDTO
        public static GenericQueueDTO EpisodeQueueToGenericQueueDTO(EpisodeQueue queue)
        {
            return new GenericQueueDTO
            {
                Name = queue.Name,
                Url = queue.Url,
                NameCfg = queue.NameCfg,
                TimeRequest = queue.TimeRequest
            };
        }
    }
}
