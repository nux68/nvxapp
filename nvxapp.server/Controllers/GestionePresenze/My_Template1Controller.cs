using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class My_Template1Controller : NvxControllerBase
    {
        private readonly IMy_template1Service _my_Template1Service;

        public My_Template1Controller(
                                        IHttpContextAccessor httpContextAccessor,
                                        IMy_template1Service dip_GG_CausaliService
        ) : base(httpContextAccessor)
        {
            _my_Template1Service = dip_GG_CausaliService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> inModel)
        {
            var res = await _my_Template1Service.GetAll(inModel, false);

            return res;
        }

        

    }



}
