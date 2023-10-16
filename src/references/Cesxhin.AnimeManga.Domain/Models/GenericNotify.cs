namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericNotify
    {
        public string Message { get; set; }
        public string Image { get; set; }

        //convert NotifyAnimeDTO to GenericNotify
        public static GenericNotify NotifyAnimeDTOToGenericNotify(NotifyAnimeDTO notify)
        {
            return new GenericNotify
            {
                Message = notify.Message,
                Image = notify.Image
            };
        }

        //convert NotifyMangaDTO to GenericNotify
        public static GenericNotify NotifyMangaDTOToGenericNotify(NotifyMangaDTO notify)
        {
            return new GenericNotify
            {
                Message = notify.Message,
                Image = notify.Image
            };
        }

        //convert NotifyRequestAnimeDTO to GenericNotify
        public static GenericNotify NotifyRequestAnimeDTOToGenericNotify(NotifyRequestAnimeDTO notify)
        {
            return new GenericNotify
            {
                Message = notify.Message,
                Image = notify.Image
            };
        }

        //convert NotifyRequestMangaDTO to GenericNotify
        public static GenericNotify NotifyRequestMangaDTOToGenericNotify(NotifyRequestMangaDTO notify)
        {
            return new GenericNotify
            {
                Message = notify.Message,
                Image = notify.Image
            };
        }
    }
}
