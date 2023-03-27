using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Interfaces.Controllers
{
    public interface IGeneralControllerBase<I, O, R, D>
    {
        //get
        public Task<IActionResult> GetInfoAll(string nameCfg, string username);
        public Task<IActionResult> GetInfoByName(string nameCfg, string name, string username);
        public Task<IActionResult> GetMostInfoByName(string nameCfg, string name, string username);
        public Task<IActionResult> GetAll(string nameCfg, string username);
        public Task<IActionResult> GetObjectByName(string name);
        public Task<IActionResult> GetObjectById(string id);
        public Task<IActionResult> GetObjectRegisterByObjectId(string id);
        public Task<IActionResult> GetListSearchByName(string nameCfg, string name);

        //put
        public Task<IActionResult> PutInfo(string nameCfg, I infoClass);
        public Task<IActionResult> PutObject(O objectClass);
        public Task<IActionResult> PutObjects(List<O> objectsClass);
        public Task<IActionResult> PutObjectsRegisters(List<R> objectsRegistersClass);
        public Task<IActionResult> UpdateObjectRegister(R objectRegisterClass);
        public Task<IActionResult> RedownloadObjectByUrlPage(string id);
        public Task<IActionResult> DownloadInfoByUrlPage(D objectsClass);
        public Task<IActionResult> PutUpdateStateDownload(O objectClass);

        //delete
        public Task<IActionResult> DeleteInfo(string nameCfg, string id);
    }
}
