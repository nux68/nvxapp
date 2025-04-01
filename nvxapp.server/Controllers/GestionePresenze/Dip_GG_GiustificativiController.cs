using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Giustificativi;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_GiustificativiController : NvxControllerBase
    {
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;

        public Dip_GG_GiustificativiController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_GiustificativiService dip_GG_GiustificativiService
        ) : base(httpContextAccessor)
        {
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_GiustificativiOutModel>> GetAll(GenericRequest<Dip_GG_GiustificativiInModel> inModel)
        {
            var res = await _dip_GG_GiustificativiService.GetAll(inModel, false);

            return res;
        }

        

    }



}
