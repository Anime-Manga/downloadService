using Cesxhin.AnimeManga.Application.Exceptions;
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
            return ProgressChapterDTO.ProgressChapterToProgressChapterDTO(result);
        }

        public async Task<ProgressChapterDTO> UpdateProgress(ProgressChapterDTO progress)
        {
            ProgressChapter result;

            try
            {
                await _progressChapterRepository.CheckProgress(progress.Name, progress.Username, progress.NameCfg);
                result = await _progressChapterRepository.UpdateProgress(ProgressChapter.ProgressChapterDTOToProgressChapter(progress));
            }
            catch (ApiNotFoundException)
            {
                result = await _progressChapterRepository.CreateProgress(ProgressChapter.ProgressChapterDTOToProgressChapter(progress));
            }

            return ProgressChapterDTO.ProgressChapterToProgressChapterDTO(result);

        }
    }
}
