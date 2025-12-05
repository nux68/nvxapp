using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_ProfiloOrarioController : NvxControllerBase
    {
        private readonly IDip_ProfiloOrarioService _dip_ProfiloOrarioService;

        public Dip_ProfiloOrarioController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_ProfiloOrarioService dip_ProfiloOrarioService
        ) : base(httpContextAccessor)
        {
            _dip_ProfiloOrarioService = dip_ProfiloOrarioService;
        }

        

       [Authorize]
        [HttpPost]
        [Route("Dip_ProfiloOrarioGet")]
        public async Task<GenericResult<Dip_ProfiloOrario_Get_OutModel>> Dip_ProfiloOrarioGet(GenericRequest<Dip_ProfiloOrario_Get_InModel> inModel)
        {
            return await _dip_ProfiloOrarioService.Dip_ProfiloOrarioGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_ProfiloOrarioPut")]
        public async Task<GenericResult<Dip_ProfiloOrario_Put_OutModel>> Dip_ProfiloOrarioPut(GenericRequest<Dip_ProfiloOrario_Put_InModel> inModel)
        {
            return await _dip_ProfiloOrarioService.Dip_ProfiloOrarioPut(inModel, false);
        }

        

    }



}
