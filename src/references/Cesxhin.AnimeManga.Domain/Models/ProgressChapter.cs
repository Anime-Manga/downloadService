using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("progresschapter")]
    public class ProgressChapter
    {
        [Identity]
        [Map("id")]
        public string ID { get; set; }
        [Map("username")]
        public string Username { get; set; }
        [Map("name")]
        public string Name { get; set; }
        [Map("namecfg")]
        public string NameCfg { get; set; }
        [Map("namechapter")]
        public string NameChapter { get; set; }
        [Map("page")]
        public int Page { get; set; }

        //convert ProgressEpisodeDTO to ProgressEpisode
        public static ProgressChapter ProgressChapterDTOToProgressChapter(ProgressChapterDTO progress)
        {
            return new ProgressChapter
            {
                ID = progress.ID,
                Username = progress.Username,
                Name = progress.Name,
                Page = progress.Page,
                NameChapter = progress.NameChapter,
                NameCfg = progress.NameCfg,
            };
        }
    }
}
