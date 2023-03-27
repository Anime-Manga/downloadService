using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IGeneralNameService<TGenericDTO, TGeneralNameDTO>
    {
        //get
        Task<IEnumerable<TGeneralNameDTO>> GetNameAllAsync(string nameCfg, string username);
        Task<TGeneralNameDTO> GetNameByNameAsync(string nameCfg, string name, string username);
        Task<IEnumerable<TGeneralNameDTO>> GetMostNameByNameAsync(string nameCfg, string name, string username);
        Task<IEnumerable<TGenericDTO>> GetNameAllWithAllAsync(string nameCfg, string username);
        Task<IEnumerable<TGeneralNameDTO>> GetNameAllOnlyWatchListAsync(string nameCfg, string username);

        //insert
        Task<TGeneralNameDTO> InsertNameAsync(string nameCfg, TGeneralNameDTO anime);

        //delete
        Task<string> DeleteNameByIdAsync(string nameCfg, string id);
    }
}
