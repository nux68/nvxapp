using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Timbratura;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_TimbraturaController : NvxControllerBase
    {
        private readonly IDip_GG_TimbraturaService _Dip_GG_TimbraturaService;

        public Dip_GG_TimbraturaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_TimbraturaService Dip_GG_TimbraturaService
        ) : base(httpContextAccessor)
        {
            _Dip_GG_TimbraturaService = Dip_GG_TimbraturaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_TimbraturaOutModel>> GetAll(GenericRequest<Dip_GG_TimbraturaInModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
