using Cesxhin.AnimeManga.Domain.Models;

namespace Cesxhin.AnimeManga.Domain.DTO
{
    public class ProgressEpisodeDTO
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string NameCfg { get; set; }
        public string NameEpisode { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        //convert ProgressChapter to ProgressChapterDTO
        public static ProgressEpisodeDTO ProgressEpisodeToProgressEpisodeDTO(ProgressEpisode progress)
        {
            return new ProgressEpisodeDTO
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