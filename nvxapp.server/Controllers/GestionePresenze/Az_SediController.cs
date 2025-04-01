using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_Sedi;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediController : NvxControllerBase
    {
        private readonly IAz_SediService _az_SediService;

        public Az_SediController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_SediService az_SediService
        ) : base(httpContextAccessor)
        {
            _az_SediService = az_SediService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SediOutModel>> GetAll(GenericRequest<Az_SediInModel> inModel)
        {
            var res = await _az_SediService.GetAll(inModel, false);

            return res;
        }

        

    }



}
