using Cesxhin.AnimeManga.Domain.DTO;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IProgressChapterService
    {
        //get
        public Task<ProgressChapterDTO> GetProgressByName(string name, string username, string nameCfg);

        //update
        public Task<ProgressChapterDTO> UpdateProgress(ProgressChapterDTO progress);
    }
}
