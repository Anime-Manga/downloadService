using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class DescriptionService : IDescriptionService
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //interfaces
        private readonly IDescriptionRepository _descriptionRepository;
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IEpisodeRegisterRepository _episodeRegisterRepository;

        public DescriptionService(IDescriptionRepository descriptionRepository, IEpisodeRepository episodeRepository, IEpisodeRegisterRepository episodeRegisterRepository)
        {
            _descriptionRepository = descriptionRepository;
            _episodeRepository = episodeRepository;
            _episodeRegisterRepository = episodeRegisterRepository;
        }
        public async Task<string> DeleteNameByIdAsync(string name)
        {
            //check all finish downloaded

            //get anime
            var anime = await _descriptionRepository.GetNameByNameAsync(name);

            if (anime == null)
                return null;

            //get episodes
            var episodes = await _episodeRepository.GetObjectsByNameAsync(name);

            foreach (var episode in episodes)
            {
                if (!(episode.StateDownload == "completed" || episode.StateDownload == null))
                    return "-1";
            }

            var rs = await _descriptionRepository.DeleteNameAsync(name);
            var rsEpisode = await _episodeRepository.DeleteByNameAsync(name);

            if (rs <= 0 || rsEpisode <= 0)
                return null;

            return name;
        }

        public async Task<IEnumerable<JObject>> GetMostNameByNameAsync(string name)
        {
            return await _descriptionRepository.GetMostNameByNameAsync(name);
        }

        public async Task<IEnumerable<JObject>> GetNameAllAsync()
        {
            return await _descriptionRepository.GetNameAllAsync();
        }

        public async Task<IEnumerable<JObject>> GetNameAllWithAllAsync()
        {
            List<JObject> listGenericDTO = new();
            List<EpisodeDTO> listEpisodeDTO = new();
            List<EpisodeRegisterDTO> listEpisodeRegisterDTO = new();

            var listDescriptions = await _descriptionRepository.GetNameAllAsync();
            if (listDescriptions == null)
                return null;

            //anime
            foreach (var description in listDescriptions)
            {
                var episodes = await _episodeRepository.GetObjectsByNameAsync(description.GetValue("name_id").ToString());

                //episodes
                foreach (var episode in episodes)
                {
                    var episodesRegisters = await _episodeRegisterRepository.GetObjectsRegisterByObjectId(episode.ID);

                    //get first episodeRegister
                    foreach (var episodeRegister in episodesRegisters)
                        listEpisodeRegisterDTO.Add(EpisodeRegisterDTO.EpisodeRegisterToEpisodeRegisterDTO(episodeRegister));

                    listEpisodeDTO.Add(EpisodeDTO.EpisodeToEpisodeDTO(episode));
                }

                var objectAll = JObject.FromObject(new
                {
                    description,
                    listEpisodeDTO,
                    listEpisodeRegisterDTO
                });

                listGenericDTO.Add(objectAll);

                //reset
                listEpisodeDTO = new();
                listEpisodeRegisterDTO = new();
            }

            return listGenericDTO;
        }

        public async Task<JObject> GetNameByNameAsync(string name)
        {
            return await _descriptionRepository.GetNameByNameAsync(name);
        }

        public async Task<JObject> InsertNameAsync(JObject description)
        {
            if(description.ContainsKey("name_id"))
            {
                var find = await _descriptionRepository.GetNameByNameAsync(description.GetValue("name_id").ToString());

                if (find == null)
                    return await _descriptionRepository.InsertNameAsync(description);
                else
                    return null;
            }else
            {
                _logger.Error("Not found field 'name_id'");
                throw new Exception();
            }
        }
    }
}
