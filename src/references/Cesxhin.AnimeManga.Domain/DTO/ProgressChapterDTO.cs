using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class ProgressChapterDTO
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string NameCfg { get; set; }
        public string NameChapter { get; set; }
        public int Page { get; set; }

        //convert ProgressEpisode to ProgressEpisodeDTO
        public static ProgressChapterDTO ProgressChapterToProgressChapterDTO(ProgressChapter progress)
        {
            return new ProgressChapterDTO
            {
                ID = progress.ID,
                Name = progress.Name,
                Page = progress.Page,
                Username = progress.Username,
                NameChapter = progress.NameChapter,
                NameCfg = progress.NameCfg,
            };
        }
    }
}