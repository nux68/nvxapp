using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
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

    // ---------------------------------------------------------------------------------------------------------------
    // Letture per gli strumenti AI (ANALISI_INFRASTRUCTURE_MCP.md MCP-32): testo breve per la chat e la voce, dati
    // strutturati (CommandResult.Data) con date ISO e importi numerici, perche' l'AI li possa elaborare.
    // ---------------------------------------------------------------------------------------------------------------

    // R5 — ricerca di clienti e contatti per testo (ragione sociale, città, partita IVA, recapiti, nome / email del contatto)
    public class CrmCustomerSearchHandler : BaseCommandHandler
    {
        private const int MaxRisultati = 30;

        private readonly ICrm_ClienteService _clienteService;
        private readonly IntentDefinition _intentDefinition;

        public CrmCustomerSearchHandler(ICrm_ClienteService clienteService)
        {
            _clienteService = clienteService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmCustomerSearch",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatCustomerSearchDisplayName"),
                Description = CrmMessages.Get("ChatCustomerSearchDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "cerca cliente", "cerca clienti", "trova cliente", "cerca contatto", "partita iva", "clienti di", "chi è il contatto",
                                   "search customer", "find customer", "find contact", "vat number", "customers in" },
                Examples = new() { "cerca i clienti di Milano", "trova il cliente con partita IVA 01234567890", "chi è Mario Bianchi tra i contatti?",
                                   "search customers in Turin", "find the contact with email mario@rossi.it" },
                RequiresConfirmation = false,
                Pages = new() { "/crmcustomerlist" },
                Slots = BuildSlots(SlotTesto("text", true,
                    "testo da cercare come detto dall'utente: parte della ragione sociale, città, partita IVA o codice fiscale, telefono, email, oppure nome / cognome / email di un contatto del cliente; più parole devono comparire tutte",
                    CrmMessages.Get("ChatCustomerSearchTextQuestion"), CrmMessages.Get("ChatCustomerSearchTextLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var testo = slots.GetValueOrDefault("text")?.Trim() ?? string.Empty;
            var res = await _clienteService.Crm_ClienteCerca(new GenericRequest<Crm_ClienteCercaInModel>(
                new Crm_ClienteCercaInModel { Testo = testo, Max = MaxRisultati }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var clienti = res.Data.Clienti;
            var dati = new CrmCustomerSearchData
            {
                Text = testo,
                Total = res.Data.Totale,
                Customers = clienti.Select(c => new CrmCustomerSearchRow
                {
                    Id = c.Id, Customer = c.Descrizione, Type = CrmMessages.Get($"ChatCustomerType{c.Tipo}"),
                    Status = CrmMessages.Get($"CustomerStatus{(int)c.Stato}"), Active = c.Attivo, City = c.Citta, Province = c.Provincia,
                    VatNumber = c.PartitaIva, Phone = c.Telefono, Email = c.Email, Owner = c.Responsabile,
                    MainContact = Contatto(c.ContattoPrincipale),
                    MatchingContacts = c.ContattiTrovati.Select(x => Contatto(x)!).ToList()
                }).ToList()
            };
            if (clienti.Count == 0)
                return CommandResult.Ok(CrmMessages.Get("ChatCustomerSearchNone", testo), dati);

            var titolo = CrmMessages.Get("ChatCustomerSearchTitle", res.Data.Totale, testo);
            if (res.Data.Totale > clienti.Count) titolo += " " + CrmMessages.Get("ChatPartialList", clienti.Count, res.Data.Totale);
            return CommandResult.Ok(Elenco(titolo, clienti.Select(Riga)), dati);
        }

        // "Rossi srl — Milano — Mario Rossi 02 1234567 — contatto: Bianchi Anna"
        private static string Riga(Crm_ClienteCercaModel c)
        {
            var parti = new List<string> { c.Descrizione };
            if (!string.IsNullOrWhiteSpace(c.Citta)) parti.Add(c.Citta);
            var recapito = c.Telefono ?? c.Email;
            if (!string.IsNullOrWhiteSpace(recapito)) parti.Add(recapito);
            var contatti = c.ContattiTrovati.Count > 0 ? c.ContattiTrovati : (c.ContattoPrincipale != null ? new() { c.ContattoPrincipale } : new());
            if (contatti.Count > 0) parti.Add(CrmMessages.Get("ChatCustomerSearchContact", string.Join(", ", contatti.Select(x => x.Nominativo))));
            return string.Join(" — ", parti);
        }

        private static CrmContactRow? Contatto(Crm_ContattoModel? x) => x == null ? null : new CrmContactRow
        {
            Name = x.Nominativo, Role = x.Ruolo, Phone = x.Telefono, Mobile = x.Cellulare, Email = x.Email, Main = x.Principale
        };
    }

    // R6 — preventivi (offerte) filtrabili per cliente, stato e periodo (data del preventivo; default gli ultimi 12 mesi)
    public class CrmQuotesHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 365;
        private const int MaxRighe = 100;

        // stato detto dall'utente: radici delle parole (italiano e inglese); "aperti" = bozza o inviata
        private static readonly (string[] Radici, Crm_StatoOfferta[] Stati)[] _stati =
        {
            (new[] { "bozz", "draft" }, new[] { Crm_StatoOfferta.Bozza }),
            (new[] { "inviat", "spedit", "sent" }, new[] { Crm_StatoOfferta.Inviata }),
            (new[] { "accettat", "approvat", "vint", "accepted", "won" }, new[] { Crm_StatoOfferta.Accettata }),
            (new[] { "rifiutat", "respint", "pers", "rejected", "declined", "lost" }, new[] { Crm_StatoOfferta.Rifiutata }),
            (new[] { "scadut", "expired" }, new[] { Crm_StatoOfferta.Scaduta }),
            (new[] { "apert", "attesa", "pendent", "open", "pending" }, new[] { Crm_StatoOfferta.Bozza, Crm_StatoOfferta.Inviata }),
        };

        private readonly ICrm_OffertaService _offertaService;
        private readonly ICrm_ClienteService _clienteService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmQuotesHandler(ICrm_OffertaService offertaService, ICrm_ClienteService clienteService, ICrm_ConfigService configService)
        {
            _offertaService = offertaService;
            _clienteService = clienteService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmQuotes",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatQuotesDisplayName"),
                Description = CrmMessages.Get("ChatQuotesDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "preventivi", "preventivo", "offerte", "offerta", "preventivi accettati", "preventivi inviati", "quotes", "quote", "offers", "quotations" },
                Examples = new() { "preventivi inviati a Rossi", "preventivi accettati questo mese", "che preventivi abbiamo fatto a ottobre?", "quotes sent to Rossi last month" },
                RequiresConfirmation = false,
                Pages = new() { "/crmofferlist", "/crmofferedit" },
                Slots = BuildSlots(
                    SlotCliente(false),
                    new SlotDefinition
                    {
                        Name = "status",
                        Type = "string",
                        Required = false,
                        PromptDescription = "stato dei preventivi come detto dall'utente: bozza, inviato, accettato, rifiutato, scaduto, oppure 'aperti' (bozza o inviato); ometti se non indicato",
                        Question = CrmMessages.Get("ChatQuoteStatusQuestion"),
                        Label = CrmMessages.Get("ChatQuoteStatusLabel"),
                        Validator = v => Stati(v) != null
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, CrmMessages.Get("ChatQuoteStatusInvalid", v), "status")
                    },
                    SlotData("from", false, string.Empty, CrmMessages.Get("ChatFromQuestion"), CrmMessages.Get("ChatFromLabel"),
                        "INIZIO del periodo sulla data del preventivo; per un mese copia l'espressione (es. 'ottobre', 'questo mese'); ometti se l'utente non indica un periodo (valgono gli ultimi 12 mesi)"),
                    SlotData("to", false, string.Empty, CrmMessages.Get("ChatToQuestion"), CrmMessages.Get("ChatToLabel"),
                        "FINE del periodo sulla data del preventivo; ometti se non indicata (vale oggi)"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // stati corrispondenti al testo (null se non riconosciuto)
        private static Crm_StatoOfferta[]? Stati(string? valore)
        {
            var parole = ChatAIKeywords.Normalize(valore).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return _stati.FirstOrDefault(s => parole.Any(p => s.Radici.Any(r => p.StartsWith(r, StringComparison.Ordinal)))).Stati;
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (dal, al, errorePeriodo) = PeriodoPassato(slots, GiorniPredefiniti);
            if (errorePeriodo != null) return CommandResult.Fail(errorePeriodo);

            var filtri = new List<string>();
            int? idCliente = null;
            var nome = slots.GetValueOrDefault("customerName");
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var (c, errore) = await RisolviCliente(_clienteService, nome);
                if (c == null) return CommandResult.Fail(errore!);
                idCliente = c.Id;
                filtri.Add(c.Descrizione);
            }

            Crm_StatoOfferta[]? stati = null;
            var statoTesto = slots.GetValueOrDefault("status");
            if (!string.IsNullOrWhiteSpace(statoTesto))
            {
                stati = Stati(statoTesto);
                if (stati == null) return CommandResult.Fail(CrmMessages.Get("ChatQuoteStatusInvalid", statoTesto));
                filtri.Add(string.Join(" / ", stati.Select(s => CrmMessages.Get($"ChatOfferState{(int)s}"))));
            }
            filtri.Add(Periodo(dal, al));

            var res = await _offertaService.Crm_OffertaList(new GenericRequest<Crm_OffertaListInModel>(new Crm_OffertaListInModel
            {
                IdAz_Cliente = idCliente, Stato = stati?.Length == 1 ? stati[0] : null, Dal = dal, Al = al
            }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var offerte = res.Data.Offerte
                .Where(o => stati == null || stati.Contains(o.Stato))
                .OrderByDescending(o => o.Data).ThenByDescending(o => o.Id)
                .ToList();
            var valuta = (await Setup(_configService))?.Cfg?.Valuta;
            var filtro = $" ({string.Join(", ", filtri)})";

            var dati = new CrmQuotesData
            {
                From = Iso(dal), To = Iso(al), Customer = idCliente.HasValue ? filtri[0] : null,
                Status = stati?.Select(s => CrmMessages.Get($"ChatOfferState{(int)s}")).ToList(),
                Currency = valuta, Count = offerte.Count, Total = offerte.Sum(o => o.Totale),
                Quotes = offerte.Take(MaxRighe).Select(o => new CrmQuoteRow
                {
                    Id = o.Id, Number = o.NumeroCompleto, Date = Iso(o.Data), ValidUntil = Iso(o.ValidaFinoAl), Customer = o.Cliente,
                    Subject = o.Oggetto, Total = o.Totale, Status = CrmMessages.Get($"ChatOfferState{(int)o.Stato}"), OutcomeDate = Iso(o.DataEsito)
                }).ToList()
            };
            if (offerte.Count == 0)
                return CommandResult.Ok(CrmMessages.Get("ChatQuotesNone", filtro), dati);

            var titolo = CrmMessages.Get("ChatQuotesTitle", offerte.Count, filtro, Importo(dati.Total, valuta));
            if (offerte.Count > MaxRighe) titolo += " " + CrmMessages.Get("ChatPartialList", MaxRighe, offerte.Count);
            return CommandResult.Ok(Elenco(titolo, offerte.Select(o =>
            {
                var oggetto = string.IsNullOrWhiteSpace(o.Oggetto) ? string.Empty : $" — {Breve(o.Oggetto, 50)}";
                return $"{o.NumeroCompleto} {Data(o.Data)} — {o.Cliente}{oggetto} — {Importo(o.Totale, valuta)} — {CrmMessages.Get($"ChatOfferState{(int)o.Stato}")}";
            })), dati);
        }
    }

    // R7 — storico di un cliente: attività (chiamate, appuntamenti, email...) e note in un periodo, dalla più recente
    public class CrmCustomerHistoryHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 90;
        private const int MaxEventi = 100;

        private readonly ICrm_ClienteService _clienteService;
        private readonly IntentDefinition _intentDefinition;

        public CrmCustomerHistoryHandler(ICrm_ClienteService clienteService)
        {
            _clienteService = clienteService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmCustomerHistory",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatCustomerHistoryDisplayName"),
                Description = CrmMessages.Get("ChatCustomerHistoryDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "storico", "cronologia", "cosa abbiamo fatto", "ultimi contatti", "ultime attività", "history", "activity log", "last contacts" },
                Examples = new() { "storico del cliente Rossi", "cosa abbiamo fatto con Bianchi srl negli ultimi tre mesi?", "attività con Verdi spa a ottobre",
                                   "history of customer Rossi" },
                RequiresConfirmation = false,
                Pages = new() { "/crmcustomeredit", "/crmactivitylist" },
                Slots = BuildSlots(
                    SlotCliente(true),
                    SlotData("from", false, string.Empty, CrmMessages.Get("ChatFromQuestion"), CrmMessages.Get("ChatFromLabel"),
                        "INIZIO del periodo; per un mese copia l'espressione (es. 'ottobre'); ometti se l'utente non indica un periodo (valgono gli ultimi 90 giorni)"),
                    SlotData("to", false, string.Empty, CrmMessages.Get("ChatToQuestion"), CrmMessages.Get("ChatToLabel"),
                        "FINE del periodo; ometti se non indicata (vale oggi)"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (dal, al, errorePeriodo) = PeriodoPassato(slots, GiorniPredefiniti);
            if (errorePeriodo != null) return CommandResult.Fail(errorePeriodo);

            var (c, errore) = await RisolviCliente(_clienteService, slots.GetValueOrDefault("customerName"));
            if (c == null) return CommandResult.Fail(errore!);

            var res = await _clienteService.Crm_ClienteStorico(new GenericRequest<Crm_ClienteStoricoInModel>(
                new Crm_ClienteStoricoInModel { IdAz_Cliente = c.Id, Dal = dal, Al = al, Max = MaxEventi }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var eventi = res.Data.Eventi;
            var testoPeriodo = Periodo(dal, al);
            var dati = new CrmCustomerHistoryData
            {
                CustomerId = c.Id, Customer = c.Descrizione, From = Iso(dal), To = Iso(al), Total = res.Data.Totale,
                Events = eventi.Select(e => new CrmHistoryRow
                {
                    Date = Iso(e.Data), Time = e.Nota ? null : Ora(e.Data),
                    Kind = e.Nota ? "note" : "activity",
                    Type = e.Nota ? CrmMessages.Get("ChatHistoryNote") : e.Tipo,
                    Status = e.Stato.HasValue ? CrmMessages.Get($"ChatActivityState{(int)e.Stato.Value}") : null,
                    Subject = e.Oggetto, Description = e.Descrizione, Outcome = e.Esito, User = e.Utente, Contact = e.Contatto, Opportunity = e.Opportunita
                }).ToList()
            };
            if (eventi.Count == 0)
                return CommandResult.Ok(CrmMessages.Get("ChatCustomerHistoryNone", c.Descrizione, testoPeriodo), dati);

            var titolo = CrmMessages.Get("ChatCustomerHistoryTitle", res.Data.Totale, c.Descrizione, testoPeriodo);
            if (res.Data.Totale > eventi.Count) titolo += " " + CrmMessages.Get("ChatPartialList", eventi.Count, res.Data.Totale);
            return CommandResult.Ok(Elenco(titolo, eventi.Select(Riga)), dati);
        }

        // "12/11/2026 10:00 Chiamata — rinnovo contratto — esito: interessato (Mario)"; "12/11/2026 Nota — testo"
        private static string Riga(Crm_ClienteStoricoModel e)
        {
            if (e.Nota)
                return $"{Data(e.Data)} {CrmMessages.Get("ChatHistoryNote")} — {Breve(e.Oggetto)}";
            var sb = new StringBuilder($"{Data(e.Data)} {Ora(e.Data)} {e.Tipo} — {Breve(e.Oggetto, 60)}");
            if (e.Stato != Crm_StatoAttivita.Fatta && e.Stato.HasValue)
                sb.Append($" ({CrmMessages.Get($"ChatActivityState{(int)e.Stato.Value}")})");
            if (!string.IsNullOrWhiteSpace(e.Esito))
                sb.Append(" — ").Append(CrmMessages.Get("ChatHistoryOutcome", Breve(e.Esito, 60)));
            if (!string.IsNullOrWhiteSpace(e.Utente))
                sb.Append($" [{e.Utente}]");
            return sb.ToString();
        }
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Dati strutturati restituiti agli strumenti AI (serializzati in camelCase dall'endpoint MCP)
    // ---------------------------------------------------------------------------------------------------------------

    public class CrmContactRow
    {
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool Main { get; set; }
    }

    public class CrmCustomerSearchRow
    {
        public int Id { get; set; }
        public string Customer { get; set; } = string.Empty;
        // cliente / fornitore / entrambi
        public string Type { get; set; } = string.Empty;
        // stato nel CRM (prospect, cliente...)
        public string Status { get; set; } = string.Empty;
        public bool Active { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? VatNumber { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Owner { get; set; }
        public CrmContactRow? MainContact { get; set; }
        public List<CrmContactRow> MatchingContacts { get; set; } = new();
    }

    public class CrmCustomerSearchData
    {
        public string Text { get; set; } = string.Empty;
        // clienti trovati in tutto (l'elenco ne contiene al massimo 30)
        public int Total { get; set; }
        public List<CrmCustomerSearchRow> Customers { get; set; } = new();
    }

    public class CrmQuoteRow
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? ValidUntil { get; set; }
        public string Customer { get; set; } = string.Empty;
        public string? Subject { get; set; }
        // totale IVA compresa
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? OutcomeDate { get; set; }
    }

    public class CrmQuotesData
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? Customer { get; set; }
        public List<string>? Status { get; set; }
        public string? Currency { get; set; }
        // preventivi trovati e loro totale (l'elenco ne contiene al massimo 100)
        public int Count { get; set; }
        public decimal Total { get; set; }
        public List<CrmQuoteRow> Quotes { get; set; } = new();
    }

    public class CrmHistoryRow
    {
        public string Date { get; set; } = string.Empty;
        public string? Time { get; set; }
        // "activity" oppure "note"
        public string Kind { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Outcome { get; set; }
        public string? User { get; set; }
        public string? Contact { get; set; }
        public string? Opportunity { get; set; }
    }

    public class CrmCustomerHistoryData
    {
        public int CustomerId { get; set; }
        public string Customer { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        // eventi del periodo in tutto (l'elenco ne contiene al massimo 100)
        public int Total { get; set; }
        public List<CrmHistoryRow> Events { get; set; } = new();
    }
}
