using Cesxhin.AnimeManga.Domain.DTO;
using RepoDb.Attributes;

namespace Cesxhin.AnimeManga.Domain.Models
{
    [Map("progressepisode")]
    public class ProgressEpisode
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
        [Map("nameepisode")]
        public string NameEpisode { get; set; }
        [Map("hours")]
        public int Hours { get; set; }
        [Map("minutes")]
        public int Minutes { get; set; }
        [Map("seconds")]
        public int Seconds { get; set; }

        //convert ProgressChapterDTO to ProgressChapter
        public static ProgressEpisode ProgressEpisodeDTOToProgressEpisode(ProgressEpisodeDTO progress)
        {
            return new ProgressEpisode
            {
                ID = progress.ID,
                Username = progress.Username,
                Name = progress.Name,
                Hours = progress.Hours,
                Minutes = progress.Minutes,
                Seconds = progress.Seconds,
                NameEpisode = progress.NameEpisode,
                NameCfg = progress.NameCfg,
            };
        }
    }
}