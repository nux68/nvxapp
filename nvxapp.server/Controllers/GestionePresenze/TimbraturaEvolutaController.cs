using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.
    [ApiController]
    [Route("api/[controller]")]
    public class TimbraturaEvolutaController : NvxControllerBase
    {
        private readonly ITimbraturaEvolutaService _service;
        public TimbraturaEvolutaController(IHttpContextAccessor httpContextAccessor, ITimbraturaEvolutaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Sedi")]
        public async Task<GenericResult<TimbraturaEvoluta_SediOutModel>> Sedi(GenericRequest<TimbraturaEvoluta_SediInModel> inModel) => await _service.Sedi(inModel, false);

        [Authorize, HttpPost, Route("ConfigGet")]
        public async Task<GenericResult<TimbraturaEvoluta_ConfigGetOutModel>> ConfigGet(GenericRequest<TimbraturaEvoluta_ConfigGetInModel> inModel) => await _service.ConfigGet(inModel, false);

        [Authorize, HttpPost, Route("ConfigPut")]
        public async Task<GenericResult<TimbraturaEvoluta_ConfigPutOutModel>> ConfigPut(GenericRequest<TimbraturaEvoluta_ConfigPutInModel> inModel) => await _service.ConfigPut(inModel, false);

        [Authorize, HttpPost, Route("QrCode")]
        public async Task<GenericResult<TimbraturaEvoluta_QrCodeOutModel>> QrCode(GenericRequest<TimbraturaEvoluta_QrCodeInModel> inModel) => await _service.QrCode(inModel, false);

        [Authorize, HttpPost, Route("Stamp")]
        public async Task<GenericResult<TimbraturaEvoluta_StampOutModel>> Stamp(GenericRequest<TimbraturaEvoluta_StampInModel> inModel) => await _service.Stamp(inModel, false);

        [Authorize, HttpPost, Route("KioskRegistra")]
        public async Task<GenericResult<TimbraturaEvoluta_KioskRegistraOutModel>> KioskRegistra(GenericRequest<TimbraturaEvoluta_KioskRegistraInModel> inModel) => await _service.KioskRegistra(inModel, false);

        [Authorize, HttpPost, Route("KioskStamp")]
        public async Task<GenericResult<TimbraturaEvoluta_KioskStampOutModel>> KioskStamp(GenericRequest<TimbraturaEvoluta_KioskStampInModel> inModel) => await _service.KioskStamp(inModel, false);

        [Authorize, HttpPost, Route("TerminaleSblocca")]
        public async Task<GenericResult<TimbraturaEvoluta_TerminaleSbloccaOutModel>> TerminaleSblocca(GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel> inModel) => await _service.TerminaleSblocca(inModel, false);

        [Authorize, HttpPost, Route("PinGetAll")]
        public async Task<GenericResult<TimbraturaEvoluta_PinGetAllOutModel>> PinGetAll(GenericRequest<TimbraturaEvoluta_PinGetAllInModel> inModel) => await _service.PinGetAll(inModel, false);

        [Authorize, HttpPost, Route("PinPut")]
        public async Task<GenericResult<TimbraturaEvoluta_PinPutOutModel>> PinPut(GenericRequest<TimbraturaEvoluta_PinPutInModel> inModel) => await _service.PinPut(inModel, false);

        [Authorize, HttpPost, Route("Info4Giorno")]
        public async Task<GenericResult<TimbraturaEvoluta_Info4GiornoOutModel>> Info4Giorno(GenericRequest<TimbraturaEvoluta_Info4GiornoInModel> inModel) => await _service.Info4Giorno(inModel, false);
    }
}
