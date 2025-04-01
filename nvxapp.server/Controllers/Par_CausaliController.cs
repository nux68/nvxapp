using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Account;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_CausaliController : NvxControllerBase
    {
        private readonly IPar_CausaliService _par_CausaliService;

        public Par_CausaliController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_CausaliService par_CausaliService
        ) : base(httpContextAccessor)
        {
            _par_CausaliService = par_CausaliService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> inModel)
        {
            var res = await _par_CausaliService.GetAll(inModel, false);

            return res;
        }

        

    }



}
