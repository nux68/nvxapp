using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ProfiloOrarioController : NvxControllerBase
    {
        private readonly IPar_ProfiloOrarioService _par_ProfiloOrarioService;

        public Par_ProfiloOrarioController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_ProfiloOrarioService par_ProfiloOrarioService
        ) : base(httpContextAccessor)
        {
            _par_ProfiloOrarioService = par_ProfiloOrarioService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_ProfiloOrario_GetAllOutModel>> GetAll(GenericRequest<Par_ProfiloOrario_GetAllInModel> inModel)
        {
            var res = await _par_ProfiloOrarioService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloOrarioGet")]
        public async Task<GenericResult<Par_ProfiloOrario_GetOutModel>> Par_ProfiloOrarioGet(GenericRequest<Par_ProfiloOrario_GetInModel> inModel)
        {
            return await _par_ProfiloOrarioService.Par_ProfiloOrarioGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloOrarioPut")]
        public async Task<GenericResult<Par_ProfiloOrario_PutOutModel>> Par_ProfiloOrarioPut(GenericRequest<Par_ProfiloOrario_PutInModel> inModel)
        {
            return await _par_ProfiloOrarioService.Par_ProfiloOrarioPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ProfiloOrarioDelete")]
        public async Task<GenericResult<Par_ProfiloOrario_DeleteOutModel>> Par_ProfiloOrarioDelete(GenericRequest<Par_ProfiloOrario_DeleteInModel> inModel)
        {
            return await _par_ProfiloOrarioService.Par_ProfiloOrarioDelete(inModel, false);
        }

    }



}
