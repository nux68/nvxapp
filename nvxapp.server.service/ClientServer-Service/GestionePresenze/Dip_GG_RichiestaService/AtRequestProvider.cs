using System.Globalization;
using Newtonsoft.Json.Linq;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{
    /*
     Richieste di Gestione Presenze (timbrature, giustificativi, note spese, scambi turno) nelle pagine comuni delle richieste
     (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-AT.1). Riusa Dip_GG_RichiestaService: modello dati, approvatori, strategie
     di approvazione, revoca ed effetti sul cartellino restano quelli di Presenze.
        - elenco e contatori come sottoprocesso (come la chat AI); approva / rifiuta / annulla come azioni del client
          (isSubProcess = false): SetState controlla i permessi, verifica il rapporto di lavoro e avvisa gli interessati
        - Mine: richieste dell'utente (escluse le cancellate); Admin: richieste dell'azienda viste dall'approvatore
          (Get_Dip_GG_Richiesta porta la fase corrente allo stato personale dell'approvatore: Immessa = tocca a lui)
    */
    public class AtRequestProvider : IRequestProvider
    {
        public const string Codice = "at";

        private readonly IDip_GG_RichiestaService _service;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public AtRequestProvider(IDip_GG_RichiestaService service,
                                 IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                 IDip_AnagraficaRepository dip_AnagraficaRepository,
                                 IPar_GiustificativiRepository par_GiustificativiRepository)
        {
            _service = service;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        public ApplicationType Application => ApplicationType.AttendanceTracking;

        public string Prefix => Codice;

        public async Task<RequestCounts> CountAsync(bool admin)
        {
            var res = await _service.RichiestaCount(new GenericRequest<Dip_GG_Richiesta_Count_InModel> { Data = new() { Admin = admin } }, true);
            return res.Success && res.Data != null
                ? new RequestCounts { DaApprovare = res.Data.DaApprovare, InAttesa = res.Data.InAttesa }
                : new RequestCounts();
        }

        public async Task<List<RequestItem>> ListAsync(RequestScope scope, RequestFilter filtro)
        {
            List<Dip_GG_RichiestaModel> richieste;
            if (scope == RequestScope.Admin)
            {
                var res = await _service.GetAll4Admin(new GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel>
                {
                    Data = new()
                    {
                        Filtro = filtro switch
                        {
                            RequestFilter.DaApprovare => Dip_GG_Richiesta_Filtro.DaApprovare,
                            RequestFilter.Approvate => Dip_GG_Richiesta_Filtro.Approvate,
                            _ => Dip_GG_Richiesta_Filtro.Tutte
                        }
                    }
                }, true);
                richieste = res.Success && res.Data != null ? res.Data.Dip_GG_Richiesta : new();
            }
            else
            {
                var res = await _service.GetAll4User(new GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>(), true);
                richieste = res.Success && res.Data != null ? res.Data.Dip_GG_Richiesta : new();
                if (filtro == RequestFilter.DaApprovare)
                    richieste = richieste.Where(x => InCorso(x.RevocaStato ?? x.RichiestaStato)).ToList();
                else if (filtro == RequestFilter.Approvate)
                    richieste = richieste.Where(x => Approvato(x.RevocaStato ?? x.RichiestaStato)).ToList();
            }

            // le cancellate non si mostrano (come nelle pagine di Presenze)
            richieste = richieste.Where(x => x.RichiestaStato != StatoRichiesta.Cancellata).ToList();
            if (richieste.Count == 0)
                return new List<RequestItem>();

            // nomi dei richiedenti (rapporto di lavoro -> anagrafica in Common) e dei giustificativi
            var idRapporti = richieste.Select(x => x.IdDip_RapportoLavoro).Distinct().ToList();
            var rapporti = _dip_RapportoLavoroRepository.FindAll(x => idRapporti.Contains(x.Id))
                                                         .Select(x => new { x.Id, x.IdDip_Anagrafica }).ToList();
            var idAnagrafiche = rapporti.Select(x => x.IdDip_Anagrafica).Distinct().ToList();
            var anagrafiche = _dip_AnagraficaRepository.FindAll(x => idAnagrafiche.Contains(x.Id))
                                                       .Select(x => new { x.Id, x.Cognome, x.Nome }).ToList();
            var nomi = rapporti.ToDictionary(r => r.Id, r =>
            {
                var a = anagrafiche.FirstOrDefault(x => x.Id == r.IdDip_Anagrafica);
                return a == null ? null : $"{a.Cognome} {a.Nome}".Trim();
            });
            var giustificativi = richieste.Any(x => x.RichiestaTipo == TipoRichiesta.Giustificativo)
                ? _par_GiustificativiRepository.FindAll(x => true).Select(x => new { x.Id, x.Codice, x.Descrizione }).ToList()
                      .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.Descrizione) ? (x.Codice ?? string.Empty) : $"{x.Codice} - {x.Descrizione}")
                : new Dictionary<int, string>();

            return richieste.GroupBy(x => x.Id).Select(g => g.First()).Select(r =>
            {
                var corrente = r.RevocaStato ?? r.RichiestaStato;
                var dal = Data(r.Data) ?? DateTime.MinValue;
                var al = Data(r.DataA);
                return new RequestItem
                {
                    Id = r.Id.ToString(CultureInfo.InvariantCulture),
                    Tipo = r.RichiestaTipo.ToString(),
                    TipoTesto = AttendanceTrackingMessages.Get("RequestType_" + r.RichiestaTipo),
                    Richiedente = nomi.GetValueOrDefault(r.IdDip_RapportoLavoro),
                    Dal = dal,
                    Al = al != null && al.Value > dal ? al : null,
                    SoloData = true,
                    Dettaglio = Dettaglio(r, giustificativi),
                    Stato = StatoComune(r),
                    StatoTesto = r.RevocaStato == null
                        ? TestoStato(r.RichiestaStato)
                        : AttendanceTrackingMessages.Get("RequestStateWithRevoke", TestoStato(r.RichiestaStato), TestoStato(r.RevocaStato.Value)),
                    // come Dip_GG_Richiesta_Admin_Can_Approve/Reject: la fase corrente e' "Immessa" per questo approvatore
                    CanApprove = scope == RequestScope.Admin && corrente == StatoRichiesta.Immessa,
                    CanReject = scope == RequestScope.Admin && corrente == StatoRichiesta.Immessa,
                    // come Dip_GG_Richiesta_User_Can_Delete
                    CanCancel = scope == RequestScope.Mine && PuoAnnullare(r),
                    Extra = r.RichiestaTipo == TipoRichiesta.NotaSpesa ? "at.notaSpesa" : null,
                };
            }).ToList();
        }

        public Task ApproveAsync(string id) => SetStateAsync(id, StatoRichiesta.Approvata);

        public Task RejectAsync(string id, string? motivo) => SetStateAsync(id, StatoRichiesta.Rifiutata);

        // come la pagina dell'utente: richiesta approvata -> revoca (Immessa), altrimenti cancellazione
        public async Task CancelAsync(string id)
        {
            var res = await _service.GetAll4User(new GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>(), true);
            var r = (res.Success && res.Data != null ? res.Data.Dip_GG_Richiesta : new()).FirstOrDefault(x => x.Id.ToString(CultureInfo.InvariantCulture) == id)
                    ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));
            if (!PuoAnnullare(r))
                throw new UserMessageException(AttendanceTrackingMessages.Get("RequestNotAllowedCancel"));
            bool revoca = r.RichiestaStato == StatoRichiesta.Approvata && (r.RevocaStato == null || r.RevocaStato == StatoRichiesta.Cancellata);
            await SetStateAsync(id, revoca ? StatoRichiesta.Immessa : StatoRichiesta.Cancellata);
        }

        // ---------------------------------------------------------------------------

        private async Task SetStateAsync(string id, StatoRichiesta stato)
        {
            if (!int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out int idRichiesta))
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

            var res = await _service.SetState(new GenericRequest<Dip_GG_Richiesta_SetState_InModel>
            {
                Data = new() { IdDip_GG_Richiesta = new List<int> { idRichiesta }, RichiestaStato = stato }
            }, false);
            if (!res.Success)
                throw new UserMessageException(string.Join(" ", res.Messages.Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text)));
        }

        private static bool InCorso(StatoRichiesta s)
            => s == StatoRichiesta.Immessa || s == StatoRichiesta.ApprovazioneInCorso || s == StatoRichiesta.ParzialmenteApprovata;

        private static bool Approvato(StatoRichiesta s) => s == StatoRichiesta.Approvata || s == StatoRichiesta.Diretta;

        private static bool PuoAnnullare(Dip_GG_RichiestaModel r)
            => r.RevocaStato == null
                ? r.RichiestaStato == StatoRichiesta.Immessa || r.RichiestaStato == StatoRichiesta.ApprovazioneInCorso || r.RichiestaStato == StatoRichiesta.Approvata
                : r.RevocaStato == StatoRichiesta.Immessa || r.RevocaStato == StatoRichiesta.ApprovazioneInCorso;

        // stato comune: con una revoca in corso la richiesta resta "in attesa"; revoca approvata = annullata
        private static RequestState StatoComune(Dip_GG_RichiestaModel r)
        {
            if (r.RevocaStato != null)
            {
                if (InCorso(r.RevocaStato.Value))
                    return r.RevocaStato == StatoRichiesta.Immessa ? RequestState.InAttesa : RequestState.InApprovazione;
                if (r.RevocaStato == StatoRichiesta.Approvata)
                    return RequestState.Annullata;
            }
            return r.RichiestaStato switch
            {
                StatoRichiesta.Immessa => RequestState.InAttesa,
                StatoRichiesta.ApprovazioneInCorso or StatoRichiesta.ParzialmenteApprovata => RequestState.InApprovazione,
                StatoRichiesta.Approvata or StatoRichiesta.Diretta => RequestState.Approvata,
                StatoRichiesta.Rifiutata => RequestState.Rifiutata,
                _ => RequestState.Annullata
            };
        }

        private static string TestoStato(StatoRichiesta s) => AttendanceTrackingMessages.Get("RequestState_" + s);

        // come TimeSheetUtilityService.getDatitext del client
        private static string? Dettaglio(Dip_GG_RichiestaModel r, IReadOnlyDictionary<int, string> giustificativi)
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

        // data del modello ("dd/MM/yyyy")
        private static DateTime? Data(string? valore)
            => DateTime.TryParseExact(valore, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}
