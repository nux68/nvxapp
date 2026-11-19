using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Cartellino di un dipendente in un periodo (default il mese corrente, al massimo MaxGiorni giorni):
    // per ogni giorno ore lavorate e previste (Dip_GG_Result), timbrature, giustificativi, causali e anomalie,
    // gli stessi dati della pagina del cartellino (TimeSheet_EngineService.Get_Timesheet_AllData) e della pagina
    // "Anomalie presenze" (GestionePresenzeAnomalie). Il testo e' un riepilogo, il dettaglio giorno per giorno
    // e' nei dati strutturati (accesso AI, MCP-31).
    // L'HR consulta tutti, il CompanyAdmin anche i dipendenti dei reparti che amministra, gli altri solo sé stessi.
    public class EmployeeTimeSheetHandler : BaseCommandDipeHandler
    {
        public const int MaxGiorni = 31;
        private const int MaxRighe = 10;

        private readonly IntentDefinition _intentDefinition;
        private readonly ITimeSheet_EngineService _timeSheet_EngineService;
        private readonly IDip_AnomalieService _dip_AnomalieService;
        private readonly IGestionePresenzeAnomalie _anomalie;
        private readonly IPar_GiustificativiService _par_GiustificativiService;
        private readonly IPar_CausaliService _par_CausaliService;

        public EmployeeTimeSheetHandler(IDip_AnagraficaService dip_AnagraficaService,
                                        ITimeSheet_EngineService timeSheet_EngineService,
                                        IDip_AnomalieService dip_AnomalieService,
                                        IGestionePresenzeAnomalie anomalie,
                                        IPar_GiustificativiService par_GiustificativiService,
                                        IPar_CausaliService par_CausaliService,
                                        IChatUserContext chatUserContext
                                        ) : base(dip_AnagraficaService, chatUserContext)
        {
            _timeSheet_EngineService = timeSheet_EngineService;
            _dip_AnomalieService = dip_AnomalieService;
            _anomalie = anomalie;
            _par_GiustificativiService = par_GiustificativiService;
            _par_CausaliService = par_CausaliService;

            _intentDefinition = new IntentDefinition
            {
                Name = "EmployeeTimeSheet",
                Application = ApplicationType.AttendanceTracking,
                Category = AttendanceTrackingMessages.Get("ChatCategoryTimeSheet"),
                CategoryOrder = 600,
                DisplayName = AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin, RuoloUser },
                Description = AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetDescription", MaxGiorni),
                Keywords = new() { "cartellino", "il mio cartellino", "cartellino di", "riepilogo presenze", "time sheet", "timesheet", "my time sheet" },
                Examples = new() { "fammi vedere il cartellino di Rossi di ottobre", "il mio cartellino dal 1 al 15 novembre", "cartellino di Bianchi della settimana scorsa", "show my time sheet for this month" },
                RequiresConfirmation = false,
                Pages = new() { "/usertimesheet", "/admintimesheet", "/poweradmintimesheet" },
                Slots = BuildSlots(
                    Facoltativo(EmployeeNameSlot, ChatSelf.Token),
                    SlotData("from", false, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodFromQuestion"), AttendanceTrackingMessages.Get("ChatFromLabel"),
                        SignificatoDal + $"; se manca vale il primo giorno del mese (della data finale, se indicata, altrimenti del mese corrente); periodo massimo {MaxGiorni} giorni"),
                    SlotData("to", false, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodToQuestion"), AttendanceTrackingMessages.Get("ChatToLabel"),
                        SignificatoAl + "; se manca vale l'ultimo giorno del mese della data iniziale"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dipendente = await RisolviDipendente(slots.GetValueOrDefault("employeeName", ChatSelf.Token), _dip_AnagraficaService, _chatUserContext);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeSelfNotFound"));
            if (!await PuoVedere(dipendente))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOnlyOwnData"));

            var (dal, al) = Periodo(slots);
            if (al < dal)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetPeriodInvalid"));
            if ((al - dal).TotalDays + 1 > MaxGiorni)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetPeriodTooLong", MaxGiorni));

            var res = await _timeSheet_EngineService.Get_Timesheet_AllData(new GenericRequest<Timesheet_AllData_InModel>
            {
                Data = new() { UsersId = new List<string> { dipendente.IdAspNetUsers }, Dal = dal, Al = al }
            }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));
            var tutti = res.Data.Dip_GG_AllData_OutModel;

            var giustificativi = (await Giustificativi(_par_GiustificativiService)).ToDictionary(g => g.Id);
            var causali = await Causali();

            // anomalie dei giorni trascorsi, con le regole della pagina "Anomalie presenze"
            var anomalie = dal < DateTime.Today
                ? _anomalie.Analizza(new List<int> { dipendente.Id }, dal, al < DateTime.Today ? al : DateTime.Today.AddDays(-1), DateTime.Today)
                           .Giornate.GroupBy(g => g.Data.Date).ToDictionary(g => g.Key, g => g.SelectMany(x => x.Anomalie).ToList())
                : new Dictionary<DateTime, List<AnomaliaModel>>();

            var giorni = new List<CartellinoGiorno>();
            for (var giorno = dal; giorno <= al; giorno = giorno.AddDays(1))
            {
                var risultati = tutti.Dip_GG_Result.Where(r => r.Data.Date == giorno).ToList();
                int lavorati = risultati.Sum(r => (int)r.HH_Lav.ToTimeSpan().TotalMinutes);
                int previsti = risultati.Sum(r => (int)r.HH_Teo.ToTimeSpan().TotalMinutes);

                giorni.Add(new CartellinoGiorno
                {
                    Data = Iso(giorno),
                    GiornoSettimana = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(giorno.DayOfWeek),
                    Calcolato = risultati.Count > 0,
                    Festivo = risultati.Any(r => r.Festivo),
                    MinutiLavorati = lavorati,
                    MinutiPrevisti = previsti,
                    OreLavorate = Ore(lavorati),
                    OrePreviste = Ore(previsti),
                    MinutiStraordinario = risultati.Sum(r => r.Min_Strao + r.Min_StraoNott + r.Min_StraoFest + r.Min_StraoFestNott + r.Min_Suppl + r.Min_SupplEcc),
                    MinutiAssenza = risultati.Sum(r => r.Min_Assenze),
                    Timbrature = tutti.Dip_GG_Timbratura
                        .Where(t => t.GiornoCompetenza.Date == giorno && t.RichiestaStato != StatoRichiesta.Cancellata && t.RichiestaStato != StatoRichiesta.Rifiutata)
                        .OrderBy(t => t.Timbratura)
                        .Select(t => new CartellinoTimbratura
                        {
                            Data = Iso(t.Timbratura),
                            Ora = t.Timbratura.ToString("HH:mm", CultureInfo.InvariantCulture),
                            Verso = t.TimbraturaTipo.ToString(),
                            Stato = t.RichiestaStato.ToString(),
                            Valida = StatoValido(t.RichiestaStato)
                        }).ToList(),
                    Giustificativi = tutti.Dip_GG_Giustificativi
                        .Where(g => g.Data.Date == giorno && g.RichiestaStato != StatoRichiesta.Cancellata && g.RichiestaStato != StatoRichiesta.Rifiutata)
                        .Select(g =>
                        {
                            var par = giustificativi.GetValueOrDefault(g.IdPar_Giustificativi);
                            return new CartellinoGiustificativo
                            {
                                Codice = par?.Codice ?? string.Empty,
                                Descrizione = par?.Descrizione ?? par?.Codice ?? string.Empty,
                                Inserimento = g.InputType.ToString(),
                                Ore = g.Hours != null ? Ore((int)g.Hours.Value.TotalMinutes) : null,
                                Dalle = g.From != null ? g.From.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture) : null,
                                Importo = g.Importo,
                                Stato = g.RichiestaStato.ToString(),
                                Valido = StatoValido(g.RichiestaStato)
                            };
                        }).ToList(),
                    Causali = tutti.Dip_GG_Causali
                        .Where(c => c.Data.Date == giorno)
                        .Select(c =>
                        {
                            var par = causali.GetValueOrDefault(c.IdPar_Causali);
                            return new CartellinoCausale
                            {
                                Codice = par?.Codice ?? string.Empty,
                                Descrizione = par?.Descrizione ?? par?.Codice ?? string.Empty,
                                Ore = par?.Tipo == TipoValore.Importo ? null : c.Valore.ToString("HH:mm", CultureInfo.InvariantCulture),
                                Importo = c.Importo
                            };
                        }).ToList(),
                    Anomalie = (anomalie.GetValueOrDefault(giorno) ?? new())
                        .Select(a => new CartellinoAnomalia
                        {
                            Tipo = a.Tipo.ToString(),
                            DaSistemare = a.Gravita == AnomaliaGravita.Errore,
                            Descrizione = a.Descrizione
                        }).ToList()
                });
            }

            var trascorsi = giorni.Where(g => DateTime.ParseExact(g.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= DateTime.Today).ToList();
            int totLavorati = trascorsi.Sum(g => g.MinutiLavorati);
            int totPrevisti = trascorsi.Sum(g => g.MinutiPrevisti);
            var conAnomalie = giorni.Where(g => g.Anomalie.Any(a => a.DaSistemare)).ToList();

            var dati = new CartellinoDati
            {
                Dipendente = Nome(dipendente),
                Matricola = dipendente.Matricola,
                Dal = Iso(dal),
                Al = Iso(al),
                MinutiLavorati = totLavorati,
                MinutiPrevisti = totPrevisti,
                OreLavorate = Ore(totLavorati),
                OrePreviste = Ore(totPrevisti),
                GiorniDaSistemare = conAnomalie.Count,
                Giorni = giorni
            };

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetTitle", Nome(dipendente), Data(dal), Data(al), Ore(totLavorati), Ore(totPrevisti)));

            var assenze = giorni.SelectMany(g => g.Giustificativi.Where(x => x.Valido)).GroupBy(x => x.Descrizione)
                                .Select(g => $"{g.Key} ({g.Count()})").ToList();
            if (assenze.Count > 0)
                sb.AppendLine(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetJustifications", Elenco(assenze)));

            if (conAnomalie.Count == 0)
                sb.AppendLine(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetNoAnomalies"));
            else
            {
                sb.AppendLine(AttendanceTrackingMessages.Get("ChatEmployeeTimeSheetAnomalies", conAnomalie.Count));
                foreach (var g in conAnomalie.Take(MaxRighe))
                    sb.AppendLine($"  • {Data(DateTime.ParseExact(g.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture))}: {string.Join("; ", g.Anomalie.Where(a => a.DaSistemare).Select(a => a.Descrizione))}");
                if (conAnomalie.Count > MaxRighe)
                    sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", conAnomalie.Count - MaxRighe));
            }

            return CommandResult.Ok(sb.ToString().TrimEnd(), dati);
        }

        // come le pagine: sé stessi; l'HR tutti; il CompanyAdmin i dipendenti dei reparti che amministra
        private async Task<bool> PuoVedere(Dip_AnagraficaModel dipendente)
        {
            if (PuoAccedere(_chatUserContext, dipendente)) return true;
            if (!_chatUserContext.IsInRole(RuoloAdmin)) return false;
            var visibili = await _dip_AnomalieService.DipendentiVisibili(new List<string> { dipendente.IdAspNetUsers }, DateTime.Today);
            return visibili.Contains(dipendente.Id);
        }

        // periodo dagli slot: nessuna data = mese corrente; una sola data = il suo mese a partire da / fino a quella data
        private static (DateTime Dal, DateTime Al) Periodo(Dictionary<string, string> slots)
        {
            DateTime? dal = !string.IsNullOrWhiteSpace(slots.GetValueOrDefault("from")) ? ParseData(slots["from"]) : null;
            DateTime? al = !string.IsNullOrWhiteSpace(slots.GetValueOrDefault("to")) ? ParseData(slots["to"]) : null;

            if (dal == null && al == null)
            {
                var inizio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                return (inizio, inizio.AddMonths(1).AddDays(-1));
            }
            if (dal == null)
                return (new DateTime(al!.Value.Year, al.Value.Month, 1), al.Value);
            if (al == null)
                return (dal.Value, new DateTime(dal.Value.Year, dal.Value.Month, 1).AddMonths(1).AddDays(-1));
            return (dal.Value, al.Value);
        }

        private async Task<Dictionary<int, Par_CausaliModel>> Causali()
        {
            var res = await _par_CausaliService.GetAll(new GenericRequest<Par_CausaliInModel> { Data = new() }, true);
            return res.Success && res.Data != null ? res.Data.Par_Causali.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First()) : new();
        }

        private static string Iso(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    // ---------------------------------------------------------------------------
    // Dati strutturati del cartellino (accesso AI): date yyyy-MM-dd, orari HH:mm, durate H:mm e minuti
    // ---------------------------------------------------------------------------

    public class CartellinoDati
    {
        public string Dipendente { get; set; } = string.Empty;
        public string? Matricola { get; set; }
        public string Dal { get; set; } = string.Empty;
        public string Al { get; set; } = string.Empty;
        // totali dei soli giorni fino a oggi (i giorni futuri hanno le ore previste ma niente di lavorato)
        public int MinutiLavorati { get; set; }
        public int MinutiPrevisti { get; set; }
        public string OreLavorate { get; set; } = string.Empty;
        public string OrePreviste { get; set; } = string.Empty;
        public int GiorniDaSistemare { get; set; }
        public List<CartellinoGiorno> Giorni { get; set; } = new();
    }

    public class CartellinoGiorno
    {
        public string Data { get; set; } = string.Empty;
        public string GiornoSettimana { get; set; } = string.Empty;
        // false = giorno non ancora calcolato dal motore delle presenze (ore a zero)
        public bool Calcolato { get; set; }
        public bool Festivo { get; set; }
        public int MinutiLavorati { get; set; }
        public int MinutiPrevisti { get; set; }
        public string OreLavorate { get; set; } = string.Empty;
        public string OrePreviste { get; set; } = string.Empty;
        public int MinutiStraordinario { get; set; }
        public int MinutiAssenza { get; set; }
        public List<CartellinoTimbratura> Timbrature { get; set; } = new();
        public List<CartellinoGiustificativo> Giustificativi { get; set; } = new();
        public List<CartellinoCausale> Causali { get; set; } = new();
        public List<CartellinoAnomalia> Anomalie { get; set; } = new();
    }

    public class CartellinoTimbratura
    {
        public string Data { get; set; } = string.Empty;
        public string Ora { get; set; } = string.Empty;
        // Entrata / Uscita / SenzaVerso
        public string Verso { get; set; } = string.Empty;
        // Diretta / Approvata = valida; Immessa, ApprovazioneInCorso, ParzialmenteApprovata = richiesta in attesa
        public string Stato { get; set; } = string.Empty;
        public bool Valida { get; set; }
    }

    public class CartellinoGiustificativo
    {
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        // Manual (ore indicate) / AllDay (giornata intera) / IntegrateDay (a completamento della giornata)
        public string Inserimento { get; set; } = string.Empty;
        public string? Ore { get; set; }
        public string? Dalle { get; set; }
        public decimal? Importo { get; set; }
        public string Stato { get; set; } = string.Empty;
        public bool Valido { get; set; }
    }

    public class CartellinoCausale
    {
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public string? Ore { get; set; }
        public decimal? Importo { get; set; }
    }

    public class CartellinoAnomalia
    {
        public string Tipo { get; set; } = string.Empty;
        // true = da sistemare prima dell'export paghe; false = da verificare
        public bool DaSistemare { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }
}
