using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    // formule personalizzate del calcolo giornata: CompanyPowerAdmin e piano Pro, controllati nel servizio (IFormuleAccesso)
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ProfiloFormuleController : NvxControllerBase
    {
        private readonly IPar_ProfiloFormuleService _service;

        public Par_ProfiloFormuleController(IHttpContextAccessor httpContextAccessor, IPar_ProfiloFormuleService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_ProfiloFormuleOutModel>> GetAll(GenericRequest<Par_ProfiloFormuleInModel> inModel)
            => await _service.GetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloFormuleGet")]
        public async Task<GenericResult<Par_ProfiloFormuleGetOutModel>> Par_ProfiloFormuleGet(GenericRequest<Par_ProfiloFormuleGetInModel> inModel)
            => await _service.Par_ProfiloFormuleGet(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloFormulePut")]
        public async Task<GenericResult<Par_ProfiloFormulePutOutModel>> Par_ProfiloFormulePut(GenericRequest<Par_ProfiloFormulePutInModel> inModel)
            => await _service.Par_ProfiloFormulePut(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloFormuleDelete")]
        public async Task<GenericResult<Par_ProfiloFormule_DeleteOutModel>> Par_ProfiloFormuleDelete(GenericRequest<Par_ProfiloFormule_DeleteInModel> inModel)
            => await _service.Par_ProfiloFormuleDelete(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloFormuleDuplica")]
        public async Task<GenericResult<Par_ProfiloFormule_DuplicaOutModel>> Par_ProfiloFormuleDuplica(GenericRequest<Par_ProfiloFormule_DuplicaInModel> inModel)
            => await _service.Par_ProfiloFormuleDuplica(inModel, false);
    }
}
