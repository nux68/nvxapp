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
            IMy_template1Service myTemplate1Service
        ) : base(httpContextAccessor)
        {
            _my_Template1Service = myTemplate1Service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> inModel)
        {
            var res = await _my_Template1Service.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("MyTemplate1Get")]
        public async Task<GenericResult<My_template1_GetOutModel>> MyTemplate1Get(GenericRequest<My_template1_GetInModel> inModel)
        {
            var res = await _my_Template1Service.MyTemplate1Get(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("MyTemplate1Put")]
        public async Task<GenericResult<My_template1_PutOutModel>> MyTemplate1Put(GenericRequest<My_template1_PutInModel> inModel)
        {
            var res = await _my_Template1Service.MyTemplate1Put(inModel, false);
            return res;
        }
    }
}
