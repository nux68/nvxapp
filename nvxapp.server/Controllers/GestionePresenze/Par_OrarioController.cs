using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_Orario;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;



namespace nvxapp.server.Controllers
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
        public async Task<GenericResult<Par_OrarioOutModel>> GetAll(GenericRequest<Par_OrarioInModel> inModel)
        {
            var res = await _par_OrarioService.GetAll(inModel, false);

            return res;
        }

        

    }



}
