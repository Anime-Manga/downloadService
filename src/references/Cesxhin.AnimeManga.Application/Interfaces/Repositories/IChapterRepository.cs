using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IChapterRepository : IGeneralObjectRepository<Chapter>
    {
        public Task<int> DeleteByNameAsync(string id);
    }
}
