using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_GiustificativiController : NvxControllerBase
    {
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public Par_GiustificativiController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_GiustificativiService par_GiustificativiService
        ) : base(httpContextAccessor)
        {
            _par_GiustificativiService = par_GiustificativiService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_GiustificativiOutModel>> GetAll(GenericRequest<Par_GiustificativiInModel> inModel)
        {
            var res = await _par_GiustificativiService.GetAll(inModel, false);

            return res;
        }

        

    }



}
