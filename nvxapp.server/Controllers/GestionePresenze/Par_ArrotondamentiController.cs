using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_Arrotondamenti;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ArrotondamentiService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ArrotondamentiController : NvxControllerBase
    {
        private readonly IPar_ArrotondamentiService _par_ArrotondamentiService;

        public Par_ArrotondamentiController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_ArrotondamentiService par_ArrotondamentiService
        ) : base(httpContextAccessor)
        {
            _par_ArrotondamentiService = par_ArrotondamentiService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_ArrotondamentiOutModel>> GetAll(GenericRequest<Par_ArrotondamentiInModel> inModel)
        {
            var res = await _par_ArrotondamentiService.GetAll(inModel, false);

            return res;
        }

        

    }



}
