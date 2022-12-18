using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class ChapterRegisterDTO
    {
        public string ChapterId { get; set; }
        public string[] ChapterPath { get; set; }
        public string[] ChapterHash { get; set; }

        //convert ChapterRegister to ChapterRegisterDTO
        public static ChapterRegisterDTO ChapterRegisterToChapterRegisterDTO(ChapterRegister chapter)
        {
            return new ChapterRegisterDTO
            {
                ChapterId = chapter.ChapterId,
                ChapterPath = chapter.ChapterPath,
                ChapterHash = chapter.ChapterHash
            };
        }
    }
}
