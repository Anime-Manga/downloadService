using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class ProgressChapterService : IProgressChapterService
    {
        public readonly IProgressChapterRepository _progressChapterRepository;
        public ProgressChapterService(IProgressChapterRepository progressChapterRepository)
        {
            _progressChapterRepository = progressChapterRepository;
        }

        public async Task<ProgressChapterDTO> GetProgressByName(string name, string username, string nameCfg)
        {
            var result = await _progressChapterRepository.CheckProgress(name, username, nameCfg);

            if (result == null)
                return null;

            return ProgressChapterDTO.ProgressChapterToProgressChapterDTO(result);
        }

        public async Task<ProgressChapterDTO> UpdateProgress(ProgressChapterDTO progress)
        {
            var search = await _progressChapterRepository.CheckProgress(progress.Name, progress.Username, progress.NameCfg);
            ProgressChapter result;

            if(search == null)
            {
                result = await _progressChapterRepository.CreateProgress(ProgressChapter.ProgressChapterDTOToProgressChapter(progress));
            }
            else
            {
                result = await _progressChapterRepository.UpdateProgress(ProgressChapter.ProgressChapterDTOToProgressChapter(progress));
            }

            if (result == null)
                return null;

            return ProgressChapterDTO.ProgressChapterToProgressChapterDTO(result);

        }
    }
}
