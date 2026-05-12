using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContatoriController : NvxControllerBase
    {
        private readonly IContatoriService _contatoriService;

        public ContatoriController(
            IHttpContextAccessor httpContextAccessor,
            IContatoriService contatoriService) : base(httpContextAccessor)
        {
            _contatoriService = contatoriService;
        }

        // ?? Calcolo ??????????????????????????????????????????????????????????

        [Authorize]
        [HttpPost]
        [Route("CalcolaContatori")]
        public async Task<GenericResult<Contatori_Calcolo_OutModel>> CalcolaContatori(
            GenericRequest<Contatori_Calcolo_InModel> inModel)
        {
            return await _contatoriService.CalcolaContatori(inModel, false);
        }

        // ?? Riporto (Mese 0) ?????????????????????????????????????????????????

        [Authorize]
        [HttpPost]
        [Route("Riporto_GetAll")]
        public async Task<GenericResult<Contatori_Riporto_GetAll_OutModel>> Riporto_GetAll(
            GenericRequest<Contatori_Riporto_GetAll_InModel> inModel)
        {
            return await _contatoriService.Riporto_GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Riporto_Upsert")]
        public async Task<GenericResult<Contatori_Riporto_Upsert_OutModel>> Riporto_Upsert(
            GenericRequest<Contatori_Riporto_Upsert_InModel> inModel)
        {
            return await _contatoriService.Riporto_Upsert(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Riporto_Delete")]
        public async Task<GenericResult<Contatori_Riporto_Delete_OutModel>> Riporto_Delete(
            GenericRequest<Contatori_Riporto_Delete_InModel> inModel)
        {
            return await _contatoriService.Riporto_Delete(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CalcolaContatori_Anno")]
        public async Task<GenericResult<Contatori_Anno_OutModel>> CalcolaContatori_Anno(
            GenericRequest<Contatori_Anno_InModel> inModel)
        {
            return await _contatoriService.CalcolaContatori_Anno(inModel, false);
        }
    }
}
