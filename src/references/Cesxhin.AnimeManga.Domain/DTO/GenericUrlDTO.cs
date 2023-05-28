using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericUrlDTO
    {
        public string Name { get; set; }
        public string UrlPageDownload { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public bool Exists { get; set; } = false;

        //convert GenericUrl to GenericUrlDTO
        public static GenericUrlDTO GenericUrlToGenericUrlDTO(GenericUrl anime)
        {
            return new GenericUrlDTO
            {
                Name = anime.Name,
                UrlPageDownload = anime.Url,
                Image = anime.UrlImage,
                Type = anime.TypeView
            };
        }
    }
}
