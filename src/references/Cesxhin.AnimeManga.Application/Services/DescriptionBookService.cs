using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.Interfaces.Services;
using Cesxhin.AnimeManga.Application.NlogManager;
using Cesxhin.AnimeManga.Domain.DTO;
using Newtonsoft.Json.Linq;
using NLog;
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
            //get episodes
            var episodes = await _chapterRepository.GetObjectsByNameAsync(name);

            foreach (var episode in episodes)
            {
                if (!(episode.StateDownload == "completed" || episode.StateDownload == null))
                    throw new ApiConflictException("Conflict DeleteNameByIdAsync");
            }

            try
            {
                await _descriptionRepository.DeleteNameAsync(nameCfg, name);
            }
            catch (ApiNotFoundException)
            {
                _logger.Warn($"I tried delete description book name: {name} nameCfg: {nameCfg}");
            }

            try
            {
                await _chapterRepository.DeleteByNameAsync(name);
            }
            catch (ApiNotFoundException)
            {
                _logger.Warn($"I tried delete chapters name: {name} nameCfg: {nameCfg}");
            }

            return name;
        }

        public async Task<IEnumerable<JObject>> GetMostNameByNameAsync(string nameCfg, string name, string username)
        {
            var result = await _descriptionRepository.GetMostNameByNameAsync(nameCfg, name);

            try
            {
                var watchList = await _accountService.GetListWatchListByUsername(username);
                result.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) =>
                    {
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
            catch (ApiNotFoundException) { }

            return result;
        }

        public async Task<IEnumerable<JObject>> GetNameAllAsync(string nameCfg, string username)
        {
            var result = await _descriptionRepository.GetNameAllAsync(nameCfg);

            try
            {
                var watchList = await _accountService.GetListWatchListByUsername(username);

                result.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) =>
                    {
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
            catch (ApiNotFoundException) { }

            return result;
        }

        public async Task<IEnumerable<JObject>> GetNameAllOnlyWatchListAsync(string nameCfg, string username)
        {
            var watchList = await _accountService.GetListWatchListByUsername(username);

            var result = new List<JObject>();
            JObject resultFind;

            foreach (var watch in watchList)
            {
                if (watch.NameCfg == nameCfg)
                {
                    try
                    {
                        resultFind = await _descriptionRepository.GetNameByNameAsync(watch.NameCfg, watch.Name);

                        if (resultFind != null)
                            result.Add(resultFind);
                    }
                    catch (ApiNotFoundException) { }
                }
            }

            result.ForEach(item =>
            {
                var filterWatchList = watchList.Where((singleWatchList) =>
                {
                    if (singleWatchList.Name == item["name_id"].ToString() && singleWatchList.NameCfg == item["nameCfg"].ToString())
                        return true;
                    return false;
                });

                if (filterWatchList.Count() > 0)
                    item["watchList"] = true;
                else
                    item["watchList"] = false;
            });

            return result;
        }

        public async Task<IEnumerable<GenericBookDTO>> GetNameAllWithAllAsync(string nameCfg, string username)
        {
            List<GenericBookDTO> listGenericDTO = new();
            List<ChapterDTO> listEpisodeDTO = new();
            List<ChapterRegisterDTO> listEpisodeRegisterDTO = new();

            var listDescriptions = await _descriptionRepository.GetNameAllAsync(nameCfg);

            try
            {
                var watchList = await _accountService.GetListWatchListByUsername(username);

                listDescriptions.ForEach(item =>
                {
                    var filterWatchList = watchList.Where((singleWatchList) =>
                    {
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
            catch (ApiNotFoundException) { }

            //anime
            foreach (var description in listDescriptions)
            {
                var episodes = await _chapterRepository.GetObjectsByNameAsync(description.GetValue("name_id").ToString());

                //episodes
                foreach (var episode in episodes)
                {
                    var episodeRegister = await _chapterRegisterRepository.GetObjectRegisterByObjectId(episode.ID);

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

            try
            {
                var watchList = await _accountService.GetListWatchListByUsername(username);

                var filterWatchList = watchList.Where((singleWatchList) =>
                {
                    if (singleWatchList.Name == result["name_id"].ToString() && singleWatchList.NameCfg == result["nameCfg"].ToString())
                        return true;
                    return false;
                });

                if (filterWatchList.Count() > 0)
                    result["watchList"] = true;
                else
                    result["watchList"] = false;
            }
            catch (ApiNotFoundException) { }

            return result;
        }

        public async Task<JObject> InsertNameAsync(string nameCfg, JObject description)
        {
            if (description.ContainsKey("name_id"))
            {
                try
                {
                    await _descriptionRepository.GetNameByNameAsync(nameCfg, description.GetValue("name_id").ToString());
                    throw new ApiConflictException();
                }
                catch (ApiNotFoundException)
                {
                    return await _descriptionRepository.InsertNameAsync(nameCfg, description);
                }
            }
            else
            {
                _logger.Error("Not found field 'name_id' of book");
                throw new ApiNotFoundException("Not found field 'name_id' of book");
            }
        }

        public async Task<JObject> UpdateNameAsync(string nameCfg, JObject description)
        {
            if (description.ContainsKey("name_id"))
            {
                await _descriptionRepository.GetNameByNameAsync(nameCfg, description.GetValue("name_id").ToString());
                return await _descriptionRepository.UpdateNameAsync(nameCfg, description);
            }
            else
            {
                _logger.Error("Not found field 'name_id' of book");
                throw new ApiNotFoundException("Not found field 'name_id' of book");
            }
        }
    }
}