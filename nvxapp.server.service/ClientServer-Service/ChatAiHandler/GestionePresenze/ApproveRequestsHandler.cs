using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Approva o rifiuta le richieste in attesa di un dipendente (facoltativo: solo quelle di un giorno
    // o di un tipo). Il riepilogo elenca le richieste che verranno decise, prima della conferma.
    // Solo HR: la decisione è definitiva (FromHR), come dalla pagina delle richieste.
    // Il dipendente è obbligatorio: niente approvazioni di tutta l'azienda con una frase.
    public class ApproveRequestsHandler : BaseCommandDipeHandler
    {
        public const string Approve = "approve";
        public const string Reject = "reject";

        private static readonly Dictionary<string, string> _decisioni = new(StringComparer.OrdinalIgnoreCase)
        {
            ["approva"] = Approve, ["approvare"] = Approve, ["approvale"] = Approve, ["approvo"] = Approve, ["accetta"] = Approve,
            ["approve"] = Approve, ["accept"] = Approve, ["ok"] = Approve,
            ["rifiuta"] = Reject, ["rifiutare"] = Reject, ["rifiutale"] = Reject, ["respingi"] = Reject, ["nega"] = Reject,
            ["reject"] = Reject, ["deny"] = Reject, ["decline"] = Reject,
        };

        // tipo di richiesta detto dall'utente -> TipoRichiesta
        private static readonly Dictionary<string, TipoRichiesta> _tipi = new(StringComparer.OrdinalIgnoreCase)
        {
            ["timbratura"] = TipoRichiesta.Timbratura, ["timbrature"] = TipoRichiesta.Timbratura, ["clocking"] = TipoRichiesta.Timbratura,
            ["giustificativo"] = TipoRichiesta.Giustificativo, ["giustificativi"] = TipoRichiesta.Giustificativo, ["ferie"] = TipoRichiesta.Giustificativo,
            ["permesso"] = TipoRichiesta.Giustificativo, ["permessi"] = TipoRichiesta.Giustificativo, ["malattia"] = TipoRichiesta.Giustificativo,
            ["rol"] = TipoRichiesta.Giustificativo, ["leave"] = TipoRichiesta.Giustificativo, ["holiday"] = TipoRichiesta.Giustificativo,
            ["spese"] = TipoRichiesta.NotaSpesa, ["spesa"] = TipoRichiesta.NotaSpesa, ["expense"] = TipoRichiesta.NotaSpesa, ["expenses"] = TipoRichiesta.NotaSpesa,
            ["straordinario"] = TipoRichiesta.ApprovazioneStraordinario, ["straordinari"] = TipoRichiesta.ApprovazioneStraordinario, ["overtime"] = TipoRichiesta.ApprovazioneStraordinario,
        };

        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public ApproveRequestsHandler(IDip_AnagraficaService dip_AnagraficaService,
                                      IDip_GG_RichiestaService dip_GG_RichiestaService,
                                      IPar_GiustificativiService par_GiustificativiService,
                                      IChatUserContext chatUserContext
                                      ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "ApproveRequests",
                Category = AttendanceTrackingMessages.Get("ChatCategoryRequests"),
                CategoryOrder = 400,
                DisplayName = AttendanceTrackingMessages.Get("ChatApproveRequestsDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatApproveRequestsDescription"),
                // non i valori dello slot ("approva", "rifiuta"): un valore uguale a una parola chiave viene scartato
                Keywords = new() { "richieste", "richiesta", "requests", "request" },
                Examples = new() { "approva tutte le richieste di Rossi", "rifiuta la richiesta di Bianchi del 3 novembre", "approva la timbratura di Verdi", "approve Rossi's requests" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatApproveRequestsSummary"),
                Pages = new() { "/requestlistadmin" },
                // le richieste che verranno decise compaiono nel riepilogo (o, se non ce ne sono, lo si dice subito)
                CrossValidator = ElencoDaDecidere,
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new SlotDefinition
                    {
                        Name = "decision",
                        Type = "approve/reject",
                        Required = true,
                        PromptDescription = "decisione come detta dall'utente (es. 'approva', 'rifiuta', 'approve', 'reject')",
                        Question = AttendanceTrackingMessages.Get("ChatApproveDecisionQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatApproveDecisionLabel"),
                        Normalizer = v => v.Split(new[] { ' ', '\'', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(w => _decisioni.GetValueOrDefault(w)).FirstOrDefault(x => x != null),
                        Validator = v => v is Approve or Reject
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatApproveDecisionQuestion"), "decision")
                    },
                    SlotData("date", false, string.Empty, AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")),
                    new SlotDefinition
                    {
                        Name = "requestType",
                        Type = "string",
                        Required = false,
                        PromptDescription = "tipo di richiesta, solo se indicato: timbratura, giustificativo (ferie, permesso, malattia, ROL), nota spese, straordinario",
                        Question = AttendanceTrackingMessages.Get("ChatApproveTypeQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatApproveTypeLabel"),
                        Normalizer = v => ChatAIKeywords.Normalize(v).Split(' ')
                                           .Select(w => _tipi.TryGetValue(w, out var t) ? t.ToString() : null).FirstOrDefault(x => x != null),
                        Validator = v => Enum.TryParse<TipoRichiesta>(v, out _)
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatApproveTypeQuestion"), "requestType")
                    })
            };
        }

        // richieste in attesa del dipendente, filtrate per giorno e tipo se indicati
        private async Task<(Dip_AnagraficaModel? Dipendente, List<Dip_GG_RichiestaModel> Richieste)> DaDecidere(Dictionary<string, string> slots)
        {
            var dipendente = await RisolviDipendente(slots.GetValueOrDefault("employeeName"), _dip_AnagraficaService, _chatUserContext);
            if (dipendente == null) return (null, new());

            DateTime? giorno = slots.TryGetValue("date", out var d) ? ParseData(d) : null;
            TipoRichiesta? tipo = slots.TryGetValue("requestType", out var t) && Enum.TryParse<TipoRichiesta>(t, out var tt) ? tt : null;
            var rapporti = dipendente.Dip_RapportoLavoro.Select(r => r.Id).ToHashSet();

            var richieste = (await ChatRichiesteHelper.InAttesa(_dip_GG_RichiestaService))
                .Where(r => rapporti.Contains(r.IdDip_RapportoLavoro))
                .Where(r => giorno == null || ChatRichiesteHelper.Comprende(r, giorno.Value))
                .Where(r => tipo == null || r.RichiestaTipo == tipo)
                .ToList();
            return (dipendente, richieste);
        }

        // Validazione dell'azione completa (sincrona, come i Validator): elenco delle richieste nel riepilogo
        private SlotValidationResult? ElencoDaDecidere(Dictionary<string, string> slots)
        {
            if (!slots.ContainsKey("employeeName")) return null;
            var (dipendente, richieste) = DaDecidere(slots).GetAwaiter().GetResult();
            if (dipendente == null) return null;

            if (richieste.Count == 0)
                return SlotValidationResult.Failed(SlotValidationError.BusinessRuleViolation,
                    AttendanceTrackingMessages.Get("ChatApproveNone", Nome(dipendente), string.Empty), "employeeName");

            var nomi = Giustificativi(_par_GiustificativiService).GetAwaiter().GetResult()
                .ToDictionary(g => g.Id, g => g.Descrizione ?? g.Codice ?? string.Empty);
            var elenco = string.Join("; ", richieste.Select(r => ChatRichiesteHelper.Descrivi(r, dipendente, nomi)));
            return SlotValidationResult.Ok(string.Empty, AttendanceTrackingMessages.Get("ChatApproveList", richieste.Count, elenco));
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (dipendente, richieste) = await DaDecidere(slots);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", slots.GetValueOrDefault("employeeName", "-")));

            var approva = slots.GetValueOrDefault("decision", Approve) == Approve;
            DateTime? giorno = slots.TryGetValue("date", out var d) ? ParseData(d) : null;
            var ids = richieste.Select(r => r.Id).ToList();

            if (ids.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatApproveNone", Nome(dipendente),
                    giorno != null ? " " + AttendanceTrackingMessages.Get("ChatOnDate", Data(giorno.Value)) : string.Empty));

            var res = await _dip_GG_RichiestaService.SetState(new GenericRequest<Dip_GG_Richiesta_SetState_InModel>
            {
                Data = new()
                {
                    IdDip_GG_Richiesta = ids,
                    RichiestaStato = approva ? StatoRichiesta.Approvata : StatoRichiesta.Rifiutata,
                    FromHR = true,
                    ExcludeRicalc = false
                }
            }, false);

            if (!res.Success)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatApproveError", string.Join(", ", res.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get(approva ? "ChatApproveDone" : "ChatRejectDone", ids.Count, Nome(dipendente)));
        }
    }
}
