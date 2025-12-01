using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Threading.Tasks;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_OrarioController : NvxControllerBase
    {
        private readonly IPar_OrarioService _par_OrarioService;

        public Par_OrarioController(
            IHttpContextAccessor httpContextAccessor,
            IPar_OrarioService par_OrarioService
        ) : base(httpContextAccessor)
        {
            _par_OrarioService = par_OrarioService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_Orario_GetAllOutModel>> GetAll(GenericRequest<Par_Orario_GetAllInModel> inModel)
        {
            return await _par_OrarioService.GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioGet")]
        public async Task<GenericResult<Par_Orario_GetOutModel>> Par_OrarioGet(GenericRequest<Par_Orario_GetInModel> inModel)
        {
            return await _par_OrarioService.Par_OrarioGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioPut")]
        public async Task<GenericResult<Par_Orario_PutOutModel>> Par_OrarioPut(GenericRequest<Par_Orario_PutInModel> inModel)
        {
            return await _par_OrarioService.Par_OrarioPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioDelete")]
        public async Task<GenericResult<Par_Orario_DeleteOutModel>> Par_OrarioDelete(GenericRequest<Par_Orario_DeleteInModel> inModel)
        {
            return await _par_OrarioService.Par_OrarioDelete(inModel, false);
        }
    }
}
