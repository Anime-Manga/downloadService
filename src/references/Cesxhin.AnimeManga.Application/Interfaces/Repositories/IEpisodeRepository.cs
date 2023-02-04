using Cesxhin.AnimeManga.Domain.Models;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Repositories
{
    public interface IEpisodeRepository : IGeneralObjectRepository<Episode>
    {
        public Task<int> DeleteByNameAsync(string id);
    }
}
