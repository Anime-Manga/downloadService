using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IGeneralObjectRegisterRepository<T>
    {
        //get
        Task<List<T>> GetObjectsRegisterByObjectId(string id);

        //insert
        Task<T> InsertObjectRegisterAsync(T objectRegister);

        //put
        Task<T> UpdateObjectRegisterAsync(T objectRegister);
    }
}
