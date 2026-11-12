using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets.ChatAstHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets
{
    // Scritture di Risorse aziendali dalla chat: riepilogo e conferma (anche a voce) prima di eseguire.
    // Le regole (conflitti, durata massima, anticipo massimo, prenotazioni passate) restano nel servizio.
    // ANALISI_ASSETS_VOCALI.md §3.2

    // W1 — prenota una risorsa (sala, postazione, auto in pool) a nome dell'utente
    public class AstBookResourceHandler : BaseCommandHandler
    {
        // valore di "end" calcolato da "start" (si ricalcola se l'utente cambia l'inizio)
        private const string FineCalcolata = "_endAuto";

        private readonly IAst_PrenotazioniService _service;
        private readonly IntentDefinition _intentDefinition;

        public AstBookResourceHandler(IAst_PrenotazioniService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstBookResource",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryBookings"),
                CategoryOrder = CategoryOrderPrenotazioni,
                DisplayName = AssetsMessages.Get("ChatBookResourceDisplayName"),
                Description = AssetsMessages.Get("ChatBookResourceDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "prenota", "prenotami", "riserva", "blocca la sala", "prenotare", "book", "reserve" },
                Examples = new() { "prenota la sala riunioni domani dalle 10 alle 11", "prenotami una postazione per lunedì dalle 9 alle 13", "prenota un'auto aziendale venerdì dalle 8 alle 18 per la visita al cliente", "book the meeting room tomorrow from 3 to 4 pm" },
                SummaryTemplate = AssetsMessages.Get("ChatBookResourceSummary"),
                Pages = new() { "/astbookings", "/astbookingedit" },
                Slots = BuildSlots(
                    SlotRisorsa(true),
                    SlotData("date", false, "oggi", AssetsMessages.Get("ChatDateQuestion"), AssetsMessages.Get("ChatDateLabel")),
                    SlotOrario("start", true, AssetsMessages.Get("ChatStartQuestion"), AssetsMessages.Get("ChatStartLabel"),
                        "ora di INIZIO della prenotazione ('dalle 10' -> '10')"),
                    SlotOrario("end", false, AssetsMessages.Get("ChatEndQuestion"), AssetsMessages.Get("ChatEndLabel"),
                        "ora di FINE della prenotazione ('alle 11' dopo 'dalle 10' -> '11'); ometti se l'utente non la dice (vale un'ora)"),
                    new SlotDefinition
                    {
                        Name = "title",
                        Type = "string",
                        Required = false,
                        PromptDescription = "motivo della prenotazione se l'utente lo dice (es. 'per la riunione con il cliente' -> 'riunione con il cliente'), altrimenti ometti",
                        Question = AssetsMessages.Get("ChatTitleQuestion"),
                        Label = AssetsMessages.Get("ChatTitleLabel"),
                        HasRelevantContent = msg => !string.IsNullOrWhiteSpace(msg)
                    }),
                CrossValidator = ControllaFascia
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // fine = inizio + un'ora se non detta (così il riepilogo la mostra); la fine deve seguire l'inizio
        private static SlotValidationResult? ControllaFascia(Dictionary<string, string> slots)
        {
            var start = ParseOra(slots.GetValueOrDefault("start"));
            if (start == null) return null;

            var endText = slots.GetValueOrDefault("end");
            if (string.IsNullOrWhiteSpace(endText) || endText == slots.GetValueOrDefault(FineCalcolata))
            {
                var fine = start.Value.AddMinutes(DurataPredefinitaMinuti);
                if (fine < start.Value) fine = new TimeOnly(23, 59);
                var testo = fine.ToString("HH:mm", CultureInfo.InvariantCulture);
                slots["end"] = testo;
                slots[FineCalcolata] = testo;
                return null;
            }

            var end = ParseOra(endText);
            if (end != null && end <= start)
                return SlotValidationResult.Failed(SlotValidationError.BusinessRuleViolation, AssetsMessages.Get("ChatEndBeforeStart", endText, slots["start"]), "end");
            return null;
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorno = ParseData(slots.GetValueOrDefault("date"), DateTime.Today);
            var start = ParseOra(slots.GetValueOrDefault("start"));
            if (start == null) return CommandResult.Fail(AssetsMessages.Get("ChatStartQuestion"));
            var inizio = giorno.Add(start.Value.ToTimeSpan());
            var end = ParseOra(slots.GetValueOrDefault("end"));
            var fine = end != null && end > start ? giorno.Add(end.Value.ToTimeSpan()) : inizio.AddMinutes(DurataPredefinitaMinuti);

            // risorse attive e prenotazioni già presenti nella fascia (degli altri solo "occupato")
            var lista = await _service.Ast_PrenotazioneList(new GenericRequest<Ast_PrenotazioneListInModel>(
                new Ast_PrenotazioneListInModel { Dal = inizio, Al = fine }), false);
            if (!lista.Success || lista.Data == null) return Fallito(lista);

            var nome = slots.GetValueOrDefault("resource");
            var (candidate, generico) = RisolviRisorse(lista.Data.Risorsa, nome);
            if (candidate.Count == 0)
                return CommandResult.Fail(AssetsMessages.Get("ChatResourceNotFound", nome,
                    string.Join(", ", lista.Data.Risorsa.Where(r => r.Attiva).Take(10).Select(r => r.Descrizione))));

            var occupate = lista.Data.Prenotazione
                .Where(p => (p.Stato == (int)AstStatoPrenotazione.Confermata || p.Stato == (int)AstStatoPrenotazione.InAttesa) && p.Inizio < fine && p.Fine > inizio)
                .Select(p => p.IdAst_Risorsa).ToHashSet();

            Ast_RisorsaModel risorsa;
            if (generico)
            {
                // "una sala": la prima libera nella fascia
                var libera = candidate.Where(r => !occupate.Contains(r.Id)).OrderBy(r => r.Descrizione).FirstOrDefault();
                if (libera == null)
                    return CommandResult.Fail(AssetsMessages.Get("ChatBookResourceNoneFree", nome, Fascia(inizio, fine)));
                risorsa = libera;
            }
            else if (candidate.Count > 1)
                return CommandResult.Fail(AssetsMessages.Get("ChatResourceAmbiguous", candidate.Count, nome, string.Join(", ", candidate.Take(8).Select(r => r.Descrizione))));
            else
                risorsa = candidate[0];

            // il servizio ricontrolla tutto (conflitti anche concorrenti, durata e anticipo massimi, passato)
            var titolo = slots.GetValueOrDefault("title");
            var res = await _service.Ast_PrenotazionePut(new GenericRequest<Ast_PrenotazionePutInModel>(new Ast_PrenotazionePutInModel
            {
                Prenotazione = new Ast_PrenotazioneModel
                {
                    Id = 0,
                    IdAst_Risorsa = risorsa.Id,
                    Inizio = inizio,
                    Fine = fine,
                    Titolo = string.IsNullOrWhiteSpace(titolo) ? null : titolo.Trim()
                }
            }), false);
            if (!res.Success) return Fallito(res);

            // risorsa con approvazione: la prenotazione resta in attesa della decisione di un amministratore
            if (res.Data?.Prenotazione.Stato == (int)AstStatoPrenotazione.InAttesa)
                return CommandResult.Ok(AssetsMessages.Get("ChatBookResourcePending", risorsa.Descrizione, Fascia(inizio, fine)));
            return CommandResult.Ok(AssetsMessages.Get("ChatBookResourceDone", risorsa.Descrizione, Fascia(inizio, fine)));
        }
    }

    // W2 — annulla una MIA prenotazione futura (quelle degli altri si annullano dalla pagina, con la notifica)
    public class AstCancelBookingHandler : BaseCommandHandler
    {
        private const int GiorniRicerca = 62;

        private readonly IAst_PrenotazioniService _service;
        private readonly IntentDefinition _intentDefinition;

        public AstCancelBookingHandler(IAst_PrenotazioniService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstCancelBooking",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryBookings"),
                CategoryOrder = CategoryOrderPrenotazioni,
                DisplayName = AssetsMessages.Get("ChatCancelBookingDisplayName"),
                Description = AssetsMessages.Get("ChatCancelBookingDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "annulla la prenotazione", "cancella la prenotazione", "disdici", "disdire", "non mi serve più la sala", "cancel my booking", "cancel the reservation" },
                Examples = new() { "annulla la prenotazione della sala riunioni di domani", "disdici la postazione di lunedì", "cancella la mia prenotazione dell'auto di venerdì alle 8", "cancel my booking for tomorrow" },
                SummaryTemplate = AssetsMessages.Get("ChatCancelBookingSummary"),
                Pages = new() { "/astbookings", "/astbookingedit" },
                Slots = BuildSlots(
                    SlotRisorsa(false, "risorsa della prenotazione da annullare, se detta"),
                    SlotData("date", false, string.Empty, AssetsMessages.Get("ChatDateQuestion"), AssetsMessages.Get("ChatDateLabel"),
                        "giorno della prenotazione se l'utente lo dice, altrimenti ometti"),
                    SlotOrario("start", false, AssetsMessages.Get("ChatStartQuestion"), AssetsMessages.Get("ChatStartLabel"),
                        "ora di inizio della prenotazione se l'utente la dice, altrimenti ometti"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var res = await _service.Ast_PrenotazioneList(new GenericRequest<Ast_PrenotazioneListInModel>(
                new Ast_PrenotazioneListInModel { Dal = oggi, Al = oggi.AddDays(GiorniRicerca), SoloMie = true }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            // solo le mie, confermate o in attesa di approvazione e non ancora finite
            var candidate = res.Data.Prenotazione.Where(p => p.Mia && p.Modificabile).ToList();

            var nome = slots.GetValueOrDefault("resource");
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var (risorse, _) = RisolviRisorse(res.Data.Risorsa, nome);
                var ids = risorse.Select(r => r.Id).ToHashSet();
                // anche per nome della risorsa sulla prenotazione (risorsa disattivata nel frattempo)
                candidate = candidate.Where(p => ids.Contains(p.IdAst_Risorsa) || Trova(new[] { p }, nome, x => x.Risorsa).Count > 0).ToList();
            }
            if (slots.TryGetValue("date", out var giorno) && !string.IsNullOrWhiteSpace(giorno))
            {
                var data = ParseData(giorno, oggi);
                candidate = candidate.Where(p => p.Inizio.Date <= data && p.Fine.Date >= data).ToList();
            }
            var ora = ParseOra(slots.GetValueOrDefault("start"));
            if (ora != null)
                candidate = candidate.Where(p => TimeOnly.FromDateTime(p.Inizio) == ora).ToList();

            if (candidate.Count == 0)
                return CommandResult.Fail(AssetsMessages.Get("ChatCancelBookingNone"));
            if (candidate.Count > 1)
                return CommandResult.Fail(AssetsMessages.Get("ChatCancelBookingAmbiguous", candidate.Count,
                    string.Join("; ", candidate.OrderBy(p => p.Inizio).Take(6).Select(p => $"{p.Risorsa} {Fascia(p.Inizio, p.Fine)}"))));

            var prenotazione = candidate[0];
            var esito = await _service.Ast_PrenotazioneAnnulla(new GenericRequest<Ast_IdInModel>(new Ast_IdInModel { Id = prenotazione.Id }), false);
            if (!esito.Success) return Fallito(esito);

            return CommandResult.Ok(AssetsMessages.Get("ChatCancelBookingDone", prenotazione.Risorsa, Fascia(prenotazione.Inizio, prenotazione.Fine)));
        }
    }
}
