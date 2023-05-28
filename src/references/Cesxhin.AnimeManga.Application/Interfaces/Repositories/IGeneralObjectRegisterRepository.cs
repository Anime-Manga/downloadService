using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IGeneralObjectRegisterRepository<T, F>
    {
        //get
        Task<T> GetObjectRegisterByObjectId(string id);
        Task<IEnumerable<T>> GetObjectsRegistersByListObjectId(List<F> listObjects);

        //insert
        Task<IEnumerable<T>> InsertObjectsRegisterAsync(List<T> objectRegister);
        Task<T> InsertObjectRegisterAsync(T objectRegister);

        //put
        Task<T> UpdateObjectRegisterAsync(T objectRegister);
    }
}
