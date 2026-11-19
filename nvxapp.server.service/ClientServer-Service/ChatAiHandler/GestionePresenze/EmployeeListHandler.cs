using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Elenco dei dipendenti visibili (HR: tutti; CompanyAdmin: quelli dei reparti che amministra, come anomalie e report)
    // con reparto e sede di oggi e il rapporto di lavoro attivo oggi (contratto, ore settimanali, profilo orario e
    // orario del giorno). Filtri facoltativi: testo (cognome, nome, matricola) e reparto/sede.
    // Il testo e' un riepilogo, l'elenco completo (al massimo MaxDipendenti) e' nei dati strutturati (accesso AI, MCP-31).
    public class EmployeeListHandler : BaseCommandHandler
    {
        public const int MaxDipendenti = 200;
        private const int MaxRighe = 20;

        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_AnagraficaService _dip_AnagraficaService;
        private readonly IDip_AnomalieService _dip_AnomalieService;
        private readonly IAz_SediService _az_SediService;
        private readonly IAz_SediRepartoService _az_SediRepartoService;
        private readonly IAz_SediRepartoUserService _az_SediRepartoUserService;
        private readonly ITimeSheet_EngineService _timeSheet_EngineService;

        public EmployeeListHandler(IDip_AnagraficaService dip_AnagraficaService,
                                   IDip_AnomalieService dip_AnomalieService,
                                   IAz_SediService az_SediService,
                                   IAz_SediRepartoService az_SediRepartoService,
                                   IAz_SediRepartoUserService az_SediRepartoUserService,
                                   ITimeSheet_EngineService timeSheet_EngineService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
            _dip_AnomalieService = dip_AnomalieService;
            _az_SediService = az_SediService;
            _az_SediRepartoService = az_SediRepartoService;
            _az_SediRepartoUserService = az_SediRepartoUserService;
            _timeSheet_EngineService = timeSheet_EngineService;

            _intentDefinition = new IntentDefinition
            {
                Name = "EmployeeList",
                Application = ApplicationType.AttendanceTracking,
                Category = AttendanceTrackingMessages.Get("ChatCategoryAttendance"),
                CategoryOrder = 500,
                DisplayName = AttendanceTrackingMessages.Get("ChatEmployeeListDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin },
                Description = AttendanceTrackingMessages.Get("ChatEmployeeListDescription"),
                Keywords = new() { "elenco dipendenti", "lista dipendenti", "quali dipendenti", "dipendenti del reparto", "employee list", "list of employees", "employees in department" },
                Examples = new() { "elenco dei dipendenti", "quali dipendenti ci sono nel reparto Produzione?", "cerca il dipendente Rossi", "list the employees of the Milano site" },
                RequiresConfirmation = false,
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "search",
                        Type = "string",
                        Required = false,
                        PromptDescription = "testo da cercare in cognome, nome o matricola del dipendente (es. 'Rossi'); ometti lo slot per tutti i dipendenti",
                        Question = AttendanceTrackingMessages.Get("ChatEmployeeListSearchQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatEmployeeListSearchLabel")
                    },
                    new SlotDefinition
                    {
                        Name = "department",
                        Type = "string",
                        Required = false,
                        PromptDescription = "nome (anche parziale) del reparto o della sede (es. 'Produzione', 'Milano'); ometti lo slot per tutti i reparti",
                        Question = AttendanceTrackingMessages.Get("ChatEmployeeListDepartmentQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatEmployeeListDepartmentLabel")
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var testo = (slots.GetValueOrDefault("search") ?? string.Empty).Trim();
            var reparto = (slots.GetValueOrDefault("department") ?? string.Empty).Trim();

            // dipendenti visibili all'utente (il servizio respinge chi non e' HR o CompanyAdmin)
            var visibili = (await _dip_AnomalieService.DipendentiVisibili(new List<string>(), oggi)).ToHashSet();
            var dipendenti = (await Dipendenti(_dip_AnagraficaService)).Where(d => visibili.Contains(d.Id)).ToList();
            if (dipendenti.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatNoEmployeesAvailable"));

            // reparto e sede di oggi per utente
            var repartiUtente = await RepartiDiOggi(oggi);

            var trovati = dipendenti
                .Where(d => testo.Length == 0 || Contiene(d.Cognome, testo) || Contiene(d.Nome, testo) || Contiene(d.Matricola, testo) ||
                            Contiene(Nome(d), testo) || Contiene($"{d.Nome} {d.Cognome}", testo))
                .Where(d => reparto.Length == 0 || (repartiUtente.GetValueOrDefault(d.IdAspNetUsers) ?? new())
                                .Any(r => Contiene(r.Reparto, reparto) || Contiene(r.Sede, reparto)))
                .OrderBy(Nome)
                .ToList();

            var filtro = string.Join(", ", new[] { testo, reparto }.Where(s => s.Length > 0));
            if (trovati.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatEmployeeListNone", filtro), new DipendentiDati { Filtro = filtro });

            var elenco = trovati.Take(MaxDipendenti).ToList();
            var orari = await OrariDiOggi(elenco, oggi);

            var righe = elenco.Select(d =>
            {
                var o = orari.GetValueOrDefault(d.IdAspNetUsers);
                return new DipendenteRiga
                {
                    Dipendente = Nome(d),
                    Cognome = d.Cognome,
                    Nome = d.Nome,
                    Matricola = d.Matricola,
                    Reparti = repartiUtente.GetValueOrDefault(d.IdAspNetUsers) ?? new(),
                    RapportoAttivo = o != null,
                    DataAssunzione = o?.DataAssunzione,
                    DataLicenziamento = o?.DataLicenziamento,
                    TipoContratto = o?.TipoContratto,
                    OreSettimanaliContratto = o?.OreSettimanali,
                    Ccnl = o?.Ccnl,
                    ProfiloOrario = o?.ProfiloOrario,
                    OrarioOggi = o?.OrarioOggi
                };
            }).ToList();

            var dati = new DipendentiDati
            {
                Filtro = filtro,
                Data = oggi.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Totale = trovati.Count,
                Dipendenti = righe
            };

            var sb = new StringBuilder();
            sb.AppendLine(filtro.Length > 0
                ? AttendanceTrackingMessages.Get("ChatEmployeeListTitleFiltered", trovati.Count, filtro)
                : AttendanceTrackingMessages.Get("ChatEmployeeListTitle", trovati.Count));
            foreach (var r in righe.Take(MaxRighe))
            {
                var dettagli = new List<string>();
                if (r.Reparti.Count > 0) dettagli.Add(string.Join(", ", r.Reparti.Select(x => string.IsNullOrEmpty(x.Sede) ? x.Reparto : $"{x.Reparto} ({x.Sede})")));
                if (!string.IsNullOrEmpty(r.ProfiloOrario)) dettagli.Add(r.ProfiloOrario);
                if (!r.RapportoAttivo) dettagli.Add(AttendanceTrackingMessages.Get("ChatEmployeeListNoContract"));
                sb.AppendLine($"  • {r.Dipendente}" + (dettagli.Count > 0 ? ": " + string.Join(" · ", dettagli) : string.Empty));
            }
            if (trovati.Count > MaxRighe)
                sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", trovati.Count - MaxRighe));

            return CommandResult.Ok(sb.ToString().TrimEnd(), dati);
        }

        // utente -> reparti (con sede) di cui fa parte oggi
        private async Task<Dictionary<string, List<DipendenteReparto>>> RepartiDiOggi(DateTime oggi)
        {
            var resReparti = await _az_SediRepartoService.GetAll(new GenericRequest<Az_SediReparto_GetAll_InModel> { Data = new() }, true);
            var reparti = resReparti.Success && resReparti.Data != null ? resReparti.Data.Az_SediReparto : new List<Az_SediRepartoModel>();
            if (reparti.Count == 0) return new();

            var resSedi = await _az_SediService.GetAll(new GenericRequest<Az_Sedi_GetAll_InModel> { Data = new() }, true);
            var sedi = (resSedi.Success && resSedi.Data != null ? resSedi.Data.Az_Sedi : new List<Az_SediModel>())
                .ToDictionary(s => s.Id, s => s.Descrizione ?? string.Empty);
            var perId = reparti.ToDictionary(r => r.Id);

            var resUtenti = await _az_SediRepartoUserService.GetAllPeriod(new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>
            {
                Data = new() { IdAz_SediReparto = reparti.Select(r => r.Id).ToList() }
            }, true);
            var utenti = resUtenti.Success && resUtenti.Data != null ? resUtenti.Data.Az_RepartoUser : new List<Az_SediRepartoUserModel>();

            return utenti
                .Where(u => u.UserInDepartment && (u.DataDal == null || u.DataDal.Value.Date <= oggi) && (u.DataAl == null || u.DataAl.Value.Date >= oggi)
                            && perId.ContainsKey(u.IdAz_SediReparto))
                .GroupBy(u => u.IdAspNetUsers)
                .ToDictionary(g => g.Key, g => g.Select(u => u.IdAz_SediReparto).Distinct().Select(id => new DipendenteReparto
                {
                    Reparto = perId[id].Descrizione ?? string.Empty,
                    Sede = sedi.GetValueOrDefault(perId[id].IdAz_Sedi) ?? string.Empty
                }).ToList());
        }

        // utente -> rapporto di lavoro attivo oggi con profilo orario e orario del giorno (stessa regola del cartellino)
        private async Task<Dictionary<string, OrarioDipendente>> OrariDiOggi(List<Dip_AnagraficaModel> dipendenti, DateTime oggi)
        {
            var res = await _timeSheet_EngineService.Get_OrariSchema_4User(new GenericRequest<OrariSchema_4User_InModel>
            {
                Data = new() { UsersId = dipendenti.Select(d => d.IdAspNetUsers).ToList(), Dal = oggi, Al = oggi }
            }, true);
            if (!res.Success || res.Data == null) return new();

            var schema = res.Data;
            var anagrafiche = dipendenti.ToDictionary(d => d.Id);
            var retVal = new Dictionary<string, OrarioDipendente>();
            foreach (var rapporto in schema.Dip_RapportoLavoro.OrderByDescending(r => r.DataAss))
            {
                if (!anagrafiche.TryGetValue(rapporto.IdDip_Anagrafica, out var dipendente) || retVal.ContainsKey(dipendente.IdAspNetUsers))
                    continue;

                var slot = schema.DaySlots.FirstOrDefault(s => s.IdDip_RapportoLavoro == rapporto.Id && s.Data.Date == oggi);
                var profilo = slot != null ? schema.Par_ProfiloOrario.FirstOrDefault(p => p.Id == slot.IdPar_ProfiloOrario) : null;
                var idOrario = slot?.Orari.OrderBy(o => o.ZOrder).Select(o => (int?)o.IdPar_Orario).FirstOrDefault();
                var orario = idOrario != null ? schema.ParOrario.FirstOrDefault(o => o.Id == idOrario) : null;

                retVal[dipendente.IdAspNetUsers] = new OrarioDipendente
                {
                    DataAssunzione = rapporto.DataAss?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    DataLicenziamento = rapporto.DataLic?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TipoContratto = rapporto.TipoContratto.ToString(),
                    OreSettimanali = rapporto.OreSettimanaliContratto,
                    Ccnl = rapporto.Ccnl,
                    ProfiloOrario = profilo != null ? (string.IsNullOrEmpty(profilo.Descrizione) ? profilo.Codice : profilo.Descrizione) : null,
                    OrarioOggi = orario != null ? (string.IsNullOrEmpty(orario.Descrizione) ? orario.Codice : orario.Descrizione) : null
                };
            }
            return retVal;
        }

        private static bool Contiene(string? campo, string testo) =>
            !string.IsNullOrEmpty(campo) && campo.Contains(testo, StringComparison.OrdinalIgnoreCase);

        private class OrarioDipendente
        {
            public string? DataAssunzione { get; set; }
            public string? DataLicenziamento { get; set; }
            public string? TipoContratto { get; set; }
            public decimal? OreSettimanali { get; set; }
            public string? Ccnl { get; set; }
            public string? ProfiloOrario { get; set; }
            public string? OrarioOggi { get; set; }
        }
    }

    // ---------------------------------------------------------------------------
    // Dati strutturati dell'elenco dipendenti (accesso AI): date yyyy-MM-dd
    // ---------------------------------------------------------------------------

    public class DipendentiDati
    {
        public string Filtro { get; set; } = string.Empty;
        // giorno a cui si riferiscono reparti e rapporto di lavoro
        public string Data { get; set; } = string.Empty;
        // dipendenti trovati (l'elenco si ferma a EmployeeListHandler.MaxDipendenti)
        public int Totale { get; set; }
        public List<DipendenteRiga> Dipendenti { get; set; } = new();
    }

    public class DipendenteRiga
    {
        public string Dipendente { get; set; } = string.Empty;
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Matricola { get; set; }
        public List<DipendenteReparto> Reparti { get; set; } = new();
        // false = nessun rapporto di lavoro attivo oggi (campi seguenti vuoti)
        public bool RapportoAttivo { get; set; }
        public string? DataAssunzione { get; set; }
        public string? DataLicenziamento { get; set; }
        // FullTime / PartTime
        public string? TipoContratto { get; set; }
        public decimal? OreSettimanaliContratto { get; set; }
        public string? Ccnl { get; set; }
        public string? ProfiloOrario { get; set; }
        public string? OrarioOggi { get; set; }
    }

    public class DipendenteReparto
    {
        public string Reparto { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
    }
}
