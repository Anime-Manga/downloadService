using System.Collections.Generic;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericAnimeDTO
    {
        //Anime
        public AnimeDTO Anime { get; set; }
        public List<EpisodeDTO> Episodes { get; set; }
        public List<EpisodeRegisterDTO> EpisodeRegister { get; set; }
    }
}
