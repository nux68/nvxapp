using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance.ChatCmpHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance
{
    // Interrogazioni di Compliance dalla chat (sola lettura, senza conferma). ANALISI_COMPLIANCE_VOCALI.md §3.1
    // Le segnalazioni del whistleblowing sono escluse dalla chat (ANALISI_COMPLIANCE_APPLICATIVO.md §3, §5).

    // R1 — scadenze della sicurezza sul lavoro: corsi obbligatori, visite mediche, nomine, documenti (DVR)
    public class CmpSafetyExpiriesHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpSafetyExpiriesHandler(ICmp_SicurezzaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpSafetyExpiries",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatSafetyExpiriesDisplayName"),
                Description = ComplianceMessages.Get("ChatSafetyExpiriesDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "scadenze sicurezza", "scadenze compliance", "scadenze della sicurezza", "visite mediche", "visita medica", "dvr", "nomine", "sicurezza sul lavoro", "safety deadlines", "medical examinations", "compliance deadlines" },
                Examples = new() { "quali sono le scadenze della sicurezza?", "visite mediche in scadenza nei prossimi 60 giorni", "il DVR è scaduto?", "scadenze compliance del mese", "which safety deadlines are coming up?" },
                RequiresConfirmation = false,
                Pages = new() { "/cmp81" },
                Slots = BuildSlots(
                    SlotGiorni(),
                    new SlotDefinition
                    {
                        Name = "area",
                        Type = "string",
                        Required = false,
                        PromptDescription = "solo se l'utente limita la domanda a un argomento: 'corsi' (corsi/formazione sicurezza), 'visite' (visite mediche, sorveglianza sanitaria), 'nomine' (RSPP, RLS, preposti, addetti), 'documenti' (DVR, piani di emergenza, procedure); altrimenti ometti",
                        Question = ComplianceMessages.Get("ChatAreaQuestion"),
                        Label = ComplianceMessages.Get("ChatAreaLabel"),
                        Normalizer = NormalizzaArea,
                        Validator = v => v is "corsi" or "visite" or "nomine" or "documenti"
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatAreaQuestion"), "area")
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        private static string? NormalizzaArea(string valore)
        {
            var n = valore.Trim().ToLowerInvariant();
            if (n.Contains("cors") || n.Contains("formazion") || n.Contains("course") || n.Contains("training")) return "corsi";
            if (n.Contains("visit") || n.Contains("sanitari") || n.Contains("medic") || n.Contains("exam")) return "visite";
            if (n.Contains("nomin") || n.Contains("rspp") || n.Contains("rls") || n.Contains("prepost") || n.Contains("appointment")) return "nomine";
            if (n.Contains("document") || n.Contains("dvr") || n.Contains("piano") || n.Contains("procedur") || n.Contains("plan")) return "documenti";
            return null;
        }

        private sealed record Voce(DateTime Scadenza, string Testo);

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (lookup, errore) = await Lookup(_service);
            if (lookup == null) return CommandResult.Fail(errore!);

            var oggi = DateTime.Today;
            var giorni = int.TryParse(slots.GetValueOrDefault("days"), out var g) && g > 0 ? g : lookup.GiorniPreavviso > 0 ? lookup.GiorniPreavviso : 30;
            var limite = oggi.AddDays(giorni);
            var area = slots.GetValueOrDefault("area");
            bool Tutte(string a) => string.IsNullOrWhiteSpace(area) || area == a;

            var voci = new List<Voce>();
            int corsiMancanti = 0;
            var richiesta = new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel());

            if (Tutte("corsi"))
            {
                var res = await _service.Cmp_FormazioneStato(richiesta, false);
                if (!res.Success || res.Data == null) return Fallito(res);
                corsiMancanti = res.Data.Items.Count(x => x.Stato == 0);
                voci.AddRange(res.Data.Items.Where(x => x.Stato != 0 && x.DataScadenza != null && x.DataScadenza.Value.Date <= limite)
                    .Select(x => new Voce(x.DataScadenza!.Value.Date, ComplianceMessages.Get("ChatExpiryCourse", x.Corso ?? TipoCorso(x.TipoCorso), x.Dipendente))));
            }
            if (Tutte("visite"))
            {
                // solo la data della prossima visita: esito, limitazioni e dati sanitari non escono dalla pagina
                var res = await _service.Cmp_VisitaList(richiesta, false);
                if (!res.Success || res.Data == null) return Fallito(res);
                voci.AddRange(res.Data.Items.Where(x => !x.Rinnovata && x.DataScadenza != null && x.DataScadenza.Value.Date <= limite)
                    .Select(x => new Voce(x.DataScadenza!.Value.Date, ComplianceMessages.Get("ChatExpiryVisit", x.Dipendente))));
            }
            if (Tutte("nomine"))
            {
                // per tipo e persona conta l'ultima nomina (una nomina rinnovata non è più in scadenza)
                var res = await _service.Cmp_NominaList(richiesta, false);
                if (!res.Success || res.Data == null) return Fallito(res);
                voci.AddRange(res.Data.Items.GroupBy(x => (x.Tipo, x.Persona)).Select(gr => gr.OrderByDescending(x => x.DataInizio).First())
                    .Where(x => x.DataFine != null && x.DataFine.Value.Date <= limite)
                    .Select(x => new Voce(x.DataFine!.Value.Date, ComplianceMessages.Get("ChatExpiryAppointment", TipoNomina(x.Tipo), x.Persona))));
            }
            if (Tutte("documenti"))
            {
                // per tipo e titolo conta l'ultima revisione
                var res = await _service.Cmp_DocumentoList(richiesta, false);
                if (!res.Success || res.Data == null) return Fallito(res);
                voci.AddRange(res.Data.Items.GroupBy(x => (x.Tipo, Titolo: x.Titolo.Trim().ToLowerInvariant())).Select(gr => gr.OrderByDescending(x => x.DataRevisione).First())
                    .Where(x => x.DataScadenza != null && x.DataScadenza.Value.Date <= limite)
                    .Select(x => new Voce(x.DataScadenza!.Value.Date, ComplianceMessages.Get("ChatExpiryDocument", TipoDocumento(x.Tipo), x.Titolo))));
            }

            var scadute = voci.Where(v => v.Scadenza < oggi).OrderBy(v => v.Scadenza).ToList();
            var prossime = voci.Where(v => v.Scadenza >= oggi).OrderBy(v => v.Scadenza).ToList();

            var sb = new StringBuilder();
            if (voci.Count == 0)
                sb.AppendLine(ComplianceMessages.Get("ChatSafetyExpiriesNone", giorni));
            if (scadute.Count > 0)
                sb.AppendLine(Elenco(ComplianceMessages.Get("ChatSafetyExpiriesOverdue", scadute.Count), scadute.Select(v => $"{Data(v.Scadenza)} — {v.Testo}"), 10));
            if (prossime.Count > 0)
            {
                if (scadute.Count > 0) sb.AppendLine();
                sb.AppendLine(Elenco(ComplianceMessages.Get("ChatSafetyExpiriesUpcoming", prossime.Count, giorni), prossime.Select(v => $"{Data(v.Scadenza)} — {v.Testo}")));
            }
            if (corsiMancanti > 0)
            {
                sb.AppendLine();
                sb.AppendLine(ComplianceMessages.Get("ChatSafetyExpiriesMissingCourses", corsiMancanti));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }

    // R2 — situazione della formazione obbligatoria sulla sicurezza (di tutti o di un dipendente)
    public class CmpSafetyTrainingHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpSafetyTrainingHandler(ICmp_SicurezzaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpSafetyTraining",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatSafetyTrainingDisplayName"),
                Description = ComplianceMessages.Get("ChatSafetyTrainingDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "formazione sicurezza", "corsi sicurezza", "corsi obbligatori", "formazione obbligatoria", "corsi di sicurezza", "safety training", "mandatory training", "safety courses" },
                Examples = new() { "com'è la situazione della formazione sicurezza?", "chi non ha fatto i corsi obbligatori?", "corsi sicurezza di Rossi", "safety training status" },
                RequiresConfirmation = false,
                Pages = new() { "/cmp81" },
                Slots = BuildSlots(SlotDipendente(false))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _service.Cmp_FormazioneStato(new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel()), false);
            if (!res.Success || res.Data == null) return Fallito(res);
            var righe = res.Data.Items;

            var nome = slots.GetValueOrDefault("person");
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var nomi = Trova(righe.Select(x => x.Dipendente ?? string.Empty).Distinct(), nome, x => x);
                if (nomi.Count == 0)
                    return CommandResult.Ok(ComplianceMessages.Get("ChatSafetyTrainingNonePerson", nome));
                if (nomi.Count > 1)
                    return CommandResult.Fail(ComplianceMessages.Get("ChatPersonAmbiguous", nomi.Count, nome, string.Join(", ", nomi.Take(8))));
                var sue = righe.Where(x => x.Dipendente == nomi[0]).OrderBy(x => x.Stato == 1 ? 9 : x.Stato).ThenBy(x => x.TipoCorso).ToList();
                return CommandResult.Ok(Elenco(ComplianceMessages.Get("ChatSafetyTrainingPersonTitle", nomi[0], sue.Count), sue.Select(Riga)));
            }

            if (righe.Count == 0)
                return CommandResult.Ok(ComplianceMessages.Get("ChatSafetyTrainingEmpty"));

            var mancanti = righe.Count(x => x.Stato == 0);
            var scaduti = righe.Count(x => x.Stato == 3);
            var inScadenza = righe.Count(x => x.Stato == 2);
            var validi = righe.Count(x => x.Stato == 1);
            var sb = new StringBuilder();
            sb.AppendLine(ComplianceMessages.Get("ChatSafetyTrainingSummary", righe.Count, righe.Select(x => x.IdDip_Anagrafica).Distinct().Count(), validi, inScadenza, scaduti, mancanti));
            var problemi = righe.Where(x => x.Stato is 0 or 3).OrderBy(x => x.Dipendente).ThenBy(x => x.TipoCorso).ToList();
            if (problemi.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Elenco(ComplianceMessages.Get("ChatSafetyTrainingProblems", problemi.Count), problemi.Select(x => $"{x.Dipendente} — {Riga(x)}")));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }

        // "Antincendio: scaduto il 05/11/2026"
        private static string Riga(Cmp_FormazioneStatoModel x)
        {
            var corso = x.Corso ?? TipoCorso(x.TipoCorso);
            return x.Stato switch
            {
                0 => ComplianceMessages.Get("ChatTrainingMissing", corso),
                3 => ComplianceMessages.Get("ChatTrainingExpired", corso, Data(x.DataScadenza!.Value)),
                2 => ComplianceMessages.Get("ChatTrainingExpiring", corso, Data(x.DataScadenza!.Value)),
                _ => x.DataScadenza != null
                    ? ComplianceMessages.Get("ChatTrainingValidUntil", corso, Data(x.DataScadenza.Value))
                    : ComplianceMessages.Get("ChatTrainingValid", corso)
            };
        }
    }

    // R3 — violazioni dei dati (data breach) da notificare al Garante e termine delle 72 ore
    public class CmpDataBreachesHandler : BaseCommandHandler
    {
        private readonly ICmp_GdprService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpDataBreachesHandler(ICmp_GdprService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpDataBreaches",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategoryGdpr"),
                CategoryOrder = CategoryOrderGdpr,
                DisplayName = ComplianceMessages.Get("ChatDataBreachesDisplayName"),
                Description = ComplianceMessages.Get("ChatDataBreachesDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "data breach", "databreach", "violazioni dei dati", "violazione dei dati", "violazioni gdpr", "violazioni privacy", "72 ore", "garante", "breach", "breaches" },
                Examples = new() { "ci sono data breach aperti?", "violazioni dei dati da notificare al Garante", "quanto manca alle 72 ore per la violazione?", "are there open data breaches?" },
                RequiresConfirmation = false,
                Pages = new() { "/cmpgdpr" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _service.Cmp_ViolazioneList(new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel()), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var adesso = DateTime.Now;
            var anno = res.Data.Items.Count(x => x.DataScoperta.Year == adesso.Year);
            // né descrizione né dati coinvolti: solo date, rischio e stato della notifica
            var aperte = res.Data.Items.Where(x => x.NotificaDovuta).OrderBy(x => x.TermineNotifica).ToList();
            if (aperte.Count == 0)
                return CommandResult.Ok(ComplianceMessages.Get("ChatDataBreachesNone", anno));

            var testo = Elenco(ComplianceMessages.Get("ChatDataBreachesTitle", aperte.Count), aperte.Select(x =>
            {
                var rischio = ComplianceMessages.Get("BreachRisk_" + Math.Clamp(x.Rischio, 0, 2));
                if (x.TermineNotifica < adesso)
                    return ComplianceMessages.Get("ChatDataBreachLate", DataOra(x.DataScoperta), rischio, DataOra(x.TermineNotifica));
                var ore = (int)Math.Ceiling((x.TermineNotifica - adesso).TotalHours);
                return ComplianceMessages.Get("ChatDataBreachOpen", DataOra(x.DataScoperta), rischio, DataOra(x.TermineNotifica), ore);
            }));
            return CommandResult.Ok(testo + "\n" + ComplianceMessages.Get("ChatDataBreachesYear", anno));
        }
    }

    // R4 — stato del registro GDPR: trattamenti, responsabili esterni (contratti), violazioni
    public class CmpGdprRegisterHandler : BaseCommandHandler
    {
        private readonly ICmp_GdprService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpGdprRegisterHandler(ICmp_GdprService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpGdprRegister",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategoryGdpr"),
                CategoryOrder = CategoryOrderGdpr,
                DisplayName = ComplianceMessages.Get("ChatGdprRegisterDisplayName"),
                Description = ComplianceMessages.Get("ChatGdprRegisterDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "registro gdpr", "registro trattamenti", "registro dei trattamenti", "gdpr", "privacy", "responsabili esterni", "trattamenti", "gdpr register", "processing register", "processors" },
                Examples = new() { "com'è messo il registro GDPR?", "stato del registro dei trattamenti", "ci sono contratti dei responsabili esterni in scadenza?", "how is the GDPR register?" },
                RequiresConfirmation = false,
                Pages = new() { "/cmpgdpr" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var richiesta = new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel());
            var trattamenti = await _service.Cmp_TrattamentoList(richiesta, false);
            if (!trattamenti.Success || trattamenti.Data == null) return Fallito(trattamenti);
            var responsabili = await _service.Cmp_ResponsabileList(richiesta, false);
            if (!responsabili.Success || responsabili.Data == null) return Fallito(responsabili);
            var violazioni = await _service.Cmp_ViolazioneList(richiesta, false);
            if (!violazioni.Success || violazioni.Data == null) return Fallito(violazioni);

            var attivi = trattamenti.Data.Items.Where(x => x.Attivo).ToList();
            // incompleti: mancano le voci che l'art. 30 chiede sempre
            var incompleti = attivi.Where(x => string.IsNullOrWhiteSpace(x.Finalita) || string.IsNullOrWhiteSpace(x.CategorieInteressati)
                                               || string.IsNullOrWhiteSpace(x.CategorieDati) || string.IsNullOrWhiteSpace(x.TerminiCancellazione)
                                               || string.IsNullOrWhiteSpace(x.MisureSicurezza)).ToList();
            var righe = new List<string>
            {
                ComplianceMessages.Get("ChatGdprTreatments", attivi.Count, attivi.Count(x => x.DatiParticolari)),
            };
            if (incompleti.Count > 0)
                righe.Add(ComplianceMessages.Get("ChatGdprTreatmentsIncomplete", incompleti.Count, string.Join(", ", incompleti.Take(5).Select(x => x.Denominazione))));

            var resp = responsabili.Data.Items;
            righe.Add(ComplianceMessages.Get("ChatGdprProcessors", resp.Count, resp.Count(x => x.Scaduta), resp.Count(x => x.InScadenza)));
            foreach (var r in resp.Where(x => x.Scaduta || x.InScadenza).OrderBy(x => x.DataScadenza).Take(5))
                righe.Add(ComplianceMessages.Get(r.Scaduta ? "ChatGdprProcessorExpired" : "ChatGdprProcessorExpiring", r.Denominazione, Data(r.DataScadenza!.Value)));

            var anno = DateTime.Today.Year;
            righe.Add(ComplianceMessages.Get("ChatGdprBreaches", violazioni.Data.Items.Count(x => x.DataScoperta.Year == anno), violazioni.Data.Items.Count(x => x.NotificaDovuta)));

            return CommandResult.Ok(Elenco(ComplianceMessages.Get("ChatGdprRegisterTitle"), righe));
        }
    }

    // R5 — i miei adempimenti: corsi con scadenza, prossima visita (solo la data), DPI da confermare, nomine
    public class CmpMyComplianceHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpMyComplianceHandler(ICmp_SicurezzaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpMyCompliance",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatMyComplianceDisplayName"),
                Description = ComplianceMessages.Get("ChatMyComplianceDescription"),
                Usable4Role = RuoliTutti,
                Keywords = new() { "miei adempimenti", "miei corsi sicurezza", "mia visita medica", "mie visite", "miei dpi", "mie nomine", "my compliance", "my safety courses", "my medical", "my ppe" },
                Examples = new() { "quando scade la mia visita medica?", "i miei corsi sicurezza sono in regola?", "che DPI ho ricevuto?", "my safety courses" },
                RequiresConfirmation = false,
                Pages = new() { "/cmpmine" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var res = await _service.Cmp_Mine(new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel()), false);
            if (!res.Success || res.Data == null) return Fallito(res);
            var m = res.Data;
            if (!m.Dipendente)
                return CommandResult.Ok(ComplianceMessages.Get("ChatMyComplianceNoEmployee"));

            var righe = new List<string>();
            foreach (var c in m.Formazione.Where(x => !x.Rinnovata).OrderBy(x => x.DataScadenza ?? DateTime.MaxValue))
            {
                var corso = string.IsNullOrWhiteSpace(c.Descrizione) ? TipoCorso(c.TipoCorso) : c.Descrizione;
                righe.Add(c.DataScadenza == null ? ComplianceMessages.Get("ChatMyCourseNoExpiry", corso, Data(c.DataCorso))
                    : c.Scaduta ? ComplianceMessages.Get("ChatTrainingExpired", corso, Data(c.DataScadenza.Value))
                    : ComplianceMessages.Get("ChatTrainingValidUntil", corso, Data(c.DataScadenza.Value)));
            }
            if (m.Formazione.Count == 0)
                righe.Add(ComplianceMessages.Get("ChatMyNoCourses"));

            // visita: solo la scadenza della prossima, mai l'esito
            var visita = m.Visite.FirstOrDefault(x => !x.Rinnovata);
            if (visita?.DataScadenza != null)
                righe.Add(ComplianceMessages.Get(visita.Scaduta ? "ChatMyVisitExpired" : "ChatMyVisitNext", Data(visita.DataScadenza.Value)));

            var daConfermare = m.Consegne.Count(x => x.PresaVisioneRichiesta && x.DataPresaVisione == null)
                             + m.Nomine.Count(x => x.PresaVisioneRichiesta && x.DataPresaVisione == null);
            if (m.Consegne.Count > 0)
                righe.Add(ComplianceMessages.Get("ChatMyPpe", m.Consegne.Count,
                    string.Join(", ", m.Consegne.OrderByDescending(x => x.DataConsegna).Take(4).Select(x => $"{x.Dpi} ({Data(x.DataConsegna)})"))));
            foreach (var n in m.Nomine.Where(x => !x.Scaduta))
                righe.Add(ComplianceMessages.Get("ChatMyAppointment", TipoNomina(n.Tipo), Data(n.DataInizio)));
            if (daConfermare > 0)
                righe.Add(ComplianceMessages.Get("ChatMyToAcknowledge", daConfermare));

            return CommandResult.Ok(Elenco(ComplianceMessages.Get("ChatMyComplianceTitle"), righe));
        }
    }
}
