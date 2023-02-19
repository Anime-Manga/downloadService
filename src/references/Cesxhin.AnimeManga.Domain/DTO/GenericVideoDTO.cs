using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericVideoDTO
    {
        //Video
        public string Video { get; set; }
        public List<EpisodeDTO> Episodes { get; set; }
        public List<EpisodeRegisterDTO> EpisodesRegister { get; set; }
    }
}
