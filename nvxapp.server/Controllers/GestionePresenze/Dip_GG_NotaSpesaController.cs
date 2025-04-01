using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesa;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_NotaSpesaController : NvxControllerBase
    {
        private readonly IDip_GG_NotaSpesaService _dip_GG_NotaSpesaService;

        public Dip_GG_NotaSpesaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_NotaSpesaService dip_GG_NotaSpesaService
        ) : base(httpContextAccessor)
        {
            _dip_GG_NotaSpesaService = dip_GG_NotaSpesaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_NotaSpesaOutModel>> GetAll(GenericRequest<Dip_GG_NotaSpesaInModel> inModel)
        {
            var res = await _dip_GG_NotaSpesaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
