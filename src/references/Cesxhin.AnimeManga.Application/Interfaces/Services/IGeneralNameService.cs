using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IGeneralNameService<TGenericDTO, TGeneralNameDTO>
    {
        //get
        Task<IEnumerable<TGeneralNameDTO>> GetNameAllAsync(string nameCfg);
        Task<TGeneralNameDTO> GetNameByNameAsync(string nameCfg, string name);
        Task<IEnumerable<TGeneralNameDTO>> GetMostNameByNameAsync(string nameCfg, string name);
        Task<IEnumerable<TGenericDTO>> GetNameAllWithAllAsync(string nameCfg);

        //insert
        Task<TGeneralNameDTO> InsertNameAsync(string nameCfg, TGeneralNameDTO anime);

        //delete
        Task<string> DeleteNameByIdAsync(string nameCfg, string id);
    }
}
