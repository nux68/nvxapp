using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ExportCau_CausaliController : NvxControllerBase
    {
        private readonly IPar_ExportCau_CausaliService _par_ExportCau_CausaliService;

        public Par_ExportCau_CausaliController(
            IHttpContextAccessor httpContextAccessor,
            IPar_ExportCau_CausaliService par_ExportCau_CausaliService
        ) : base(httpContextAccessor)
        {
            _par_ExportCau_CausaliService = par_ExportCau_CausaliService;
        }


        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Causali_Get")]
        public async Task<GenericResult<Par_ExportCau_Causali_Get_OutModel>> Par_ExportCau_Causali_Get(GenericRequest<Par_ExportCau_Causali_Get_InModel> inModel)
        {
            return await _par_ExportCau_CausaliService.Par_ExportCau_Causali_Get(inModel, false);
        }


        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Causali_GetAll_4Edit")]
        public async Task<GenericResult<Par_ExportCau_Causali_GetAll_4Edit_OutModel>> Par_ExportCau_Causali_GetAll_4Edit(GenericRequest<Par_ExportCau_Causali_GetAll_4Edit_InModel> inModel)
        {
            return await _par_ExportCau_CausaliService.Par_ExportCau_Causali_GetAll_4Edit(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Causali_PutAll_4Edit")]
        public async Task<GenericResult<Par_ExportCau_Causali_PutAll_4Edit_OutModel>> Par_ExportCau_Causali_PutAll_4Edit(GenericRequest<Par_ExportCau_Causali_PutAll_4Edit_InModel> inModel)
        {
            return await _par_ExportCau_CausaliService.Par_ExportCau_Causali_PutAll_4Edit(inModel, false);
        }
    }
}
