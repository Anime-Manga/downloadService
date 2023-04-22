using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IGeneralObjectRepository<T>
    {
        //get
        Task<T> GetObjectByIDAsync(string id);
        Task<IEnumerable<T>> GetObjectsByNameAsync(string nameGeneral);

        //insert
        Task<IEnumerable<T>> InsertObjectsAsync(List<T> objectsGeneral);
        Task<T> InsertObjectAsync(T objectGeneral);

        //update
        Task<T> UpdateStateDownloadAsync(T objectGeneral);

        //reset
        Task<T> ResetStatusDownloadObjectByIdAsync(T objectGeneral);
        Task<IEnumerable<T>> ResetStatusDownloadObjectsByIdAsync(List<T> objectsGeneral);
    }
}
