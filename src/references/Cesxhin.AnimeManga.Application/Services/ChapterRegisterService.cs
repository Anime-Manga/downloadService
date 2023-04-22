using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class ChapterRegisterService : IChapterRegisterService
    {
        //interfaces
        private readonly IChapterRegisterRepository _chapterRegisterRepository;

        public ChapterRegisterService(IChapterRegisterRepository chapterRegisterRepository)
        {
            _chapterRegisterRepository = chapterRegisterRepository;
        }

        //get chapterRegister by chapter id
        public async Task<ChapterRegisterDTO> GetObjectRegisterByObjectId(string id)
        {
            var chapterRegister = await _chapterRegisterRepository.GetObjectRegisterByObjectId(id);
            return ChapterRegisterDTO.ChapterRegisterToChapterRegisterDTO(chapterRegister);
        }

        //insert chapterRegister
        public async Task<ChapterRegisterDTO> InsertObjectRegisterAsync(ChapterRegisterDTO chapterRegister)
        {
            var result = await _chapterRegisterRepository.InsertObjectRegisterAsync(ChapterRegister.ChapterRegisterDTOToChapterRegister(chapterRegister));
            return ChapterRegisterDTO.ChapterRegisterToChapterRegisterDTO(result);
        }

        //insert list chapterRegister
        public async Task<IEnumerable<ChapterRegisterDTO>> InsertObjectsRegistersAsync(List<ChapterRegisterDTO> chaptersRegisters)
        {
            List<ChapterRegister> chapterRegistersConvert = new();
            foreach (var chapter in chaptersRegisters)
            {
                chapterRegistersConvert.Add(ChapterRegister.ChapterRegisterDTOToChapterRegister(chapter));
            }

            var resultChapters = await _chapterRegisterRepository.InsertObjectsRegisterAsync(chapterRegistersConvert);


            List<ChapterRegisterDTO> chapterRegistersDTOConvert = new();
            foreach (var chapter in resultChapters)
            {
                chapterRegistersDTOConvert.Add(ChapterRegisterDTO.ChapterRegisterToChapterRegisterDTO(chapter));
            }

            return chapterRegistersDTOConvert;
        }

        //Update chapterRegister
        public async Task<ChapterRegisterDTO> UpdateObjectRegisterAsync(ChapterRegisterDTO chapterRegister)
        {
            var chapterResult = await _chapterRegisterRepository.UpdateObjectRegisterAsync(ChapterRegister.ChapterRegisterDTOToChapterRegister(chapterRegister));
            return ChapterRegisterDTO.ChapterRegisterToChapterRegisterDTO(chapterResult);
        }
    }
}
