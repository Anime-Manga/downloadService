using Cesxhin.AnimeManga.Domain.DTO;
using Newtonsoft.Json.Linq;

namespace Cesxhin.AnimeManga.Application.Interfaces.Services
{
    public interface IDescriptionBookService : IGeneralNameService<GenericBookDTO, JObject>
    {
    }
}
