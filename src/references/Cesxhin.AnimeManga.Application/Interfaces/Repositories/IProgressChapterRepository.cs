using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IProgressChapterRepository
    {
        public Task<ProgressChapter> UpdateProgress(ProgressChapter progress);
        public Task<ProgressChapter> CheckProgress(string name, string username, string nameCfg);
        public Task<ProgressChapter> CreateProgress(ProgressChapter progress);
    }
}
