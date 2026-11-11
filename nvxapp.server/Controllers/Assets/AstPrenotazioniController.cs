using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Assets
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.
    [ApiController]
    [Route("api/[controller]")]
    public class Ast_PrenotazioniController : NvxControllerBase
    {
        private readonly IAst_PrenotazioniService _service;
        public Ast_PrenotazioniController(IHttpContextAccessor httpContextAccessor, IAst_PrenotazioniService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Ast_RisorsaList")]
        public async Task<GenericResult<Ast_RisorsaListOutModel>> Ast_RisorsaList(GenericRequest<Ast_RisorsaListInModel> inModel) => await _service.Ast_RisorsaList(inModel, false);

        [Authorize, HttpPost, Route("Ast_RisorsaPut")]
        public async Task<GenericResult<Ast_RisorsaPutOutModel>> Ast_RisorsaPut(GenericRequest<Ast_RisorsaPutInModel> inModel) => await _service.Ast_RisorsaPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_RisorsaDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_RisorsaDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_RisorsaDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_PrenotazioneList")]
        public async Task<GenericResult<Ast_PrenotazioneListOutModel>> Ast_PrenotazioneList(GenericRequest<Ast_PrenotazioneListInModel> inModel) => await _service.Ast_PrenotazioneList(inModel, false);

        [Authorize, HttpPost, Route("Ast_PrenotazionePut")]
        public async Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazionePut(GenericRequest<Ast_PrenotazionePutInModel> inModel) => await _service.Ast_PrenotazionePut(inModel, false);

        [Authorize, HttpPost, Route("Ast_PrenotazioneAnnulla")]
        public async Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazioneAnnulla(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_PrenotazioneAnnulla(inModel, false);
    }
}
