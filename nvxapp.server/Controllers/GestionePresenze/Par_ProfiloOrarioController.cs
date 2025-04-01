using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrario;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;



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
        public async Task<GenericResult<Par_ProfiloOrarioOutModel>> GetAll(GenericRequest<Par_ProfiloOrarioInModel> inModel)
        {
            var res = await _par_ProfiloOrarioService.GetAll(inModel, false);

            return res;
        }

        

    }



}
