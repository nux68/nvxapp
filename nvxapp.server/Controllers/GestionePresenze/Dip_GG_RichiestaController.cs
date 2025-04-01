using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Richiesta;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_RichiestaController : NvxControllerBase
    {
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;

        public Dip_GG_RichiestaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_RichiestaService dip_GG_RichiestaService
        ) : base(httpContextAccessor)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_RichiestaOutModel>> GetAll(GenericRequest<Dip_GG_RichiestaInModel> inModel)
        {
            var res = await _dip_GG_RichiestaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
