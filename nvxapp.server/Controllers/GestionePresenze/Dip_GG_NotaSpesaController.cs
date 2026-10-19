using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService;



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

        // spese (con l'elenco degli allegati) di una richiesta di nota spese
        [Authorize]
        [HttpPost]
        [Route("Get4Richiesta")]
        public async Task<GenericResult<Dip_GG_NotaSpesa_Get4Richiesta_OutModel>> Get4Richiesta(GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel> inModel)
            => await _dip_GG_NotaSpesaService.Get4Richiesta(inModel, false);

        // ricerca delle spese per dipendente / tipo di spesa (CompanyPowerAdmin)
        [Authorize]
        [HttpPost]
        [Route("Search")]
        public async Task<GenericResult<Dip_GG_NotaSpesa_Search_OutModel>> Search(GenericRequest<Dip_GG_NotaSpesa_Search_InModel> inModel)
            => await _dip_GG_NotaSpesaService.Search(inModel, false);

        // contenuto di un allegato (base64)
        [Authorize]
        [HttpPost]
        [Route("AllegatoGet")]
        public async Task<GenericResult<Dip_GG_NotaSpesa_Allegato_Get_OutModel>> AllegatoGet(GenericRequest<Dip_GG_NotaSpesa_Allegato_Get_InModel> inModel)
            => await _dip_GG_NotaSpesaService.AllegatoGet(inModel, false);

        

    }



}
