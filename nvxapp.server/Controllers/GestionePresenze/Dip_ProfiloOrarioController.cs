using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Anagrafica;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrario;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;



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
        [Route("GetAll")]
        public async Task<GenericResult<Dip_ProfiloOrarioOutModel>> GetAll(GenericRequest<Dip_ProfiloOrarioInModel> inModel)
        {
            var res = await _dip_ProfiloOrarioService.GetAll(inModel, false);

            return res;
        }

        

    }



}
