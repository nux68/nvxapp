using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // Licenze d'uso (ANALISI_INFRASTRUCTURE_LICENZE.md): impostazioni e listini (SuperUser, centro abilitato), licenze dell'azienda o dello studio.
    // I ruoli li controlla il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : NvxControllerBase
    {
        private readonly ILicenseService _service;

        public LicenseController(IHttpContextAccessor httpContextAccessor, ILicenseService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("LicenseSettingsGet")]
        public async Task<GenericResult<LicenseSettingsGetOutModel>> LicenseSettingsGet(GenericRequest<LicenseSettingsGetInModel> inModel)
            => await _service.LicenseSettingsGet(inModel, false);

        [Authorize, HttpPost, Route("LicenseSettingsPut")]
        public async Task<GenericResult<LicenseSettingsPutOutModel>> LicenseSettingsPut(GenericRequest<LicenseSettingsPutInModel> inModel)
            => await _service.LicenseSettingsPut(inModel, false);

        [Authorize, HttpPost, Route("LicenseCatalogList")]
        public async Task<GenericResult<LicenseCatalogListOutModel>> LicenseCatalogList(GenericRequest<LicenseCatalogListInModel> inModel)
            => await _service.LicenseCatalogList(inModel, false);

        [Authorize, HttpPost, Route("LicenseCatalogGet")]
        public async Task<GenericResult<LicenseCatalogGetOutModel>> LicenseCatalogGet(GenericRequest<LicenseCatalogGetInModel> inModel)
            => await _service.LicenseCatalogGet(inModel, false);

        [Authorize, HttpPost, Route("LicenseCatalogPut")]
        public async Task<GenericResult<LicenseCatalogPutOutModel>> LicenseCatalogPut(GenericRequest<LicenseCatalogPutInModel> inModel)
            => await _service.LicenseCatalogPut(inModel, false);

        [Authorize, HttpPost, Route("LicenseCatalogDelete")]
        public async Task<GenericResult<LicenseCatalogDeleteOutModel>> LicenseCatalogDelete(GenericRequest<LicenseCatalogDeleteInModel> inModel)
            => await _service.LicenseCatalogDelete(inModel, false);

        // pagina prezzi dello showcase: listini globali del SuperUser, senza login
        [AllowAnonymous, HttpPost, Route("LicensePublicPriceList")]
        public async Task<GenericResult<LicensePublicPriceListOutModel>> LicensePublicPriceList(GenericRequest<LicensePublicPriceListInModel> inModel)
            => await _service.LicensePublicPriceList(inModel, false);

        [Authorize, HttpPost, Route("LicenseStatusGet")]
        public async Task<GenericResult<LicenseStatusGetOutModel>> LicenseStatusGet(GenericRequest<LicenseStatusGetInModel> inModel)
            => await _service.LicenseStatusGet(inModel, false);

        [Authorize, HttpPost, Route("LicenseSubscribe")]
        public async Task<GenericResult<LicenseSubscribeOutModel>> LicenseSubscribe(GenericRequest<LicenseSubscribeInModel> inModel)
            => await _service.LicenseSubscribe(inModel, false);

        [Authorize, HttpPost, Route("LicenseRenew")]
        public async Task<GenericResult<LicenseRenewOutModel>> LicenseRenew(GenericRequest<LicenseRenewInModel> inModel)
            => await _service.LicenseRenew(inModel, false);

        [Authorize, HttpPost, Route("LicenseAutoRenewPut")]
        public async Task<GenericResult<LicenseAutoRenewPutOutModel>> LicenseAutoRenewPut(GenericRequest<LicenseAutoRenewPutInModel> inModel)
            => await _service.LicenseAutoRenewPut(inModel, false);

        [Authorize, HttpPost, Route("LicenseCancel")]
        public async Task<GenericResult<LicenseCancelOutModel>> LicenseCancel(GenericRequest<LicenseCancelInModel> inModel)
            => await _service.LicenseCancel(inModel, false);
    }
}
