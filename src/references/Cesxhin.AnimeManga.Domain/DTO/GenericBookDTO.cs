using System.Collections.Generic;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class GenericBookDTO
    {
        //Manga
        public string Book { get; set; }
        public List<ChapterDTO> Chapters { get; set; }
        public List<ChapterRegisterDTO> ChapterRegister { get; set; }
    }
}
