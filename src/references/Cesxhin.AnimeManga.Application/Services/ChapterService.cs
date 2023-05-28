using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class ChapterService : IChapterService
    {
        //interfaces
        private readonly IChapterRepository _chapterRepository;

        public ChapterService(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        //get chapter by id
        public async Task<ChapterDTO> GetObjectByIDAsync(string id)
        {
            var rs = await _chapterRepository.GetObjectByIDAsync(id);
            return ChapterDTO.ChapterToChapterDTO(rs);
        }

        //get chapters by name
        public async Task<IEnumerable<ChapterDTO>> GetObjectsByNameAsync(string name)
        {
            var listChapter = await _chapterRepository.GetObjectsByNameAsync(name);

            List<ChapterDTO> chapters = new();
            foreach (var chapter in listChapter)
            {
                chapters.Add(ChapterDTO.ChapterToChapterDTO(chapter));
            }

            return chapters;
        }

        //insert one chapter
        public async Task<ChapterDTO> InsertObjectAsync(ChapterDTO chapter)
        {
            var chapterResult = await _chapterRepository.InsertObjectAsync(Chapter.ChapterDTOToChapter(chapter));
            return ChapterDTO.ChapterToChapterDTO(chapterResult);
        }

        //insert chapters
        public async Task<List<ChapterDTO>> InsertObjectsAsync(List<ChapterDTO> chapters)
        {
            List<Chapter> chaptersConvert = new();
            foreach (var chapter in chapters)
            {
                chaptersConvert.Add(Chapter.ChapterDTOToChapter(chapter));
            }

            var resultChapters = await _chapterRepository.InsertObjectsAsync(chaptersConvert);

            List<ChapterDTO> chaptersDTOConvert = new();
            foreach (var chapter in resultChapters)
            {
                chaptersDTOConvert.Add(ChapterDTO.ChapterToChapterDTO(chapter));
            }

            return chaptersDTOConvert;
        }

        //reset manual
        public async Task<ChapterDTO> ResetStatusDownloadObjectByIdAsync(ChapterDTO chapter)
        {
            var rs = await _chapterRepository.ResetStatusDownloadObjectByIdAsync(Chapter.ChapterDTOToChapter(chapter));
            return ChapterDTO.ChapterToChapterDTO(rs);
        }

        //reset automatic
        public async Task<List<ChapterDTO>> ResetStatusMultipleDownloadObjectByIdAsync(string name)
        {
            var listChapters = await _chapterRepository.GetObjectsByNameAsync(name);

            var resultChapters = await _chapterRepository.ResetStatusDownloadObjectsByIdAsync(listChapters.ToList());

            List<ChapterDTO> chaptersDTOConvert = new();
            foreach (var chapter in resultChapters)
            {
                chaptersDTOConvert.Add(ChapterDTO.ChapterToChapterDTO(chapter));
            }

            return chaptersDTOConvert;
        }

        //update PercentualState
        public async Task<ChapterDTO> UpdateStateDownloadAsync(ChapterDTO chapter)
        {
            var chapterResult = await _chapterRepository.UpdateStateDownloadAsync(Chapter.ChapterDTOToChapter(chapter));
            return ChapterDTO.ChapterToChapterDTO(chapterResult);
        }
    }
}
