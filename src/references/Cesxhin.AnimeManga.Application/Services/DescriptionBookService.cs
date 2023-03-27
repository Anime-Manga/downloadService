using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Services
{
    public class DescriptionBookService : IDescriptionBookService
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //interfaces
        private readonly IDescriptionRepository _descriptionRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IChapterRegisterRepository _chapterRegisterRepository;
        private readonly IAccountService _accountService;

        public DescriptionBookService(IAccountService accountService, IDescriptionRepository descriptionRepository, IChapterRepository chapterRepository, IChapterRegisterRepository chapterRegisterRepository)
        {
            _descriptionRepository = descriptionRepository;
            _chapterRepository = chapterRepository;
            _chapterRegisterRepository = chapterRegisterRepository;
            _accountService = accountService;
        }
        public async Task<string> DeleteNameByIdAsync(string nameCfg, string name)
        {
            //check all finish downloaded

            //get anime
            var anime = await _descriptionRepository.GetNameByNameAsync(nameCfg, name);

            if (anime == null)
                return null;

            //get episodes
            var episodes = await _chapterRepository.GetObjectsByNameAsync(name);

            foreach (var episode in episodes)
            {
                if (!(episode.StateDownload == "completed" || episode.StateDownload == null))
                    return "-1";
            }

            var rs = await _descriptionRepository.DeleteNameAsync(nameCfg, name);
            var rsEpisode = await _chapterRepository.DeleteByNameAsync(name);

            if (rs <= 0 || rsEpisode <= 0)
                return null;

            return name;
        }

        public async Task<IEnumerable<JObject>> GetMostNameByNameAsync(string nameCfg, string name, string username)
        {
            var result = await _descriptionRepository.GetMostNameByNameAsync(nameCfg, name);
            var watchList = await _accountService.GetListWatchListByUsername(username);

            if (watchList != null)
            {
                result.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) => {
                        if (singleWatchList.Name == item["name_id"].ToString() && singleWatchList.NameCfg == item["nameCfg"].ToString())
                            return true;
                        return false;
                    });

                    if (filterWatchList.Count() > 0)
                        item["watchList"] = true;
                    else
                        item["watchList"] = false;
                });
            }

            return result;
        }

        public async Task<IEnumerable<JObject>> GetNameAllAsync(string nameCfg, string username)
        {
            var result = await _descriptionRepository.GetNameAllAsync(nameCfg);
            var watchList = await _accountService.GetListWatchListByUsername(username);

            if (watchList != null)
            {
                result.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) => {
                        if (singleWatchList.Name == item["name_id"].ToString() && singleWatchList.NameCfg == item["nameCfg"].ToString())
                            return true;
                        return false;
                    });

                    if (filterWatchList.Count() > 0)
                        item["watchList"] = true;
                    else
                        item["watchList"] = false;
                });
            }

            return result;
        }

        public async Task<IEnumerable<JObject>> GetNameAllOnlyWatchListAsync(string nameCfg, string username)
        {
            var watchList = await _accountService.GetListWatchListByUsername(username);

            var result = new List<JObject>();
            JObject resultFind;

            if (watchList != null)
            {
                foreach (var watch in watchList)
                {
                    if (watch.NameCfg == nameCfg)
                    {
                        resultFind = await _descriptionRepository.GetNameByNameAsync(watch.NameCfg, watch.Name);

                        if (resultFind != null)
                            result.Add(resultFind);
                    }
                }

                result.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) => {
                        if (singleWatchList.Name == item["name_id"].ToString() && singleWatchList.NameCfg == item["nameCfg"].ToString())
                            return true;
                        return false;
                    });

                    if (filterWatchList.Count() > 0)
                        item["watchList"] = true;
                    else
                        item["watchList"] = false;
                });
            }

            return result;
        }

        public async Task<IEnumerable<GenericBookDTO>> GetNameAllWithAllAsync(string nameCfg, string username)
        {
            List<GenericBookDTO> listGenericDTO = new();

            List<ChapterDTO> listEpisodeDTO = new();
            List<ChapterRegisterDTO> listEpisodeRegisterDTO = new();

            var listDescriptions = await _descriptionRepository.GetNameAllAsync(nameCfg);
            if (listDescriptions == null)
                return null;

            var watchList = await _accountService.GetListWatchListByUsername(username);
            if (watchList != null)
            {
                listDescriptions.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) => {
                        if (singleWatchList.Name == item["name_id"].ToString() && singleWatchList.NameCfg == item["nameCfg"].ToString())
                            return true;
                        return false;
                    });

                    if (filterWatchList.Count() > 0)
                        item["watchList"] = true;
                    else
                        item["watchList"] = false;
                });
            }

            //anime
            foreach (var description in listDescriptions)
            {
                var episodes = await _chapterRepository.GetObjectsByNameAsync(description.GetValue("name_id").ToString());

                //episodes
                foreach (var episode in episodes)
                {
                    var episodesRegisters = await _chapterRegisterRepository.GetObjectsRegisterByObjectId(episode.ID);

                    //get first episodeRegister
                    foreach (var episodeRegister in episodesRegisters)
                        listEpisodeRegisterDTO.Add(ChapterRegisterDTO.ChapterRegisterToChapterRegisterDTO(episodeRegister));

                    listEpisodeDTO.Add(ChapterDTO.ChapterToChapterDTO(episode));
                }

                listGenericDTO.Add(new GenericBookDTO
                {
                    Book = description.ToString(),
                    Chapters = listEpisodeDTO,
                    ChapterRegister = listEpisodeRegisterDTO
                });

                //reset
                listEpisodeDTO = new();
                listEpisodeRegisterDTO = new();
            }

            return listGenericDTO;
        }

        public async Task<JObject> GetNameByNameAsync(string nameCfg, string name, string username)
        {
            var result = await _descriptionRepository.GetNameByNameAsync(nameCfg, name);
            var watchList = await _accountService.GetListWatchListByUsername(username);
            if (watchList != null)
            {
                var filterWatchList = watchList.Where((singleWatchList) => {
                    if (singleWatchList.Name == result["name_id"].ToString() && singleWatchList.NameCfg == result["nameCfg"].ToString())
                        return true;
                    return false;
                });

                if (filterWatchList.Count() > 0)
                    result["watchList"] = true;
                else
                    result["watchList"] = false;
            }

            return result;
        }

        public async Task<JObject> InsertNameAsync(string nameCfg, JObject description)
        {
            if (description.ContainsKey("name_id"))
            {
                var find = await _descriptionRepository.GetNameByNameAsync(nameCfg, description.GetValue("name_id").ToString());

                if (find == null)
                    return await _descriptionRepository.InsertNameAsync(nameCfg, description);
                else
                    return null;
            }
            else
            {
                _logger.Error("Not found field 'name_id'");
                throw new Exception();
            }
        }
    }
}