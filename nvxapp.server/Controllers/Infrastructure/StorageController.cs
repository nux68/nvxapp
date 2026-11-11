using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // Spazio a disposizione delle aziende: valori predefiniti (SuperUser), spazio delle aziende
    // (SuperUser, centro, studio in lettura, azienda in lettura). I ruoli li controlla il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : NvxControllerBase
    {
        private readonly IStorageService _service;

        public StorageController(IHttpContextAccessor httpContextAccessor, IStorageService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("StorageSettingsGet")]
        public async Task<GenericResult<StorageSettingsGetOutModel>> StorageSettingsGet(GenericRequest<StorageSettingsGetInModel> inModel)
            => await _service.StorageSettingsGet(inModel, false);

        [Authorize, HttpPost, Route("StorageSettingsPut")]
        public async Task<GenericResult<StorageSettingsPutOutModel>> StorageSettingsPut(GenericRequest<StorageSettingsPutInModel> inModel)
            => await _service.StorageSettingsPut(inModel, false);

        [Authorize, HttpPost, Route("CompanyStorageList")]
        public async Task<GenericResult<CompanyStorageListOutModel>> CompanyStorageList(GenericRequest<CompanyStorageListInModel> inModel)
            => await _service.CompanyStorageList(inModel, false);

        [Authorize, HttpPost, Route("CompanyStorageGet")]
        public async Task<GenericResult<CompanyStorageGetOutModel>> CompanyStorageGet(GenericRequest<CompanyStorageGetInModel> inModel)
            => await _service.CompanyStorageGet(inModel, false);

        [Authorize, HttpPost, Route("CompanyStoragePut")]
        public async Task<GenericResult<CompanyStoragePutOutModel>> CompanyStoragePut(GenericRequest<CompanyStoragePutInModel> inModel)
            => await _service.CompanyStoragePut(inModel, false);
    }
}
