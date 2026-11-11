using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm.ChatCrmHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm
{
    // Interrogazioni del CRM dalla chat (sola lettura, senza conferma). ANALISI_CRM_VOCALI.md §3.1

    // R1 — le mie attività da fare in un periodo (default oggi); se il periodo parte da oggi anche le scadute
    public class CrmAgendaHandler : BaseCommandHandler
    {
        private readonly ICrm_AttivitaService _attivitaService;
        private readonly IntentDefinition _intentDefinition;

        public CrmAgendaHandler(ICrm_AttivitaService attivitaService)
        {
            _attivitaService = attivitaService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmAgenda",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategoryAgenda"),
                CategoryOrder = CategoryOrderAgenda,
                DisplayName = CrmMessages.Get("ChatAgendaDisplayName"),
                Description = CrmMessages.Get("ChatAgendaDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "agenda", "appuntamenti", "attività crm", "chiamate da fare", "cosa ho da fare", "to do", "appointments", "my agenda" },
                Examples = new() { "cosa ho in agenda oggi?", "che appuntamenti ho domani?", "attività della prossima settimana", "what's on my agenda today?" },
                RequiresConfirmation = false,
                Pages = new() { "/crmactivitylist" },
                Slots = BuildSlots(
                    SlotData("from", false, "oggi", CrmMessages.Get("ChatFromQuestion"), CrmMessages.Get("ChatFromLabel"),
                        "data di INIZIO del periodo; per una settimana copia l'espressione (es. 'la prossima settimana')"),
                    SlotData("to", false, string.Empty, CrmMessages.Get("ChatToQuestion"), CrmMessages.Get("ChatToLabel"),
                        "data di FINE del periodo; ometti lo slot se l'utente indica un solo giorno"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var dal = ParseData(slots.GetValueOrDefault("from"), oggi);
            var al = ParseData(slots.GetValueOrDefault("to"), dal);
            if (al < dal) al = dal;

            var res = await _attivitaService.Crm_AttivitaList(new GenericRequest<Crm_AttivitaListInModel>(
                new Crm_AttivitaListInModel { Dal = dal, Al = al, SoloMie = true, SoloDaFare = true }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            // il servizio aggiunge le scadute prima di "dal": si mostrano solo se il periodo comprende oggi
            var attivita = res.Data.Attivita.Where(a => a.Stato == Crm_StatoAttivita.DaFare).ToList();
            var scadute = dal <= oggi ? attivita.Where(a => a.Inizio.Date < oggi && a.Inizio.Date < dal).ToList() : new();
            var periodo = attivita.Where(a => a.Inizio.Date >= dal && a.Inizio.Date <= al).OrderBy(a => a.Inizio).ToList();

            var testoPeriodo = Periodo(dal, al);
            var sb = new StringBuilder();
            if (periodo.Count == 0)
                sb.AppendLine(CrmMessages.Get("ChatAgendaNone", testoPeriodo));
            else
                sb.AppendLine(Elenco(CrmMessages.Get("ChatAgendaTitle", periodo.Count, testoPeriodo),
                    periodo.Select(a => Riga(a, dal.Date != al.Date))));

            if (scadute.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Elenco(CrmMessages.Get("ChatAgendaOverdue", scadute.Count), scadute.OrderBy(a => a.Inizio).Select(a => Riga(a, true)), 5));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }

        // "10:00 Chiamata — rinnovo contratto (Rossi srl)"
        private static string Riga(Crm_AttivitaListModel a, bool conData)
        {
            var quando = conData ? $"{Data(a.Inizio)} {Ora(a.Inizio)}" : Ora(a.Inizio);
            var con = a.Cliente ?? a.Lead;
            return string.IsNullOrEmpty(con) ? $"{quando} {a.Tipo} — {a.Oggetto}" : $"{quando} {a.Tipo} — {a.Oggetto} ({con})";
        }
    }

    // R2 — opportunità aperte, con chiusura prevista in un periodo e/o di un cliente
    public class CrmOpportunitiesHandler : BaseCommandHandler
    {
        private readonly ICrm_OpportunitaService _opportunitaService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmOpportunitiesHandler(ICrm_OpportunitaService opportunitaService, ICrm_ConfigService configService)
        {
            _opportunitaService = opportunitaService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmOpportunities",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatOpportunitiesDisplayName"),
                Description = CrmMessages.Get("ChatOpportunitiesDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "opportunità", "opportunita", "trattative", "in chiusura", "deal", "opportunities", "closing" },
                Examples = new() { "opportunità in chiusura questo mese", "che trattative ho aperte con Rossi?", "opportunità che chiudono entro novembre", "deals closing this month" },
                RequiresConfirmation = false,
                Pages = new() { "/crmpipeline", "/crmopportunityedit" },
                Slots = BuildSlots(
                    SlotData("from", false, string.Empty, CrmMessages.Get("ChatFromQuestion"), CrmMessages.Get("ChatFromLabel"),
                        "INIZIO del periodo di chiusura prevista; per un mese copia l'espressione (es. 'questo mese', 'novembre'); ometti se l'utente non indica un periodo"),
                    SlotData("to", false, string.Empty, CrmMessages.Get("ChatToQuestion"), CrmMessages.Get("ChatToLabel"),
                        "FINE del periodo di chiusura prevista ('entro il 30 novembre')"),
                    SlotCliente(false))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _opportunitaService.Crm_OpportunitaList(new GenericRequest<Crm_OpportunitaListInModel>(
                new Crm_OpportunitaListInModel { AncheChiuse = false }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var aperte = res.Data.Opportunita.Where(o => o.FaseTipo == Crm_FaseTipo.Aperta).ToList();
            var filtri = new List<string>();

            if (slots.TryGetValue("customerName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var clienti = Trova(aperte.Select(o => o.Cliente).Distinct(), nome, c => c);
                if (clienti.Count == 0)
                    return CommandResult.Ok(CrmMessages.Get("ChatOpportunitiesNoneCustomer", nome));
                if (clienti.Count > 1)
                    return CommandResult.Fail(CrmMessages.Get("ChatCustomerAmbiguous", clienti.Count, nome, string.Join(", ", clienti.Take(8))));
                aperte = aperte.Where(o => o.Cliente == clienti[0]).ToList();
                filtri.Add(clienti[0]);
            }

            var fromText = slots.GetValueOrDefault("from");
            var toText = slots.GetValueOrDefault("to");
            if (!string.IsNullOrWhiteSpace(fromText) || !string.IsNullOrWhiteSpace(toText))
            {
                var dal = ParseData(fromText, DateTime.MinValue);
                var al = ParseData(toText, dal == DateTime.MinValue ? DateTime.Today : new DateTime(dal.Year, dal.Month, 1).AddMonths(1).AddDays(-1));
                aperte = aperte.Where(o => o.DataChiusuraPrevista.HasValue && o.DataChiusuraPrevista.Value.Date >= dal && o.DataChiusuraPrevista.Value.Date <= al).ToList();
                filtri.Add(dal == DateTime.MinValue ? CrmMessages.Get("ChatClosingBy", Data(al)) : Periodo(dal, al));
            }

            var filtro = filtri.Count > 0 ? $" ({string.Join(", ", filtri)})" : string.Empty;
            if (aperte.Count == 0)
                return CommandResult.Ok(CrmMessages.Get("ChatOpportunitiesNone", filtro));

            var valuta = (await Setup(_configService))?.Cfg?.Valuta;
            var totale = aperte.Sum(o => o.Valore);
            var pesato = aperte.Sum(o => o.Valore * o.Probabilita / 100m);
            var titolo = CrmMessages.Get("ChatOpportunitiesTitle", aperte.Count, filtro, Importo(totale, valuta), Importo(pesato, valuta));
            return CommandResult.Ok(Elenco(titolo, aperte.Select(o =>
            {
                var chiusura = o.DataChiusuraPrevista.HasValue ? " — " + CrmMessages.Get("ChatClosingOn", Data(o.DataChiusuraPrevista.Value)) : string.Empty;
                return $"{o.Titolo} — {o.Cliente} — {Importo(o.Valore, valuta)} — {o.Fase} ({o.Probabilita}%){chiusura}";
            })));
        }
    }

    // R3 — pipeline per fase (opportunità aperte ad oggi) e risultato del mese
    public class CrmPipelineHandler : BaseCommandHandler
    {
        private readonly ICrm_ReportService _reportService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmPipelineHandler(ICrm_ReportService reportService, ICrm_ConfigService configService)
        {
            _reportService = reportService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmPipeline",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatPipelineDisplayName"),
                Description = CrmMessages.Get("ChatPipelineDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "pipeline", "quanto vale", "vendite del mese", "andamento vendite", "sales", "how much" },
                Examples = new() { "com'è la pipeline?", "quanto vale la pipeline?", "come vanno le vendite questo mese?", "how is the pipeline?" },
                RequiresConfirmation = false,
                Pages = new() { "/crmpipeline", "/crmreport" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var res = await _reportService.Crm_Report(new GenericRequest<Crm_ReportInModel>(
                new Crm_ReportInModel { Dal = new DateTime(oggi.Year, oggi.Month, 1), Al = oggi }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var r = res.Data;
            var valuta = (await Setup(_configService))?.Cfg?.Valuta;
            var sb = new StringBuilder();
            if (r.PipelinePerFase.Count == 0 || r.PipelinePerFase.Sum(f => f.Numero) == 0)
                sb.AppendLine(CrmMessages.Get("ChatPipelineEmpty"));
            else
                sb.AppendLine(Elenco(
                    CrmMessages.Get("ChatPipelineTitle", r.PipelinePerFase.Sum(f => f.Numero), Importo(r.PipelinePerFase.Sum(f => f.Valore), valuta), Importo(r.PipelinePerFase.Sum(f => f.ValorePesato), valuta)),
                    r.PipelinePerFase.Where(f => f.Numero > 0).Select(f => CrmMessages.Get("ChatPipelineRow", f.Etichetta, f.Numero, Importo(f.Valore, valuta)))));
            sb.AppendLine();
            sb.AppendLine(CrmMessages.Get("ChatPipelineMonth", r.Vinte, Importo(r.ValoreVinto, valuta), r.Perse));
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }

    // R4 — scheda del cliente in breve: stato, recapiti, responsabile, opportunità, attività
    public class CrmCustomerInfoHandler : BaseCommandHandler
    {
        private readonly ICrm_ClienteService _clienteService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmCustomerInfoHandler(ICrm_ClienteService clienteService, ICrm_ConfigService configService)
        {
            _clienteService = clienteService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmCustomerInfo",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatCustomerInfoDisplayName"),
                Description = CrmMessages.Get("ChatCustomerInfoDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "dimmi del cliente", "info cliente", "telefono", "recapiti", "situazione del cliente", "customer info", "phone number" },
                Examples = new() { "dimmi del cliente Rossi", "che telefono ha Bianchi srl?", "com'è la situazione con Verdi spa?", "tell me about customer Rossi" },
                RequiresConfirmation = false,
                Pages = new() { "/crmcustomerlist", "/crmcustomeredit" },
                Slots = BuildSlots(SlotCliente(true))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (c, errore) = await RisolviCliente(_clienteService, slots.GetValueOrDefault("customerName"));
            if (c == null) return CommandResult.Fail(errore!);

            var valuta = (await Setup(_configService))?.Cfg?.Valuta;
            var righe = new List<string> { CrmMessages.Get("ChatCustomerInfoStatus", CrmMessages.Get($"CustomerStatus{(int)c.Stato}")) };
            if (!string.IsNullOrWhiteSpace(c.Citta)) righe.Add(CrmMessages.Get("ChatCustomerInfoCity", c.Citta));
            if (!string.IsNullOrWhiteSpace(c.Telefono)) righe.Add(CrmMessages.Get("ChatCustomerInfoPhone", c.Telefono));
            if (!string.IsNullOrWhiteSpace(c.Email)) righe.Add(CrmMessages.Get("ChatCustomerInfoEmail", c.Email));
            if (!string.IsNullOrWhiteSpace(c.Responsabile)) righe.Add(CrmMessages.Get("ChatCustomerInfoOwner", c.Responsabile));
            righe.Add(c.OpportunitaAperte > 0
                ? CrmMessages.Get("ChatCustomerInfoOpportunities", c.OpportunitaAperte, Importo(c.ValoreAperto, valuta))
                : CrmMessages.Get("ChatCustomerInfoNoOpportunities"));
            if (c.UltimaAttivita.HasValue) righe.Add(CrmMessages.Get("ChatCustomerInfoLastActivity", Data(c.UltimaAttivita.Value)));
            if (c.ProssimaAttivita.HasValue) righe.Add(CrmMessages.Get("ChatCustomerInfoNextActivity", Data(c.ProssimaAttivita.Value)));

            return CommandResult.Ok(Elenco(c.Descrizione, righe));
        }
    }
}
