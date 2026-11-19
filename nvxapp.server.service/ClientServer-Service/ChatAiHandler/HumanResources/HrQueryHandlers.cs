using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources.ChatHrHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources
{
    // Interrogazioni di Risorse Umane dalla chat (sola lettura, senza conferma). ANALISI_HUMANRESOURCES_VOCALI.md §3.1

    // R1 — scadenze di Risorse Umane (corsi da rinnovare, voci delle checklist) scadute e entro N giorni
    public class HrExpiriesHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 30;

        private readonly IDeadlineService _deadlineService;
        private readonly IHr_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public HrExpiriesHandler(IDeadlineService deadlineService, IHr_ConfigService configService)
        {
            _deadlineService = deadlineService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrExpiries",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatExpiriesDisplayName"),
                Description = HumanResourcesMessages.Get("ChatExpiriesDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "scadenze hr", "scadenze dipendenti", "scadenze dei dipendenti", "scadenze del personale", "scadenze risorse umane",
                    "corsi in scadenza", "corsi scaduti", "formazione in scadenza", "formazione scaduta", "attestati in scadenza",
                    "hr deadlines", "hr expiries", "expiring courses", "expiring training"
                },
                Examples = new()
                {
                    "scadenze hr dei prossimi 60 giorni", "quali corsi sono in scadenza?", "ci sono corsi scaduti?",
                    "scadenze dei dipendenti di Rossi", "which training courses are expiring?"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hrcourselist", "/hrcourseedition", "/hrchecklistlist", "/hremployeelist" },
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "days",
                        Type = "int",
                        Required = false,
                        PromptDescription = "entro quanti giorni (es. 'nei prossimi 60 giorni' -> '60', 'entro un mese' -> 'un mese'); copia l'espressione; ometti se non detto",
                        Question = HumanResourcesMessages.Get("ChatDaysQuestion"),
                        Label = HumanResourcesMessages.Get("ChatDaysLabel"),
                        Normalizer = NormalizzaGiorni,
                        Validator = v => int.TryParse(v, out var g) && g > 0 && g <= 3650
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, HumanResourcesMessages.Get("ChatDaysQuestion"), "days")
                    },
                    SlotDipendente(false))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            int giorni;
            if (!int.TryParse(slots.GetValueOrDefault("days"), out giorni) || giorni <= 0)
            {
                var cfg = await _configService.Hr_CfgGet(new GenericRequest<Hr_EmptyInModel>(new Hr_EmptyInModel()), false);
                giorni = cfg.Success && cfg.Data != null && cfg.Data.Cfg.GiorniPreavviso > 0 ? cfg.Data.Cfg.GiorniPreavviso : GiorniPredefiniti;
            }

            // scadenzario comune: stessa visibilità della pagina Scadenze; si tengono solo quelle di Risorse Umane
            var res = await _deadlineService.DeadlineList(new GenericRequest<DeadlineListInModel>(
                new DeadlineListInModel { Stato = 0, EntroGiorni = giorni }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var scadenze = res.Data.Deadline.Where(d => d.Application == (int)ApplicationType.HumanResources && d.Stato == 0).ToList();
            string? persona = null;
            if (slots.TryGetValue("employeeName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                // la descrizione delle scadenze del modulo termina con il nome del dipendente ("Corso X - Rossi Mario")
                scadenze = Trova(scadenze, nome, d => d.Descrizione);
                persona = nome.Trim();
            }

            var filtro = Filtro(new[] { persona });
            if (scadenze.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatExpiriesNone", giorni, filtro));

            var oggi = DateTime.Today;
            var scadute = scadenze.Where(d => d.DataScadenza.Date < oggi).OrderBy(d => d.DataScadenza).ToList();
            var prossime = scadenze.Where(d => d.DataScadenza.Date >= oggi).OrderBy(d => d.DataScadenza).ToList();

            var sb = new StringBuilder();
            if (scadute.Count > 0)
                sb.AppendLine(Elenco(HumanResourcesMessages.Get("ChatExpiriesOverdue", scadute.Count, filtro), scadute.Select(Riga), 10));
            if (prossime.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine(Elenco(HumanResourcesMessages.Get("ChatExpiriesTitle", prossime.Count, giorni, filtro), prossime.Select(Riga)));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }

        private static string Riga(DeadlineModel d) => $"{Data(d.DataScadenza)} — {d.Descrizione}";
    }

    // R2 — corsi da fare secondo i piani di formazione dell'anno (righe non ancora svolte), per dipendente o corso
    public class HrTrainingToDoHandler : BaseCommandHandler
    {
        private readonly IHr_FormazioneService _formazioneService;
        private readonly IntentDefinition _intentDefinition;

        public HrTrainingToDoHandler(IHr_FormazioneService formazioneService)
        {
            _formazioneService = formazioneService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrTrainingToDo",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatTrainingToDoDisplayName"),
                Description = HumanResourcesMessages.Get("ChatTrainingToDoDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "corsi da fare", "formazione da fare", "piano di formazione", "piano formazione", "deve fare il corso", "devono fare",
                    "corsi mancanti", "formazione", "corsi", "corso", "training to do", "training plan", "courses to do", "training"
                },
                Examples = new()
                {
                    "che corsi deve ancora fare Rossi?", "chi deve fare il corso sicurezza?", "com'è il piano di formazione di quest'anno?",
                    "formazione da fare entro marzo", "which courses does Rossi still have to take?"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hrplanlist", "/hrplanedit", "/hrcourselist" },
                Slots = BuildSlots(SlotDipendente(false), SlotCorso(false), SlotAnno())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var anno = int.TryParse(slots.GetValueOrDefault("year"), out var a) ? a : DateTime.Today.Year;
            var lista = await _formazioneService.Hr_PianoList(new GenericRequest<Hr_PianoListInModel>(new Hr_PianoListInModel { Anno = anno }), false);
            if (!lista.Success || lista.Data == null) return Fallito(lista);
            if (lista.Data.Piani.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatTrainingNoPlan", anno));

            var righe = new List<Hr_PianoRigaModel>();
            foreach (var p in lista.Data.Piani)
            {
                var piano = await _formazioneService.Hr_PianoGet(new GenericRequest<Hr_IdInModel>(new Hr_IdInModel { Id = p.Id }), false);
                if (!piano.Success || piano.Data == null) return Fallito(piano);
                righe.AddRange(piano.Data.Piano.Righe);
            }
            var totali = righe.Count;
            var filtri = new List<string?> { anno.ToString(CultureInfo.InvariantCulture) };

            if (slots.TryGetValue("employeeName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var (dip, errore) = UnNome(righe.Select(r => r.Dipendente ?? string.Empty), nome, "ChatTrainingNoneEmployee");
                if (dip == null) return CommandResult.Ok(errore!);
                righe = righe.Where(r => string.Equals(r.Dipendente, dip, StringComparison.OrdinalIgnoreCase)).ToList();
                filtri.Add(dip);
            }
            if (slots.TryGetValue("course", out var corso) && !string.IsNullOrWhiteSpace(corso))
            {
                var corsi = Trova(righe.Select(r => r.Corso ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase), corso, x => x);
                if (corsi.Count == 0) return CommandResult.Ok(HumanResourcesMessages.Get("ChatTrainingNoneCourse", corso, anno));
                righe = righe.Where(r => corsi.Contains(r.Corso ?? string.Empty, StringComparer.OrdinalIgnoreCase)).ToList();
                filtri.Add(corsi.Count == 1 ? corsi[0] : corso.Trim());
            }

            var filtro = Filtro(filtri);
            var daFare = righe.Where(r => !r.Svolto).OrderBy(r => r.EntroIl ?? DateTime.MaxValue).ThenBy(r => r.Dipendente).ToList();
            var svolte = righe.Count - daFare.Count;
            if (daFare.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatTrainingAllDone", righe.Count, filtro));

            var oggi = DateTime.Today;
            var titolo = HumanResourcesMessages.Get("ChatTrainingTitle", daFare.Count, filtro, svolte, righe.Count);
            return CommandResult.Ok(Elenco(titolo, daFare.Select(r =>
            {
                var entro = r.EntroIl.HasValue
                    ? " — " + HumanResourcesMessages.Get(r.EntroIl.Value.Date < oggi ? "ChatTrainingLate" : "ChatTrainingBy", Data(r.EntroIl.Value))
                    : string.Empty;
                return $"{r.Dipendente} — {r.Corso}{entro}";
            })));
        }
    }

    // R3 — checklist di assunzione e cessazione aperte; per un dipendente anche le voci da completare
    public class HrOnboardingHandler : BaseCommandHandler
    {
        private readonly IHr_OnboardingService _onboardingService;
        private readonly IntentDefinition _intentDefinition;

        public HrOnboardingHandler(IHr_OnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrOnboarding",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatOnboardingDisplayName"),
                Description = HumanResourcesMessages.Get("ChatOnboardingDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "onboarding", "offboarding", "checklist hr", "checklist aperte", "checklist di", "assunzioni in corso", "cessazioni in corso", "assunzione di", "cessazione di", "nuovi assunti", "chi entra", "chi esce",
                    "new hires", "leavers"
                },
                Examples = new()
                {
                    "come vanno gli onboarding?", "checklist aperte", "a che punto è la checklist di Rossi?", "cosa manca per l'assunzione di Bianchi?",
                    "show the open onboarding checklists"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hrchecklistlist", "/hrchecklistedit", "/hremployeelist" },
                Slots = BuildSlots(SlotDipendente(false))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _onboardingService.Hr_ChecklistList(new GenericRequest<Hr_ChecklistListInModel>(new Hr_ChecklistListInModel { SoloAperte = true }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var aperte = res.Data.Checklist;
            var oggi = DateTime.Today;

            if (slots.TryGetValue("employeeName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var (dip, errore) = UnNome(aperte.Select(c => c.Dipendente), nome, "ChatOnboardingNoneEmployee");
                if (dip == null) return CommandResult.Ok(errore!);

                // dettaglio di ogni checklist aperta del dipendente: voci da completare
                var sb = new StringBuilder();
                foreach (var c in aperte.Where(c => string.Equals(c.Dipendente, dip, StringComparison.OrdinalIgnoreCase)))
                {
                    var det = await _onboardingService.Hr_ChecklistGet(new GenericRequest<Hr_IdInModel>(new Hr_IdInModel { Id = c.Id }), false);
                    if (!det.Success || det.Data == null) return Fallito(det);
                    var voci = det.Data.Voci.Where(v => !v.Completata).OrderBy(v => v.DataPrevista).ToList();
                    if (sb.Length > 0) sb.AppendLine();
                    var titolo = HumanResourcesMessages.Get("ChatOnboardingDetail", TipoChecklist(c.Tipo), dip, Data(c.DataRiferimento),
                        det.Data.Voci.Count(v => v.Completata), det.Data.Voci.Count);
                    sb.AppendLine(voci.Count == 0
                        ? titolo
                        : Elenco(titolo, voci.Select(v =>
                        {
                            var riga = $"{v.Descrizione} — {Data(v.DataPrevista)}";
                            if (v.DataPrevista.Date < oggi) riga += " " + HumanResourcesMessages.Get("ChatLateMark");
                            if (!string.IsNullOrWhiteSpace(v.Responsabile)) riga += " — " + v.Responsabile;
                            return riga;
                        })));
                }
                return CommandResult.Ok(sb.ToString().TrimEnd());
            }

            if (aperte.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatOnboardingNone"));

            return CommandResult.Ok(Elenco(HumanResourcesMessages.Get("ChatOnboardingTitle", aperte.Count),
                aperte.OrderBy(c => c.ProssimaScadenza ?? DateTime.MaxValue).Select(c =>
                {
                    var riga = $"{c.Dipendente} — {TipoChecklist(c.Tipo)} {Data(c.DataRiferimento)} — " +
                               HumanResourcesMessages.Get("ChatOnboardingProgress", c.NumeroCompletate, c.NumeroVoci);
                    if (c.ProssimaScadenza.HasValue)
                    {
                        riga += " — " + HumanResourcesMessages.Get("ChatOnboardingNext", Data(c.ProssimaScadenza.Value));
                        if (c.ProssimaScadenza.Value.Date < oggi) riga += " " + HumanResourcesMessages.Get("ChatLateMark");
                    }
                    return riga;
                })));
        }
    }

    // R4 — scheda HR del dipendente in breve: matricola, email, assunzione e cessazione, account, checklist aperte
    public class HrEmployeeInfoHandler : BaseCommandHandler
    {
        private readonly IHr_DipendenteService _dipendenteService;
        private readonly IHr_OnboardingService _onboardingService;
        private readonly IntentDefinition _intentDefinition;

        public HrEmployeeInfoHandler(IHr_DipendenteService dipendenteService, IHr_OnboardingService onboardingService)
        {
            _dipendenteService = dipendenteService;
            _onboardingService = onboardingService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrEmployeeInfo",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatEmployeeInfoDisplayName"),
                Description = HumanResourcesMessages.Get("ChatEmployeeInfoDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "scheda hr", "anagrafica hr", "dati hr", "matricola", "data di assunzione", "assunto", "quando è stato assunto", "cessazione",
                    "hr record", "hire date", "employee number"
                },
                Examples = new()
                {
                    "scheda hr di Rossi", "che matricola ha Bianchi?", "quando è stato assunto Verdi?", "Rossi ha l'account?",
                    "show the hr record of Rossi"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hremployeelist", "/hremployeeedit" },
                Slots = BuildSlots(SlotDipendente(true))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (d, errore) = await RisolviDipendente(_dipendenteService, slots.GetValueOrDefault("employeeName"));
            if (d == null) return CommandResult.Fail(errore!);

            // niente codice fiscale: non serve a voce e resta nella pagina
            var righe = new List<string>
            {
                HumanResourcesMessages.Get(d.Attivo ? "ChatEmployeeActive" : "ChatEmployeeInactive")
            };
            if (!string.IsNullOrWhiteSpace(d.Matricola)) righe.Add(HumanResourcesMessages.Get("ChatEmployeeNumber", d.Matricola));
            if (!string.IsNullOrWhiteSpace(d.Email)) righe.Add(HumanResourcesMessages.Get("ChatEmployeeEmail", d.Email));
            if (d.DataAssunzione.HasValue) righe.Add(HumanResourcesMessages.Get("ChatEmployeeHired", Data(d.DataAssunzione.Value)));
            if (d.DataCessazione.HasValue) righe.Add(HumanResourcesMessages.Get("ChatEmployeeEnd", Data(d.DataCessazione.Value)));
            righe.Add(!d.Account.Presente
                ? HumanResourcesMessages.Get("ChatEmployeeNoAccount")
                : HumanResourcesMessages.Get(d.Account.Disabilitato ? "ChatEmployeeAccountDisabled" : "ChatEmployeeAccount", d.Account.UserName));

            var cl = await _onboardingService.Hr_ChecklistList(new GenericRequest<Hr_ChecklistListInModel>(new Hr_ChecklistListInModel { SoloAperte = true }), false);
            if (cl.Success && cl.Data != null)
                foreach (var c in cl.Data.Checklist.Where(c => c.IdDip_Anagrafica == d.Id))
                    righe.Add(HumanResourcesMessages.Get("ChatEmployeeChecklist", TipoChecklist(c.Tipo), c.NumeroCompletate, c.NumeroVoci));

            return CommandResult.Ok(Elenco(NomeDipendente(d.Cognome, d.Nome), righe));
        }
    }

    // R5 — i miei cedolini e CU (area del dipendente): periodi pubblicati e conferme mancanti, mai importi né contenuti
    public class HrMyPayslipsHandler : BaseCommandHandler
    {
        private readonly IHr_CedolinoService _cedolinoService;
        private readonly IntentDefinition _intentDefinition;

        public HrMyPayslipsHandler(IHr_CedolinoService cedolinoService)
        {
            _cedolinoService = cedolinoService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrMyPayslips",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatMyPayslipsDisplayName"),
                Description = HumanResourcesMessages.Get("ChatMyPayslipsDescription"),
                Usable4Role = RuoliDipendente,
                Keywords = new()
                {
                    "miei cedolini", "mie buste paga", "mia busta paga", "mio cedolino", "cedolini da confermare", "è arrivato il cedolino",
                    "cedolini ho", "nuovi cedolini", "mia cu", "mie cu", "my payslips", "my payslip", "my pay slips", "new payslips"
                },
                Examples = new()
                {
                    "quali cedolini ho?", "è arrivato il mio cedolino di ottobre?", "ho cedolini da confermare?", "i miei cedolini del 2025",
                    "do I have new payslips?"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hrmypayslips" },
                Slots = BuildSlots(SlotAnno())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _cedolinoService.Hr_MieiDocumenti(new GenericRequest<Hr_EmptyInModel>(new Hr_EmptyInModel()), false);
            if (!res.Success || res.Data == null) return Fallito(res);
            if (!res.Data.Dipendente)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatMyPayslipsNotEmployee"));

            var documenti = res.Data.Documenti;
            int? anno = int.TryParse(slots.GetValueOrDefault("year"), out var a) ? a : null;
            if (anno != null) documenti = documenti.Where(d => d.Anno == anno).ToList();
            var filtro = Filtro(new[] { anno?.ToString(CultureInfo.InvariantCulture) });
            if (documenti.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatMyPayslipsNone", filtro));

            var ordinati = documenti.OrderByDescending(d => d.Anno).ThenByDescending(d => d.Mese ?? 13).ToList();
            var cedolini = ordinati.Where(d => d.Tipo == 1).ToList();
            var cu = ordinati.Where(d => d.Tipo == 2).ToList();
            var daConfermare = ordinati.Where(d => d.IdPresaVisione != null && d.DataConferma == null).ToList();

            var righe = new List<string>();
            if (cedolini.Count > 0)
                righe.Add(HumanResourcesMessages.Get("ChatMyPayslipsLast", cedolini.Count, Periodo(cedolini[0].Anno, cedolini[0].Mese)));
            if (cu.Count > 0)
                righe.Add(HumanResourcesMessages.Get("ChatMyPayslipsCu", cu.Count, cu[0].Anno));
            if (anno != null && cedolini.Count > 0)
                righe.Add(HumanResourcesMessages.Get("ChatMyPayslipsMonths",
                    string.Join(", ", cedolini.OrderBy(d => d.Mese).Select(d => Periodo(d.Anno, d.Mese)))));
            righe.Add(daConfermare.Count == 0
                ? HumanResourcesMessages.Get("ChatMyPayslipsAllConfirmed")
                : HumanResourcesMessages.Get("ChatMyPayslipsToConfirm", daConfermare.Count,
                    string.Join(", ", daConfermare.Take(6).Select(d => d.Tipo == 2 ? $"CU {d.Anno}" : Periodo(d.Anno, d.Mese)))));

            return CommandResult.Ok(Elenco(HumanResourcesMessages.Get("ChatMyPayslipsTitle", documenti.Count, filtro), righe));
        }
    }

    // R6 — organigramma (ANALISI_INFRASTRUCTURE_MCP.md, MCP-34): sedi e reparti ad albero dell'azienda, anche di una sola
    // sede o reparto con i suoi sottoreparti. Fonte: tabelle comuni Az_Sedi / Az_SediReparto lette da CommonOrganizzazioneService
    // (limitato all'azienda della richiesta). Risorse Umane non registra responsabili né l'assegnazione delle persone ai reparti
    // (l'assegnazione è delle presenze): l'organigramma è la struttura, e i dati lo dicono all'AI (Nota).
    public class HrOrgChartHandler : BaseCommandHandler
    {
        // oltre questa profondità l'albero si ferma (protezione da reparti padre circolari)
        private const int ProfonditaMassima = 20;

        private readonly ICommonOrganizzazioneService _organizzazioneService;
        private readonly IntentDefinition _intentDefinition;

        public HrOrgChartHandler(ICommonOrganizzazioneService organizzazioneService)
        {
            _organizzazioneService = organizzazioneService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrOrgChart",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatOrgChartDisplayName"),
                Description = HumanResourcesMessages.Get("ChatOrgChartDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "organigramma", "struttura aziendale", "struttura dell'azienda", "reparti", "sottoreparti", "sedi dell'azienda", "unità organizzative",
                    "org chart", "organization chart", "organisation chart", "departments", "company structure"
                },
                Examples = new()
                {
                    "mostrami l'organigramma", "che reparti ha la sede di Milano?", "com'è organizzato il reparto produzione?",
                    "show the org chart", "which departments are there?"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hremployeelist" },
                Slots = BuildSlots(SlotTesto("unit", false,
                    "nome della sede o del reparto se detto (es. 'della sede di Milano' -> 'Milano', 'il reparto produzione' -> 'produzione'); ometti per tutta l'azienda",
                    HumanResourcesMessages.Get("ChatUnitQuestion"), HumanResourcesMessages.Get("ChatUnitLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _organizzazioneService.SediGetAll(new GenericRequest<CommonOrganizzazione_SediGetAll_InModel>(new CommonOrganizzazione_SediGetAll_InModel()), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            // albero appiattito in ordine di visita: sede, poi i suoi reparti con i sottoreparti
            var unita = new List<HrUnitaOrganizzativa>();
            foreach (var sede in res.Data.Az_Sedi)
            {
                var nodoSede = new HrUnitaOrganizzativa
                {
                    Id = sede.Id, Tipo = HrUnitaOrganizzativa.TipoSede, Nome = sede.Descrizione ?? string.Empty, Sede = sede.Descrizione ?? string.Empty,
                    Percorso = sede.Descrizione ?? string.Empty, Livello = 0, NumeroReparti = sede.Az_SediReparto.Count
                };
                unita.Add(nodoSede);
                foreach (var r in sede.Az_SediReparto)
                    AggiungiReparto(unita, r, nodoSede, 1);
            }
            if (unita.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatOrgChartNone"));

            string? filtro = null;
            if (slots.TryGetValue("unit", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var trovate = Trova(unita, nome, u => u.Nome);
                if (trovate.Count == 0) trovate = Trova(unita, nome, u => u.Percorso);
                if (trovate.Count == 0)
                    return CommandResult.Ok(HumanResourcesMessages.Get("ChatOrgChartNotFound", nome.Trim()));
                if (trovate.Count > 1)
                    return CommandResult.Ok(HumanResourcesMessages.Get("ChatAmbiguous", trovate.Count, nome.Trim(), string.Join(", ", trovate.Take(8).Select(u => u.Percorso))));

                // l'unità scelta e tutto ciò che le sta sotto: nell'ordine di visita sono le unità che seguono
                // con livello maggiore, fino alla prima di livello uguale o minore
                var scelta = trovate[0];
                var inizio = unita.IndexOf(scelta);
                unita = unita.Skip(inizio).TakeWhile((u, i) => i == 0 || u.Livello > scelta.Livello).ToList();
                filtro = scelta.Percorso;
            }

            var sedi = unita.Count(u => u.Tipo == HrUnitaOrganizzativa.TipoSede);
            var reparti = unita.Count - sedi;
            var nota = HumanResourcesMessages.Get("ChatOrgChartNote");
            var titolo = HumanResourcesMessages.Get("ChatOrgChartTitle", Filtro(new[] { filtro }), sedi, reparti);
            var testo = Elenco(titolo, unita.Select(u => u.Percorso), 20) + Environment.NewLine + nota;

            return CommandResult.Ok(testo, new HrOrganigrammaDati { Sedi = sedi, Reparti = reparti, Unita = unita, Nota = nota });
        }

        private const string Separatore = " / ";

        private static void AggiungiReparto(List<HrUnitaOrganizzativa> unita, Az_SediRepartoCommonModel r, HrUnitaOrganizzativa padre, int livello)
        {
            if (livello > ProfonditaMassima) return;
            var nodo = new HrUnitaOrganizzativa
            {
                Id = r.Id, Tipo = HrUnitaOrganizzativa.TipoReparto, Nome = r.Descrizione ?? string.Empty, Sede = padre.Sede,
                IdPadre = padre.Id, TipoPadre = padre.Tipo, Padre = padre.Nome,
                Percorso = padre.Percorso + Separatore + r.Descrizione, Livello = livello, NumeroReparti = r.Az_SediReparto_Sub.Count
            };
            unita.Add(nodo);
            foreach (var sub in r.Az_SediReparto_Sub)
                AggiungiReparto(unita, sub, nodo, livello + 1);
        }
    }

    // R7 — contratti in scadenza (ANALISI_INFRASTRUCTURE_MCP.md, MCP-34): dipendenti con la fine del rapporto di lavoro
    // (data di cessazione della scheda HR) entro N giorni, più quelli ancora attivi con la data già passata.
    // Risorse Umane non gestisce tipi di contratto (tempo determinato, prova, apprendistato: sono nei rapporti di lavoro
    // delle presenze): la data è quella che le presenze tengono allineata con l'ultimo rapporto (Dip_Anagrafica).
    // Stessa visibilità della pagina Dipendenti di Risorse Umane (Hr_DipendenteList: amministratore dell'azienda).
    public class HrContractExpiriesHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 60;
        private const int GiorniMassimi = 365;

        private readonly IHr_DipendenteService _dipendenteService;
        private readonly IntentDefinition _intentDefinition;

        public HrContractExpiriesHandler(IHr_DipendenteService dipendenteService)
        {
            _dipendenteService = dipendenteService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrContractExpiries",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatContractExpiriesDisplayName"),
                Description = HumanResourcesMessages.Get("ChatContractExpiriesDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "contratti in scadenza", "contratti scaduti", "contratti che scadono", "scadenza contratto", "scadenza dei contratti", "fine contratto",
                    "cessazioni previste", "chi cessa", "rapporti in scadenza",
                    "expiring contracts", "contract expiry", "contracts ending", "end of contract", "upcoming terminations"
                },
                Examples = new()
                {
                    "quali contratti scadono nei prossimi 60 giorni?", "contratti in scadenza entro tre mesi", "chi finisce il contratto questo mese?",
                    "which contracts expire in the next 90 days?", "show the expiring contracts"
                },
                RequiresConfirmation = false,
                Pages = new() { "/hremployeelist", "/hremployeeedit" },
                Slots = BuildSlots(new SlotDefinition
                {
                    Name = "days",
                    Type = "int",
                    Required = false,
                    PromptDescription = $"entro quanti giorni (es. 'nei prossimi 90 giorni' -> '90', 'entro tre mesi' -> 'tre mesi'); copia l'espressione; ometti se non detto ({GiorniPredefiniti} giorni, massimo {GiorniMassimi})",
                    Question = HumanResourcesMessages.Get("ChatDaysQuestion"),
                    Label = HumanResourcesMessages.Get("ChatDaysLabel"),
                    Normalizer = NormalizzaGiorni,
                    Validator = v => int.TryParse(v, out var g) && g > 0 && g <= GiorniMassimi
                        ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, HumanResourcesMessages.Get("ChatContractDaysInvalid", GiorniMassimi), "days")
                })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorni = int.TryParse(slots.GetValueOrDefault("days"), out var g) && g > 0 ? Math.Min(g, GiorniMassimi) : GiorniPredefiniti;

            var res = await _dipendenteService.Hr_DipendenteList(new GenericRequest<Hr_DipendenteListInModel>(new Hr_DipendenteListInModel { SoloAttivi = false }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var oggi = DateTime.Today;
            var limite = oggi.AddDays(giorni);
            // in scadenza: fine entro il periodo; già passata: solo chi risulta ancora attivo (scheda da sistemare)
            var righe = res.Data.Dipendenti
                .Where(d => d.DataCessazione.HasValue
                            && ((d.DataCessazione.Value.Date >= oggi && d.DataCessazione.Value.Date <= limite)
                                || (d.DataCessazione.Value.Date < oggi && d.Attivo)))
                .OrderBy(d => d.DataCessazione)
                .ThenBy(d => d.Cognome).ThenBy(d => d.Nome)
                .Select(d => new HrContrattoInScadenza
                {
                    IdDipendente = d.Id,
                    Dipendente = NomeDipendente(d.Cognome, d.Nome),
                    Matricola = string.IsNullOrWhiteSpace(d.Matricola) ? null : d.Matricola,
                    Attivo = d.Attivo,
                    DataAssunzione = DataIso(d.DataAssunzione),
                    DataFine = DataIso(d.DataCessazione!.Value),
                    GiorniAllaFine = (int)(d.DataCessazione.Value.Date - oggi).TotalDays,
                    Scaduto = d.DataCessazione.Value.Date < oggi
                })
                .ToList();

            var dati = new HrContrattiInScadenzaDati { Giorni = giorni, Dal = DataIso(oggi), Al = DataIso(limite), Contratti = righe };
            if (righe.Count == 0)
                return CommandResult.Ok(HumanResourcesMessages.Get("ChatContractExpiriesNone", giorni), dati);

            var scaduti = righe.Where(r => r.Scaduto).ToList();
            var prossimi = righe.Where(r => !r.Scaduto).ToList();
            var sb = new StringBuilder();
            if (scaduti.Count > 0)
                sb.AppendLine(Elenco(HumanResourcesMessages.Get("ChatContractExpiriesOverdue", scaduti.Count), scaduti.Select(Riga), 10));
            if (prossimi.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine(Elenco(HumanResourcesMessages.Get("ChatContractExpiriesTitle", prossimi.Count, giorni), prossimi.Select(Riga)));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd(), dati);
        }

        private static string Riga(HrContrattoInScadenza c)
        {
            var data = DateTime.ParseExact(c.DataFine, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return $"{Data(data)} — {c.Dipendente}" + (c.Matricola != null ? $" ({c.Matricola})" : string.Empty);
        }
    }

    // ---------------------------------------------------------------------------
    // Dati strutturati restituiti dagli strumenti di lettura (CommandResult.Data, fase 3 MCP): date yyyy-MM-dd
    // ---------------------------------------------------------------------------

    public class HrOrganigrammaDati
    {
        public int Sedi { get; set; }
        public int Reparti { get; set; }
        // in ordine di albero: sede, reparti, sottoreparti
        public List<HrUnitaOrganizzativa> Unita { get; set; } = new();
        public string Nota { get; set; } = string.Empty;
    }

    public class HrUnitaOrganizzativa
    {
        public const string TipoSede = "sede";
        public const string TipoReparto = "reparto";

        public int Id { get; set; }
        public string Tipo { get; set; } = TipoSede;          // "sede" | "reparto"
        public string Nome { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;      // sede a cui appartiene
        public int? IdPadre { get; set; }                     // sede o reparto padre (null per le sedi)
        public string? TipoPadre { get; set; }
        public string? Padre { get; set; }
        public string Percorso { get; set; } = string.Empty;  // "Sede / Reparto / Sottoreparto"
        public int Livello { get; set; }                      // 0 = sede
        public int NumeroReparti { get; set; }                // reparti diretti sotto questa unità
    }

    public class HrContrattiInScadenzaDati
    {
        public int Giorni { get; set; }
        public string Dal { get; set; } = string.Empty;
        public string Al { get; set; } = string.Empty;
        public List<HrContrattoInScadenza> Contratti { get; set; } = new();
    }

    public class HrContrattoInScadenza
    {
        public int IdDipendente { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public string? Matricola { get; set; }
        public bool Attivo { get; set; }
        public string? DataAssunzione { get; set; }
        public string DataFine { get; set; } = string.Empty;  // data di cessazione della scheda HR
        public int GiorniAllaFine { get; set; }               // negativo se già passata
        public bool Scaduto { get; set; }
    }
}
