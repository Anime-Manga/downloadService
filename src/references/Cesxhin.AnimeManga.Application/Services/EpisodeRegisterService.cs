using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class EpisodeRegisterService : IEpisodeRegisterService
    {
        //interfaces
        private readonly IEpisodeRegisterRepository _episodeRegisterRepository;

        public EpisodeRegisterService(IEpisodeRegisterRepository episodeRegisterRepository)
        {
            _episodeRegisterRepository = episodeRegisterRepository;
        }

        //get episodeRegister by episode id
        public async Task<EpisodeRegisterDTO> GetObjectRegisterByObjectId(string id)
        {
            var episodeRegister = await _episodeRegisterRepository.GetObjectRegisterByObjectId(id);
            return EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(episodeRegister);
        }

        public async Task<List<EpisodeRegisterDTO>> GetObjectsRegistersByListObjectId(List<EpisodeDTO> listEpisodeDTO)
        {
            List<Episode> listEpisode = new();

            foreach(var episodeDTO in listEpisodeDTO)
            {
                listEpisode.Add(Episode.EpisodeDTOToEpisode(episodeDTO));
            }

            var rs = await _episodeRegisterRepository.GetObjectsRegistersByListObjectId(listEpisode);


            List<EpisodeRegisterDTO> listEpisodeRegisterDTO = new();

            foreach (var episodeRegister in rs.ToList())
            {
                listEpisodeRegisterDTO.Add(EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(episodeRegister));
            }

            return listEpisodeRegisterDTO;
        }

        //insert episodeRegister
        public async Task<EpisodeRegisterDTO> InsertObjectRegisterAsync(EpisodeRegisterDTO episodeRegister)
        {
            var rs = await _episodeRegisterRepository.InsertObjectRegisterAsync(EpisodeRegister.EpisodeRegisterToEpisodeRegisterDTO(episodeRegister));
            return EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(rs);
        }

        //insert list episodeRegister
        public async Task<IEnumerable<EpisodeRegisterDTO>> InsertObjectsRegistersAsync(List<EpisodeRegisterDTO> episodesRegistersDTO)
        {
            List<EpisodeRegisterDTO> resultEpisodes = new();
            foreach (var episode in episodesRegistersDTO)
            {
                var episodeResult = await _episodeRegisterRepository.InsertObjectRegisterAsync(EpisodeRegister.EpisodeRegisterToEpisodeRegisterDTO(episode));
                resultEpisodes.Add(EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(episodeResult));
            }
            return resultEpisodes;
        }

        //Update episodeRegister
        public async Task<EpisodeRegisterDTO> UpdateObjectRegisterAsync(EpisodeRegisterDTO episodeRegister)
        {
            var rs = await _episodeRegisterRepository.UpdateObjectRegisterAsync(EpisodeRegister.EpisodeRegisterToEpisodeRegisterDTO(episodeRegister));
            return EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(rs);
        }
    }
}
