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
    public class Ast_BeniController : NvxControllerBase
    {
        private readonly IAst_BeniService _service;
        public Ast_BeniController(IHttpContextAccessor httpContextAccessor, IAst_BeniService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Ast_CategoriaList")]
        public async Task<GenericResult<Ast_CategoriaListOutModel>> Ast_CategoriaList(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_CategoriaList(inModel, false);

        [Authorize, HttpPost, Route("Ast_CategoriaPut")]
        public async Task<GenericResult<Ast_CategoriaPutOutModel>> Ast_CategoriaPut(GenericRequest<Ast_CategoriaPutInModel> inModel) => await _service.Ast_CategoriaPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_CategoriaDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_CategoriaDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_CategoriaDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_BeneList")]
        public async Task<GenericResult<Ast_BeneListOutModel>> Ast_BeneList(GenericRequest<Ast_BeneListInModel> inModel) => await _service.Ast_BeneList(inModel, false);

        [Authorize, HttpPost, Route("Ast_BeneGet")]
        public async Task<GenericResult<Ast_BeneGetOutModel>> Ast_BeneGet(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_BeneGet(inModel, false);

        [Authorize, HttpPost, Route("Ast_BenePut")]
        public async Task<GenericResult<Ast_BenePutOutModel>> Ast_BenePut(GenericRequest<Ast_BenePutInModel> inModel) => await _service.Ast_BenePut(inModel, false);

        [Authorize, HttpPost, Route("Ast_BeneDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_BeneDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_ConsegnaPut")]
        public async Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_ConsegnaPut(GenericRequest<Ast_ConsegnaPutInModel> inModel) => await _service.Ast_ConsegnaPut(inModel, false);

        [Authorize, HttpPost, Route("Ast_RestituzionePut")]
        public async Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_RestituzionePut(GenericRequest<Ast_RestituzionePutInModel> inModel) => await _service.Ast_RestituzionePut(inModel, false);

        [Authorize, HttpPost, Route("Ast_BeneAssegnazioneDelete")]
        public async Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneAssegnazioneDelete(GenericRequest<Ast_IdInModel> inModel) => await _service.Ast_BeneAssegnazioneDelete(inModel, false);

        [Authorize, HttpPost, Route("Ast_DaRestituireList")]
        public async Task<GenericResult<Ast_DaRestituireListOutModel>> Ast_DaRestituireList(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_DaRestituireList(inModel, false);

        [Authorize, HttpPost, Route("Ast_DaRestituireVerifica")]
        public async Task<GenericResult<Ast_DaRestituireVerificaOutModel>> Ast_DaRestituireVerifica(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_DaRestituireVerifica(inModel, false);

        [Authorize, HttpPost, Route("Ast_MieiBeniGet")]
        public async Task<GenericResult<Ast_MieiBeniOutModel>> Ast_MieiBeniGet(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_MieiBeniGet(inModel, false);
    }
}
