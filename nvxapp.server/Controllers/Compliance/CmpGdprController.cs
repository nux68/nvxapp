using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Compliance
{
    // I permessi (ruoli dell'azienda o dello studio) li controlla il servizio: qui solo l'autenticazione.

    // Registro GDPR (CMP-GDPR), anche per lo studio
    [ApiController]
    [Route("api/[controller]")]
    public class Cmp_GdprController : NvxControllerBase
    {
        private readonly ICmp_GdprService _service;
        public Cmp_GdprController(IHttpContextAccessor httpContextAccessor, ICmp_GdprService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Cmp_TrattamentoList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_TrattamentoList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_TrattamentoPut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoPut(GenericRequest<Cmp_PutInModel<Cmp_TrattamentoModel>> inModel) => await _service.Cmp_TrattamentoPut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_TrattamentoDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_TrattamentoDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_TrattamentoDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ResponsabileList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabileList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_ResponsabileList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ResponsabilePut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabilePut(GenericRequest<Cmp_PutInModel<Cmp_ResponsabileModel>> inModel) => await _service.Cmp_ResponsabilePut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ResponsabileDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ResponsabileDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_ResponsabileDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ViolazioneList")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazioneList(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_ViolazioneList(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ViolazionePut")]
        public async Task<GenericResult<Cmp_PutOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazionePut(GenericRequest<Cmp_PutInModel<Cmp_ViolazioneModel>> inModel) => await _service.Cmp_ViolazionePut(inModel, false);
        [Authorize, HttpPost, Route("Cmp_ViolazioneDelete")]
        public async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ViolazioneDelete(GenericRequest<Cmp_DeleteInModel> inModel) => await _service.Cmp_ViolazioneDelete(inModel, false);
        [Authorize, HttpPost, Route("Cmp_GdprExport")]
        public async Task<GenericResult<Cmp_GdprExportOutModel>> Cmp_GdprExport(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_GdprExport(inModel, false);
        [Authorize, HttpPost, Route("Cmp_StudioAziende")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_StudioAziendaModel>>> Cmp_StudioAziende(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_StudioAziende(inModel, false);
        [Authorize, HttpPost, Route("Cmp_StudioScadenze")]
        public async Task<GenericResult<Cmp_ListOutModel<Cmp_StudioScadenzaModel>>> Cmp_StudioScadenze(GenericRequest<Cmp_ListInModel> inModel) => await _service.Cmp_StudioScadenze(inModel, false);
    }
}
