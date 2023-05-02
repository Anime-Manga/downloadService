using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IGeneralObjectRegister<TObjectRegisterDTO, TObjectDTO>
    {
        //get
        Task<TObjectRegisterDTO> GetObjectRegisterByObjectId(string id);
        Task<List<TObjectRegisterDTO>> GetObjectsRegistersByListObjectId(List<TObjectDTO> objectDTOs);

        //insert
        Task<TObjectRegisterDTO> InsertObjectRegisterAsync(TObjectRegisterDTO objectGeneralRegister);
        Task<IEnumerable<TObjectRegisterDTO>> InsertObjectsRegistersAsync(List<TObjectRegisterDTO> objectGeneralRegister);

        //put
        Task<TObjectRegisterDTO> UpdateObjectRegisterAsync(TObjectRegisterDTO objectGeneralRegister);
    }
}
