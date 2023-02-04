using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IGeneralNameService<TGeneralNameDTO>
    {
        //get
        Task<IEnumerable<TGeneralNameDTO>> GetNameAllAsync();
        Task<TGeneralNameDTO> GetNameByNameAsync(string name);
        Task<IEnumerable<TGeneralNameDTO>> GetMostNameByNameAsync(string name);
        Task<IEnumerable<TGeneralNameDTO>> GetNameAllWithAllAsync();

        //insert
        Task<TGeneralNameDTO> InsertNameAsync(TGeneralNameDTO anime);

        //delete
        Task<string> DeleteNameByIdAsync(string id);
    }
}
