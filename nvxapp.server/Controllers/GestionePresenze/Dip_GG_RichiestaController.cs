using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;



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
        [Route("GetAll4User")]
        public async Task<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> inModel)
        {
            var res = await _dip_GG_RichiestaService.GetAll4User(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("GetAll4Admin")]
        public async Task<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> GetAll4Admin(GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> inModel)
        {
            var res = await _dip_GG_RichiestaService.GetAll4Admin(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Send")]
        public async Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> inModel)
        {
            var res = await _dip_GG_RichiestaService.Send(inModel, false);

            return res;
        }



    }



}
