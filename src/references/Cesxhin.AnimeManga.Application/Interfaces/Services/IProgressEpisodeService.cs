using Cesxhin.AnimeManga.Domain.DTO;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IProgressEpisodeService
    {
        //get
        public Task<ProgressEpisodeDTO> GetProgressByName(string name, string username, string nameCfg);

        //update
        public Task<ProgressEpisodeDTO> UpdateProgress(ProgressEpisodeDTO progress);
    }
}
