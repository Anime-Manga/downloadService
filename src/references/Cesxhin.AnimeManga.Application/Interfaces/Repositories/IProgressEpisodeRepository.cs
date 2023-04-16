using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IProgressEpisodeRepository
    {
        public Task<ProgressEpisode> UpdateProgress(ProgressEpisode progress);
        public Task<ProgressEpisode> CheckProgress(string name, string username, string nameCfg);
        public Task<ProgressEpisode> CreateProgress(ProgressEpisode progress);
    }
}
