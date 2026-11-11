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
    public class Ast_FlottaController : NvxControllerBase
    {
        private readonly IAst_FlottaService _service;
        public Ast_FlottaController(IHttpContextAccessor httpContextAccessor, IAst_FlottaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Ast_VeicoloList")]
        public async Task<GenericResult<Ast_VeicoloListOutModel>> Ast_VeicoloList(GenericRequest<Ast_VeicoloListInModel> inModel) => await _service.Ast_VeicoloList(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloGet")]
        public async Task<GenericResult<Ast_VeicoloGetOutModel>> Ast_VeicoloGet(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_VeicoloGet(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloPut")]
        public async Task<GenericResult<Ast_VeicoloPutOutModel>> Ast_VeicoloPut(GenericRequest<Ast_VeicoloPutInModel> inModel) => await _service.Ast_VeicoloPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_VeicoloDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_AssegnazionePut")]
        public async Task<GenericResult<Ast_AssegnazionePutOutModel>> Ast_AssegnazionePut(GenericRequest<Ast_AssegnazionePutInModel> inModel) => await _service.Ast_AssegnazionePut(inModel, false);

        [Authorize, HttpPost, Route("Ast_AssegnazioneDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_AssegnazioneDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_AssegnazioneDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloScadenzaPut")]
        public async Task<GenericResult<Ast_VeicoloScadenzaPutOutModel>> Ast_VeicoloScadenzaPut(GenericRequest<Ast_VeicoloScadenzaPutInModel> inModel) => await _service.Ast_VeicoloScadenzaPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloScadenzaDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloScadenzaDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_VeicoloScadenzaDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloCostoPut")]
        public async Task<GenericResult<Ast_VeicoloCostoPutOutModel>> Ast_VeicoloCostoPut(GenericRequest<Ast_VeicoloCostoPutInModel> inModel) => await _service.Ast_VeicoloCostoPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_VeicoloCostoDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloCostoDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_VeicoloCostoDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_MultaPut")]
        public async Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaPut(GenericRequest<Ast_MultaPutInModel> inModel) => await _service.Ast_MultaPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_MultaDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_MultaDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_MultaDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_MultaNotifica")]
        public async Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaNotifica(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_MultaNotifica(inModel, false);

        [Authorize, HttpPost, Route("Ast_MieiVeicoliGet")]
        public async Task<GenericResult<Ast_MieiVeicoliOutModel>> Ast_MieiVeicoliGet(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_MieiVeicoliGet(inModel, false);
    }
}
