using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{
    /*
     Tipi di richiesta di Gestione Presenze (timbrature, giustificativi, note spese, straordinari, scambi turno) nelle pagine
     comuni delle richieste (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-AT.6). Le richieste stanno nella tabella comune
     Dip_GG_Richiesta: elenco e "chi puo' decidere" li calcola RequestService con la cronologia degli approvatori; qui:
        - la strategia dell'azienda (Az_Cfg.ApprovazioneTipo: SigleAdmin -> Singola, AllAdmin -> Tutti, AllAdminHierarchy -> Gerarchia)
        - i testi della riga con le resx delle presenze (tipo, dettaglio, stato con la revoca), Extra "at.notaSpesa"
        - approva / rifiuta / annulla con il motore delle presenze (Dip_GG_RichiestaService.SetState, azione del client:
          controlla i permessi, verifica il rapporto di lavoro, aggiorna i dettagli e il cartellino, avvisa gli interessati)
    */
    public class AtRichiestaTipoHandler : IRichiestaTipoHandler
    {
        private static readonly TipoRichiesta[] _tipi =
        {
            TipoRichiesta.Timbratura, TipoRichiesta.Giustificativo, TipoRichiesta.NotaSpesa,
            TipoRichiesta.ApprovazioneStraordinario, TipoRichiesta.ScambioTurno
        };

        private readonly IDip_GG_RichiestaService _service;
        private readonly IAz_CfgService _az_CfgService;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public AtRichiestaTipoHandler(IDip_GG_RichiestaService service,
                                      IAz_CfgService az_CfgService,
                                      IPar_GiustificativiRepository par_GiustificativiRepository)
        {
            _service = service;
            _az_CfgService = az_CfgService;
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        public ApplicationType Application => ApplicationType.AttendanceTracking;

        public IReadOnlyCollection<TipoRichiesta> Tipi => _tipi;

        public async Task<StrategiaApprovazione> StrategiaAsync()
        {
            var res = await _az_CfgService.Az_CfgGet(new GenericRequest<Az_Cfg_Get_InModel>(), true);
            return (res.Success ? res.Data?.Az_Cfg?.ApprovazioneTipo : null) switch
            {
                TipoApprovazione.AllAdmin => StrategiaApprovazione.Tutti,
                TipoApprovazione.AllAdminHierarchy => StrategiaApprovazione.Gerarchia,
                _ => StrategiaApprovazione.Singola
            };
        }

        public async Task CompletaAsync(List<(Dip_GG_Richiesta Richiesta, RequestItem Item)> righe, RequestScope scope)
        {
            var giustificativi = righe.Any(x => x.Richiesta.RichiestaTipo == TipoRichiesta.Giustificativo)
                ? (await _par_GiustificativiRepository.FindAll(x => true).Select(x => new { x.Id, x.Codice, x.Descrizione }).AsNoTracking().ToListAsync())
                      .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.Descrizione) ? (x.Codice ?? string.Empty) : $"{x.Codice} - {x.Descrizione}")
                : new Dictionary<int, string>();

            foreach (var (r, item) in righe)
            {
                item.TipoTesto = AttendanceTrackingMessages.Get("RequestType_" + r.RichiestaTipo);
                item.Dettaglio = Dettaglio(r, giustificativi);
                // testo comune a tutti i tipi di richiesta (es. "Approvata (annullamento respinto)")
                item.StatoTesto = RichiestaRegole.TestoStato(r);
                item.SoloData = true;
                if (item.Al != null && item.Al.Value <= item.Dal)
                    item.Al = null;
                // come la pagina delle presenze: si annulla in attesa, si chiede la revoca di una approvata
                item.CanCancel = item.CanCancel && PuoAnnullare(r);
                item.Extra = r.RichiestaTipo == TipoRichiesta.NotaSpesa ? "at.notaSpesa" : null;
            }
        }

        public Task ApproveAsync(Dip_GG_Richiesta richiesta) => SetStateAsync(richiesta.Id, StatoRichiesta.Approvata);

        public Task RejectAsync(Dip_GG_Richiesta richiesta, string? motivo) => SetStateAsync(richiesta.Id, StatoRichiesta.Rifiutata);

        // come la pagina dell'utente: richiesta approvata -> revoca (Immessa), altrimenti cancellazione
        public async Task CancelAsync(Dip_GG_Richiesta richiesta)
        {
            if (!PuoAnnullare(richiesta))
                throw new UserMessageException(AttendanceTrackingMessages.Get("RequestNotAllowedCancel"));
            bool revoca = RichiestaRegole.PuoChiedereAnnullamento(richiesta);
            await SetStateAsync(richiesta.Id, revoca ? StatoRichiesta.Immessa : StatoRichiesta.Cancellata);
        }

        // ---------------------------------------------------------------------------

        private async Task SetStateAsync(int idRichiesta, StatoRichiesta stato)
        {
            var res = await _service.SetState(new GenericRequest<Dip_GG_Richiesta_SetState_InModel>
            {
                Data = new() { IdDip_GG_Richiesta = new List<int> { idRichiesta }, RichiestaStato = stato }
            }, false);
            if (!res.Success)
                throw new UserMessageException(string.Join(" ", res.Messages.Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text)));
            // SetState ignora le fasi gia' decise: per chi agisce dalla pagina e' "non trovata"
            if (res.Data == null || !res.Data.Dip_GG_Richiesta.Any(x => x.Id == idRichiesta))
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));
        }

        // ciclo unico delle richieste (come Dip_GG_Richiesta_User_Can_Delete del client)
        private static bool PuoAnnullare(Dip_GG_Richiesta r) => RichiestaRegole.PuoAnnullare(r);

        private static string TestoStato(StatoRichiesta s) => AttendanceTrackingMessages.Get("RequestState_" + s);

        // come TimeSheetUtilityService.getDatitext del client
        private static string? Dettaglio(Dip_GG_Richiesta r, IReadOnlyDictionary<int, string> giustificativi)
        {
            JObject? dati = null;
            try { dati = string.IsNullOrEmpty(r.Dati) ? null : JObject.Parse(r.Dati); } catch { }
            if (dati == null)
                return null;
            string? Campo(string nome) => dati.GetValue(nome, StringComparison.OrdinalIgnoreCase)?.ToString();
            decimal? Numero(string nome)
            {
                var v = dati.GetValue(nome, StringComparison.OrdinalIgnoreCase);
                if (v == null || v.Type == JTokenType.Null) return null;
                if (v.Type == JTokenType.Integer || v.Type == JTokenType.Float) return v.Value<decimal>();
                return decimal.TryParse(v.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
            }

            switch (r.RichiestaTipo)
            {
                case TipoRichiesta.Timbratura:
                    return Campo("hhmm");
                case TipoRichiesta.Giustificativo:
                {
                    var nome = int.TryParse(Campo("idPar_Giustificativi"), out var idG) ? giustificativi.GetValueOrDefault(idG, string.Empty) : string.Empty;
                    if (Numero("importo") is decimal importo && importo > 0)
                        return $"{nome} € {importo.ToString("0.00", CultureInfo.CurrentCulture)}".Trim();
                    if (bool.TryParse(Campo("allDay"), out var allDay) && allDay)
                        return $"{nome} - {AttendanceTrackingMessages.Get("ChatRequestAllDay")}".Trim(' ', '-');
                    return $"{nome} - {AttendanceTrackingMessages.Get("RequestHours", Campo("hhmm") ?? "?")}".Trim(' ', '-');
                }
                case TipoRichiesta.NotaSpesa:
                {
                    var totale = Numero("totale") ?? 0;
                    return $"{Campo("descrizione")} € {totale.ToString("0.00", CultureInfo.CurrentCulture)}".Trim();
                }
                case TipoRichiesta.ScambioTurno:
                    return Campo("testo");
                default:
                    return null;
            }
        }
    }
}
