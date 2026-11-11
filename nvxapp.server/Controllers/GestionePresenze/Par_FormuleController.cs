using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    // formule personalizzate del calcolo giornata: CompanyPowerAdmin e piano Pro, controllati nel servizio (IFormuleAccesso)
    [ApiController]
    [Route("api/[controller]")]
    public class Par_FormuleController : NvxControllerBase
    {
        private readonly IPar_FormuleService _service;

        public Par_FormuleController(IHttpContextAccessor httpContextAccessor, IPar_FormuleService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_FormuleOutModel>> GetAll(GenericRequest<Par_FormuleInModel> inModel)
            => await _service.GetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_FormuleGet")]
        public async Task<GenericResult<Par_FormuleGetOutModel>> Par_FormuleGet(GenericRequest<Par_FormuleGetInModel> inModel)
            => await _service.Par_FormuleGet(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_FormulePut")]
        public async Task<GenericResult<Par_FormulePutOutModel>> Par_FormulePut(GenericRequest<Par_FormulePutInModel> inModel)
            => await _service.Par_FormulePut(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_FormuleDelete")]
        public async Task<GenericResult<Par_Formule_DeleteOutModel>> Par_FormuleDelete(GenericRequest<Par_Formule_DeleteInModel> inModel)
            => await _service.Par_FormuleDelete(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_FormuleDuplica")]
        public async Task<GenericResult<Par_Formule_DuplicaOutModel>> Par_FormuleDuplica(GenericRequest<Par_Formule_DuplicaInModel> inModel)
            => await _service.Par_FormuleDuplica(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("CtrlSintassi")]
        public async Task<GenericResult<Par_Formule_CtrlSintassiOutModel>> CtrlSintassi(GenericRequest<Par_Formule_CtrlSintassiInModel> inModel)
            => await _service.CtrlSintassi(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Metodi")]
        public async Task<GenericResult<Par_Formule_MetodiOutModel>> Metodi(GenericRequest<Par_Formule_MetodiInModel> inModel)
            => await _service.Metodi(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("RapportiProva")]
        public async Task<GenericResult<Par_Formule_RapportiProvaOutModel>> RapportiProva(GenericRequest<Par_Formule_RapportiProvaInModel> inModel)
            => await _service.RapportiProva(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Prova")]
        public async Task<GenericResult<Par_Formule_ProvaOutModel>> Prova(GenericRequest<Par_Formule_ProvaInModel> inModel)
            => await _service.Prova(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Log")]
        public async Task<GenericResult<Par_Formule_LogOutModel>> Log(GenericRequest<Par_Formule_LogInModel> inModel)
            => await _service.Log(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("GeneraAI")]
        public async Task<GenericResult<Par_Formule_GeneraAIOutModel>> GeneraAI(GenericRequest<Par_Formule_GeneraAIInModel> inModel)
            => await _service.GeneraAI(inModel, false);
    }
}
