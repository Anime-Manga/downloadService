using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class EpisodeService : IEpisodeService
    {
        //interfaces
        private readonly IEpisodeRepository _episodeRepository;

        public EpisodeService(IEpisodeRepository episodeRepository)
        {
            _episodeRepository = episodeRepository;
        }

        //get episode by id
        public async Task<EpisodeDTO> GetObjectByIDAsync(string id)
        {
            var episode = await _episodeRepository.GetObjectByIDAsync(id);
            return EpisodeDTO.EpisodeToEpisodeDTO(episode);
        }

        //get episodes by name
        public async Task<IEnumerable<EpisodeDTO>> GetObjectsByNameAsync(string name)
        {
            var listEpisode = await _episodeRepository.GetObjectsByNameAsync(name);

            List<EpisodeDTO> episodes = new();
            foreach (var episode in listEpisode)
            {
                episodes.Add(EpisodeDTO.EpisodeToEpisodeDTO(episode));
            }

            return episodes;
        }

        //insert one episode
        public async Task<EpisodeDTO> InsertObjectAsync(EpisodeDTO episode)
        {
            var episodeResult = await _episodeRepository.InsertObjectAsync(new Episode().EpisodeDTOToEpisode(episode));
            return EpisodeDTO.EpisodeToEpisodeDTO(episodeResult);
        }

        //insert episodes
        public async Task<List<EpisodeDTO>> InsertObjectsAsync(List<EpisodeDTO> episodes)
        {
            List<EpisodeDTO> resultEpisodes = new();
            foreach (var episode in episodes)
            {
                var episodeResult = await _episodeRepository.InsertObjectAsync(new Episode().EpisodeDTOToEpisode(episode));
                resultEpisodes.Add(EpisodeDTO.EpisodeToEpisodeDTO(episodeResult));
            }
            return resultEpisodes;
        }

        //reset StatusDownload to null
        public async Task<EpisodeDTO> ResetStatusDownloadObjectByIdAsync(EpisodeDTO episode)
        {
            var episodeResult = await _episodeRepository.ResetStatusDownloadObjectByIdAsync(new Episode().EpisodeDTOToEpisode(episode));
            return EpisodeDTO.EpisodeToEpisodeDTO(episodeResult);
        }

        //reset all state
        public async Task<List<EpisodeDTO>> ResetStatusMultipleDownloadObjectByIdAsync(string name)
        {
            var listEpisodes = await _episodeRepository.GetObjectsByNameAsync(name);

            var resultEpisodes = await _episodeRepository.ResetStatusDownloadObjectsByIdAsync(listEpisodes.ToList());

            List<EpisodeDTO> episodesDTOConvert = new();
            foreach (var episode in resultEpisodes)
            {
                episodesDTOConvert.Add(EpisodeDTO.EpisodeToEpisodeDTO(episode));
            }

            return episodesDTOConvert;
        }

        //update PercentualState
        public async Task<EpisodeDTO> UpdateStateDownloadAsync(EpisodeDTO episode)
        {
            var episodeResult = await _episodeRepository.UpdateStateDownloadAsync(new Episode().EpisodeDTOToEpisode(episode));

            if (episodeResult == null)
                return null;

            return EpisodeDTO.EpisodeToEpisodeDTO(episodeResult);
        }
    }
}
