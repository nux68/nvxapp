using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ExportCauController : NvxControllerBase
    {
        private readonly IPar_ExportCauService _par_ExportCauService;

        public Par_ExportCauController(
            IHttpContextAccessor httpContextAccessor,
            IPar_ExportCauService par_ExportCauService
        ) : base(httpContextAccessor)
        {
            _par_ExportCauService = par_ExportCauService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_ExportCau_GetAll_OutModel>> GetAll(GenericRequest<Par_ExportCau_GetAll_InModel> inModel)
        {
            return await _par_ExportCauService.GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCauGet")]
        public async Task<GenericResult<Par_ExportCau_Get_OutModel>> Par_ExportCauGet(GenericRequest<Par_ExportCau_Get_InModel> inModel)
        {
            return await _par_ExportCauService.Par_ExportCauGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCauPut")]
        public async Task<GenericResult<Par_ExportCau_Put_OutModel>> Par_ExportCauPut(GenericRequest<Par_ExportCau_Put_InModel> inModel)
        {
            return await _par_ExportCauService.Par_ExportCauPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCauDelete")]
        public async Task<GenericResult<Par_ExportCau_Delete_OutModel>> Par_ExportCauDelete(GenericRequest<Par_ExportCau_Delete_InModel> inModel)
        {
            return await _par_ExportCauService.Par_ExportCauDelete(inModel, false);
        }
    }
}
