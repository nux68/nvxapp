using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Compliance
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Cmp_ConfigController : NvxControllerBase
    {
        private readonly ICmp_ConfigService _service;
        public Cmp_ConfigController(IHttpContextAccessor httpContextAccessor, ICmp_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Cmp_CfgGet")]
        public async Task<GenericResult<Cmp_CfgGetOutModel>> Cmp_CfgGet(GenericRequest<Cmp_EmptyInModel> inModel) => await _service.Cmp_CfgGet(inModel, false);
        [Authorize, HttpPost, Route("Cmp_CfgPut")]
        public async Task<GenericResult<Cmp_CfgPutOutModel>> Cmp_CfgPut(GenericRequest<Cmp_CfgPutInModel> inModel) => await _service.Cmp_CfgPut(inModel, false);
    }

    // Sicurezza sul lavoro (CMP-81)
    [ApiController]
    [Route("api/[controller]")]
    public class Cmp_SicurezzaController : NvxControllerBase
    {
        private readonly ICmp_SicurezzaService _service;
        public Cmp_SicurezzaController(IHttpContextAccessor httpContextAccessor, ICmp_SicurezzaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Cmp_Lookup")]
        public async Task<GenericResult<Cmp_LookupOutModel>> Cmp_Lookup(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_Lookup(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansioneList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneModel>>> Cmp_MansioneList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_MansioneList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansionePut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneModel>>> Cmp_MansionePut(GenericRequest<Cmp_PutInModel<Cmp_MansioneModel>> inModel) => await _service.Cmp_MansionePut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansioneDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_MansioneDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansioneCorsoList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_MansioneCorsoList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansioneCorsoPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoPut(GenericRequest<Cmp_PutInModel<Cmp_MansioneCorsoModel>> inModel) => await _service.Cmp_MansioneCorsoPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_MansioneCorsoDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneCorsoDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_MansioneCorsoDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DipendenteMansioneList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansioneList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_DipendenteMansioneList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DipendenteMansionePut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansionePut(GenericRequest<Cmp_PutInModel<Cmp_DipendenteMansioneModel>> inModel) => await _service.Cmp_DipendenteMansionePut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DipendenteMansioneDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DipendenteMansioneDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_DipendenteMansioneDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_FormazioneList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneModel>>> Cmp_FormazioneList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_FormazioneList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_FormazionePut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_FormazioneModel>>> Cmp_FormazionePut(GenericRequest<Cmp_PutInModel<Cmp_FormazioneModel>> inModel) => await _service.Cmp_FormazionePut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_FormazioneDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_FormazioneDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_FormazioneDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_VisitaList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_VisitaModel>>> Cmp_VisitaList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_VisitaList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_VisitaPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_VisitaModel>>> Cmp_VisitaPut(GenericRequest<Cmp_PutInModel<Cmp_VisitaModel>> inModel) => await _service.Cmp_VisitaPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_VisitaDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_VisitaDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_VisitaDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_DpiModel>>> Cmp_DpiList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_DpiList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_DpiModel>>> Cmp_DpiPut(GenericRequest<Cmp_PutInModel<Cmp_DpiModel>> inModel) => await _service.Cmp_DpiPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_DpiDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiConsegnaList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_DpiConsegnaList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiConsegnaPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaPut(GenericRequest<Cmp_PutInModel<Cmp_DpiConsegnaModel>> inModel) => await _service.Cmp_DpiConsegnaPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DpiConsegnaDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiConsegnaDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_DpiConsegnaDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_NominaList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_NominaModel>>> Cmp_NominaList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_NominaList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_NominaPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_NominaModel>>> Cmp_NominaPut(GenericRequest<Cmp_PutInModel<Cmp_NominaModel>> inModel) => await _service.Cmp_NominaPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_NominaDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_NominaDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_NominaDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DocumentoList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_DocumentoList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DocumentoPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoPut(GenericRequest<Cmp_PutInModel<Cmp_DocumentoModel>> inModel) => await _service.Cmp_DocumentoPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_DocumentoDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DocumentoDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_DocumentoDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_FormazioneStato")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneStatoModel>>> Cmp_FormazioneStato(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_FormazioneStato(inModel, false);
        [Authorize, HttpPost, Route("Cmp_Mine")]
        public async Task<GenericResult<Cmp_MineOutModel>> Cmp_Mine(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_Mine(inModel, false);
    }
}
