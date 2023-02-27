using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
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
            var listChapter = await _chapterRepository.GetObjectsByIDAsync(id);
            foreach (var chapter in listChapter)
            {
                return ChapterDTO.ChapterToChapterDTO(chapter);
            }
            return null;
        }

        //get chapters by name
        public async Task<IEnumerable<ChapterDTO>> GetObjectsByNameAsync(string name)
        {
            List<ChapterDTO> chapters = new();
            var listChapter = await _chapterRepository.GetObjectsByNameAsync(name);

            if (listChapter == null)
                return null;

            foreach (var chapter in listChapter)
            {
                chapters.Add(ChapterDTO.ChapterToChapterDTO(chapter));
            }

            if (chapters.Count <= 0)
                return null;

            return chapters;
        }

        //insert one chapter
        public async Task<ChapterDTO> InsertObjectAsync(ChapterDTO chapter)
        {
            var chapterResult = await _chapterRepository.InsertObjectAsync(Chapter.ChapterDTOToChapter(chapter));

            if (chapterResult == null)
                return null;

            return ChapterDTO.ChapterToChapterDTO(chapterResult);
        }

        //insert chapters
        public async Task<List<ChapterDTO>> InsertObjectsAsync(List<ChapterDTO> chapters)
        {
            List<ChapterDTO> resultChapters = new();
            foreach (var chapter in chapters)
            {
                var chapterResult = await _chapterRepository.InsertObjectAsync(Chapter.ChapterDTOToChapter(chapter));
                resultChapters.Add(ChapterDTO.ChapterToChapterDTO(chapterResult));
            }
            return resultChapters;
        }

        //reset manual
        public async Task<ChapterDTO> ResetStatusDownloadObjectByIdAsync(ChapterDTO chapter)
        {
            var rs = await _chapterRepository.ResetStatusDownloadObjectByIdAsync(Chapter.ChapterDTOToChapter(chapter));

            if (rs == null)
                return null;

            return ChapterDTO.ChapterToChapterDTO(rs);
        }

        //reset automatic
        public async Task<List<ChapterDTO>> ResetStatusMultipleDownloadObjectByIdAsync(string name)
        {
            var listChapters = await _chapterRepository.GetObjectsByNameAsync(name);
            List<ChapterDTO> resultChapters = new();

            foreach (var chapter in listChapters)
            {
                chapter.StateDownload = null;
                chapter.PercentualDownload = 0;

                var result = await _chapterRepository.ResetStatusDownloadObjectByIdAsync(chapter);

                if(result != null)
                    resultChapters.Add(ChapterDTO.ChapterToChapterDTO(chapter));
            }

            return resultChapters;
        }

        //update PercentualState
        public async Task<ChapterDTO> UpdateStateDownloadAsync(ChapterDTO chapter)
        {
            var chapterResult = await _chapterRepository.UpdateStateDownloadAsync(Chapter.ChapterDTOToChapter(chapter));

            if (chapterResult == null)
                return null;

            return ChapterDTO.ChapterToChapterDTO(chapterResult);
        }
    }
}
