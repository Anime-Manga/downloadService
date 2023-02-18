using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IGeneralNameRepository<TGeneralName>
    {
        //get
        Task<List<TGeneralName>> GetNameAllAsync(string nameCfg);
        Task<TGeneralName> GetNameByNameAsync(string nameCfg, string name);
        Task<List<TGeneralName>> GetMostNameByNameAsync(string nameCfg, string name);

        //Insert
        Task<TGeneralName> InsertNameAsync(string nameCfg, TGeneralName generalName);

        //delete
        Task<int> DeleteNameAsync(string nameCfg, string id);
    }
}
