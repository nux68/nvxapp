using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets.ChatAstHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets
{
    // Interrogazioni di Risorse aziendali dalla chat (sola lettura, senza conferma). ANALISI_ASSETS_VOCALI.md §3.1

    // R1 — le mie prenotazioni in un periodo (default: i prossimi 14 giorni)
    public class AstMyBookingsHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 14;

        private readonly IAst_PrenotazioniService _service;
        private readonly IntentDefinition _intentDefinition;

        public AstMyBookingsHandler(IAst_PrenotazioniService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstMyBookings",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryBookings"),
                CategoryOrder = CategoryOrderPrenotazioni,
                DisplayName = AssetsMessages.Get("ChatMyBookingsDisplayName"),
                Description = AssetsMessages.Get("ChatMyBookingsDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "le mie prenotazioni", "mie prenotazioni", "cosa ho prenotato", "ho prenotato", "my bookings", "my reservations", "what did i book" },
                Examples = new() { "quali sono le mie prenotazioni?", "cosa ho prenotato domani?", "le mie prenotazioni della prossima settimana", "show my bookings" },
                RequiresConfirmation = false,
                Pages = new() { "/astbookings" },
                Slots = BuildSlots(
                    SlotData("from", false, string.Empty, AssetsMessages.Get("ChatFromQuestion"), AssetsMessages.Get("ChatFromLabel"),
                        "data di INIZIO del periodo delle prenotazioni; per una settimana copia l'espressione (es. 'la prossima settimana'); ometti se l'utente non indica un periodo"),
                    SlotData("to", false, string.Empty, AssetsMessages.Get("ChatToQuestion"), AssetsMessages.Get("ChatToLabel"),
                        "data di FINE del periodo; ometti lo slot se l'utente indica un solo giorno"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var fromText = slots.GetValueOrDefault("from");
            var dal = ParseData(fromText, oggi);
            var al = string.IsNullOrWhiteSpace(fromText) ? oggi.AddDays(GiorniPredefiniti - 1) : ParseData(slots.GetValueOrDefault("to"), dal);
            if (al < dal) al = dal;
            var testoPeriodo = string.IsNullOrWhiteSpace(fromText) ? AssetsMessages.Get("ChatNextDays", GiorniPredefiniti) : Periodo(dal, al);

            var res = await _service.Ast_PrenotazioneList(new GenericRequest<Ast_PrenotazioneListInModel>(
                new Ast_PrenotazioneListInModel { Dal = dal, Al = al.AddDays(1), SoloMie = true }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            // le prenotazioni già finite di oggi non interessano
            var ora = DateTime.Now;
            var mie = res.Data.Prenotazione
                .Where(p => p.Mia && (p.Stato == (int)AstStatoPrenotazione.Confermata || p.Stato == (int)AstStatoPrenotazione.InAttesa) && p.Fine > ora && p.Inizio.Date <= al)
                .OrderBy(p => p.Inizio).ToList();
            if (mie.Count == 0)
                return CommandResult.Ok(AssetsMessages.Get("ChatMyBookingsNone", testoPeriodo));

            return CommandResult.Ok(Elenco(AssetsMessages.Get("ChatMyBookingsTitle", mie.Count, testoPeriodo),
                mie.Select(p => (string.IsNullOrWhiteSpace(p.Titolo)
                    ? $"{Fascia(p.Inizio, p.Fine)} {p.Risorsa}"
                    : $"{Fascia(p.Inizio, p.Fine)} {p.Risorsa} — {p.Titolo}")
                    + (p.Stato == (int)AstStatoPrenotazione.InAttesa ? " " + AssetsMessages.Get("ChatBookingPendingSuffix") : string.Empty))));
        }
    }

    // R2 — risorse libere (sale, postazioni, auto in pool) in una fascia oraria, o la disponibilità del giorno
    public class AstFreeResourcesHandler : BaseCommandHandler
    {
        private readonly IAst_PrenotazioniService _service;
        private readonly IntentDefinition _intentDefinition;

        public AstFreeResourcesHandler(IAst_PrenotazioniService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstFreeResources",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryBookings"),
                CategoryOrder = CategoryOrderPrenotazioni,
                DisplayName = AssetsMessages.Get("ChatFreeResourcesDisplayName"),
                Description = AssetsMessages.Get("ChatFreeResourcesDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "sale libere", "sala libera", "postazioni libere", "auto libere", "è libera", "disponibili", "disponibilità sale", "free rooms", "available", "is the room free" },
                Examples = new() { "che sale sono libere domani dalle 10 alle 11?", "c'è una postazione libera oggi pomeriggio alle 14?", "le auto aziendali sono libere venerdì?", "which rooms are free tomorrow at 3 pm?" },
                RequiresConfirmation = false,
                Pages = new() { "/astbookings", "/astbookingedit" },
                Slots = BuildSlots(
                    SlotTipoRisorsa(),
                    SlotData("date", false, "oggi", AssetsMessages.Get("ChatDateQuestion"), AssetsMessages.Get("ChatDateLabel")),
                    SlotOrario("start", false, AssetsMessages.Get("ChatStartQuestion"), AssetsMessages.Get("ChatStartLabel"),
                        "ora di INIZIO della fascia se l'utente la dice ('dalle 10' -> '10'), altrimenti ometti"),
                    SlotOrario("end", false, AssetsMessages.Get("ChatEndQuestion"), AssetsMessages.Get("ChatEndLabel"),
                        "ora di FINE della fascia se l'utente la dice ('alle 11' dopo 'dalle 10' -> '11'), altrimenti ometti"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorno = ParseData(slots.GetValueOrDefault("date"), DateTime.Today);
            var tipo = TipoDaParola(slots.GetValueOrDefault("resourceType"));
            var start = ParseOra(slots.GetValueOrDefault("start"));
            var end = ParseOra(slots.GetValueOrDefault("end"));

            DateTime dal, al;
            if (start != null)
            {
                dal = giorno.Add(start.Value.ToTimeSpan());
                al = end != null && end > start ? giorno.Add(end.Value.ToTimeSpan()) : dal.AddMinutes(DurataPredefinitaMinuti);
            }
            else
            {
                dal = giorno;
                al = giorno.AddDays(1);
            }

            var res = await _service.Ast_PrenotazioneList(new GenericRequest<Ast_PrenotazioneListInModel>(
                new Ast_PrenotazioneListInModel { Dal = dal, Al = al }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var risorse = res.Data.Risorsa.Where(r => r.Attiva && (tipo == null || r.Tipo == (int)tipo)).ToList();
            var testoTipo = tipo == null ? AssetsMessages.Get("ChatResourcesGeneric") : AssetsMessages.Get("ChatResourcesOfType_" + (int)tipo);
            if (risorse.Count == 0)
                return CommandResult.Ok(AssetsMessages.Get("ChatNoResources", testoTipo));

            var occupate = res.Data.Prenotazione
                .Where(p => (p.Stato == (int)AstStatoPrenotazione.Confermata || p.Stato == (int)AstStatoPrenotazione.InAttesa) && p.Inizio < al && p.Fine > dal)
                .GroupBy(p => p.IdAst_Risorsa).ToDictionary(g => g.Key, g => g.OrderBy(p => p.Inizio).ToList());

            if (start != null)
            {
                // fascia oraria: le risorse senza prenotazioni sovrapposte
                var libere = risorse.Where(r => !occupate.ContainsKey(r.Id)).ToList();
                var fascia = Fascia(dal, al);
                if (libere.Count == 0)
                    return CommandResult.Ok(AssetsMessages.Get("ChatFreeResourcesNone", testoTipo, fascia));
                return CommandResult.Ok(Elenco(AssetsMessages.Get("ChatFreeResourcesTitle", libere.Count, risorse.Count, testoTipo, fascia),
                    libere.Select(DescriviRisorsa)));
            }

            // giorno intero: per ogni risorsa le fasce occupate
            return CommandResult.Ok(Elenco(AssetsMessages.Get("ChatAvailabilityTitle", testoTipo, Data(giorno)),
                risorse.Select(r => occupate.TryGetValue(r.Id, out var p)
                    ? AssetsMessages.Get("ChatAvailabilityBusy", DescriviRisorsa(r), string.Join(", ", p.Select(x => $"{Ora(x.Inizio)}-{Ora(x.Fine)}")))
                    : AssetsMessages.Get("ChatAvailabilityFree", DescriviRisorsa(r))), 25));
        }
    }

    // R3 — scadenze della flotta: bollo, assicurazione, revisione, tagliando, contratto di noleggio/leasing
    public class AstFleetDeadlinesHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 30;
        private const int MaxVeicoli = 200;

        private readonly IAst_FlottaService _service;
        private readonly IntentDefinition _intentDefinition;

        public AstFleetDeadlinesHandler(IAst_FlottaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstFleetDeadlines",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryResources"),
                CategoryOrder = CategoryOrderRisorse,
                DisplayName = AssetsMessages.Get("ChatFleetDeadlinesDisplayName"),
                Description = AssetsMessages.Get("ChatFleetDeadlinesDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "scadenze flotta", "scadenze veicoli", "scadenze auto", "bollo", "revisione", "tagliando", "assicurazione auto", "fleet deadlines", "vehicle deadlines", "road tax", "car service" },
                Examples = new() { "quali sono le scadenze della flotta?", "quando scade il bollo della Panda?", "revisioni dei veicoli nei prossimi 60 giorni", "ci sono tagliandi scaduti?", "fleet deadlines this month" },
                RequiresConfirmation = false,
                Pages = new() { "/astvehicles", "/astvehicleedit" },
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "deadlineType",
                        Type = "string",
                        Required = false,
                        PromptDescription = "tipo di scadenza se l'utente lo dice: bollo, assicurazione, revisione, tagliando o contratto (noleggio/leasing), altrimenti ometti",
                        Question = AssetsMessages.Get("ChatDeadlineTypeQuestion"),
                        Label = AssetsMessages.Get("ChatDeadlineTypeLabel"),
                        Normalizer = v => TipoScadenza(v),
                        Validator = v => TipoScadenza(v) != null
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AssetsMessages.Get("ChatDeadlineTypeQuestion"), "deadlineType")
                    },
                    new SlotDefinition
                    {
                        Name = "vehicle",
                        Type = "string",
                        Required = false,
                        PromptDescription = "veicolo se l'utente lo indica: targa, marca o modello (es. 'della Panda' -> 'Panda'), altrimenti ometti",
                        Question = AssetsMessages.Get("ChatVehicleQuestion"),
                        Label = AssetsMessages.Get("ChatVehicleLabel")
                    },
                    new SlotDefinition
                    {
                        Name = "days",
                        Type = "int",
                        Required = false,
                        Default = GiorniPredefiniti.ToString(CultureInfo.InvariantCulture),
                        PromptDescription = "entro quanti giorni (es. 'nei prossimi 60 giorni' -> '60', 'entro un mese' -> 'un mese'); copia l'espressione, ometti se non detto",
                        Question = AssetsMessages.Get("ChatDaysQuestion"),
                        Label = AssetsMessages.Get("ChatDaysLabel"),
                        Normalizer = NormalizzaGiorni,
                        Validator = v => int.TryParse(v, out var g) && g > 0 && g <= 3650
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AssetsMessages.Get("ChatDaysQuestion"), "days")
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        private static readonly (string Chiave, AstTipoScadenzaVeicolo? Tipo, string[] Parole)[] _tipi =
        {
            ("bollo", AstTipoScadenzaVeicolo.Bollo, new[] { "bollo", "bolli", "tassa di possesso", "tassa automobilistica", "road tax", "vehicle tax" }),
            ("assicurazione", AstTipoScadenzaVeicolo.Assicurazione, new[] { "assicurazione", "assicurazioni", "rca", "polizza", "polizze", "insurance" }),
            ("revisione", AstTipoScadenzaVeicolo.Revisione, new[] { "revisione", "revisioni", "inspection", "inspections", "mot" }),
            ("tagliando", AstTipoScadenzaVeicolo.Tagliando, new[] { "tagliando", "tagliandi", "manutenzione", "manutenzioni", "service", "servicing" }),
            ("contratto", null, new[] { "contratto", "contratti", "noleggio", "leasing", "lease", "rental", "contract", "contracts" }),
        };

        // chiave canonica del tipo di scadenza ("assicurazioni" -> "assicurazione"), null se non riconosciuto
        public static string? TipoScadenza(string? valore)
        {
            var n = $" {ChatAIKeywords.Normalize(valore)} ";
            return _tipi.FirstOrDefault(t => t.Parole.Any(p => n.Contains($" {p} "))).Chiave;
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var oggi = DateTime.Today;
            var giorni = int.TryParse(slots.GetValueOrDefault("days"), out var g) && g > 0 ? g : GiorniPredefiniti;
            var limite = oggi.AddDays(giorni);
            var chiaveTipo = TipoScadenza(slots.GetValueOrDefault("deadlineType"));
            var tipo = _tipi.FirstOrDefault(t => t.Chiave == chiaveTipo);

            var lista = await _service.Ast_VeicoloList(new GenericRequest<Ast_VeicoloListInModel>(new Ast_VeicoloListInModel { Dismessi = false }), false);
            if (!lista.Success || lista.Data == null) return Fallito(lista);

            var veicoli = lista.Data.Veicolo;
            var nomeVeicolo = slots.GetValueOrDefault("vehicle");
            if (!string.IsNullOrWhiteSpace(nomeVeicolo))
            {
                var targa = Ast_FlottaService.NormalizzaTarga(nomeVeicolo);
                var perTarga = veicoli.Where(v => v.Targa == targa).ToList();
                veicoli = perTarga.Count > 0 ? perTarga : Trova(veicoli, nomeVeicolo, v => v.Descrizione);
                if (veicoli.Count == 0)
                    return CommandResult.Fail(AssetsMessages.Get("ChatVehicleNotFound", nomeVeicolo));
            }

            // (scadenza, descrizione, veicolo, km)
            var righe = new List<(DateTime Data, string Testo)>();
            foreach (var v in veicoli.Take(MaxVeicoli))
            {
                var det = await _service.Ast_VeicoloGet(new GenericRequest<Ast_IdInModel>(new Ast_IdInModel { Id = v.Id }), false);
                if (!det.Success || det.Data == null) return Fallito(det);

                if (chiaveTipo == null || tipo.Tipo != null)
                    foreach (var s in det.Data.Scadenza.Where(s => s.DataEsecuzione == null && s.DataScadenza.Date <= limite
                                                                   && (tipo.Tipo == null || s.Tipo == (int)tipo.Tipo)))
                    {
                        var descr = s.Tipo == (int)AstTipoScadenzaVeicolo.Altro && !string.IsNullOrWhiteSpace(s.Descrizione)
                            ? s.Descrizione! : AssetsMessages.Get("VehicleDeadlineType_" + s.Tipo);
                        var km = s.Km != null ? " " + AssetsMessages.Get("ChatDeadlineKm", s.Km) : string.Empty;
                        righe.Add((s.DataScadenza.Date, $"{Data(s.DataScadenza)} {descr} — {v.Descrizione}{km}"));
                    }

                var contratto = det.Data.Veicolo.ScadenzaContratto;
                if ((chiaveTipo == null || chiaveTipo == "contratto") && contratto != null && contratto.Value.Date <= limite)
                    righe.Add((contratto.Value.Date, $"{Data(contratto.Value)} " +
                        AssetsMessages.Get("ChatDeadlineContract", AssetsMessages.Get("VehicleOwnership_" + det.Data.Veicolo.Possesso)) + $" — {v.Descrizione}"));
            }

            var filtro = new List<string>();
            if (chiaveTipo != null) filtro.Add(chiaveTipo);
            if (!string.IsNullOrWhiteSpace(nomeVeicolo)) filtro.Add(veicoli.Count == 1 ? veicoli[0].Descrizione : nomeVeicolo!);
            var testoFiltro = filtro.Count > 0 ? $" ({string.Join(", ", filtro)})" : string.Empty;

            if (righe.Count == 0)
                return CommandResult.Ok(AssetsMessages.Get("ChatFleetDeadlinesNone", giorni, testoFiltro));

            var scadute = righe.Where(r => r.Data < oggi).OrderBy(r => r.Data).ToList();
            var prossime = righe.Where(r => r.Data >= oggi).OrderBy(r => r.Data).ToList();
            var sb = new StringBuilder();
            if (scadute.Count > 0)
                sb.AppendLine(Elenco(AssetsMessages.Get("ChatFleetDeadlinesOverdue", scadute.Count, testoFiltro), scadute.Select(r => r.Testo)));
            if (prossime.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine(Elenco(AssetsMessages.Get("ChatFleetDeadlinesTitle", prossime.Count, giorni, testoFiltro), prossime.Select(r => r.Testo)));
            }
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }

    // R4 — beni e veicolo in dotazione: i miei (tutti) o di un dipendente (amministratori)
    public class AstAssignedGoodsHandler : BaseCommandHandler
    {
        private readonly IAst_BeniService _beniService;
        private readonly IAst_FlottaService _flottaService;
        private readonly IChatUserContext _chatUserContext;
        private readonly IntentDefinition _intentDefinition;

        public AstAssignedGoodsHandler(IAst_BeniService beniService, IAst_FlottaService flottaService, IChatUserContext chatUserContext)
        {
            _beniService = beniService;
            _flottaService = flottaService;
            _chatUserContext = chatUserContext;
            _intentDefinition = new IntentDefinition
            {
                Name = "AstAssignedGoods",
                Application = ApplicationType.Assets,
                Category = AssetsMessages.Get("ChatCategoryResources"),
                CategoryOrder = CategoryOrderRisorse,
                DisplayName = AssetsMessages.Get("ChatAssignedGoodsDisplayName"),
                Description = AssetsMessages.Get("ChatAssignedGoodsDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "in dotazione", "dotazione", "beni assegnati", "i miei beni", "che beni ho", "cespiti assegnati", "auto assegnata", "my assets", "assigned assets", "company assets" },
                Examples = new() { "che beni ho in dotazione?", "cosa ha in dotazione Rossi?", "quali beni sono assegnati a Bianchi?", "which company assets do I have?" },
                RequiresConfirmation = false,
                Pages = new() { "/astmine", "/astgoods" },
                Slots = BuildSlots(new SlotDefinition
                {
                    Name = "person",
                    Type = "string",
                    Required = false,
                    IsPerson = true,
                    PromptDescription = "dipendente di cui si chiedono i beni (es. 'di Rossi' -> 'Rossi'); se l'utente chiede dei propri ('che beni ho') restituisci " + ChatSelf.Token,
                    Question = AssetsMessages.Get("ChatPersonQuestion"),
                    Label = AssetsMessages.Get("ChatPersonLabel")
                })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var persona = slots.GetValueOrDefault("person");
            if (string.IsNullOrWhiteSpace(persona) || persona.Equals(ChatSelf.Token, StringComparison.OrdinalIgnoreCase))
                return await MieiAsync();

            if (!_chatUserContext.IsInRole(RuoliAmministratori.ToArray()))
                return CommandResult.Fail(AssetsMessages.Get("ChatAssignedGoodsOthersNotAllowed"));

            var beni = await _beniService.Ast_BeneList(new GenericRequest<Ast_BeneListInModel>(new Ast_BeneListInModel()), false);
            if (!beni.Success || beni.Data == null) return Fallito(beni);
            var veicoli = await _flottaService.Ast_VeicoloList(new GenericRequest<Ast_VeicoloListInModel>(new Ast_VeicoloListInModel()), false);
            if (!veicoli.Success || veicoli.Data == null) return Fallito(veicoli);

            var nomi = beni.Data.Bene.Select(b => b.Assegnatario).Concat(veicoli.Data.Veicolo.Select(v => v.Assegnatario))
                .Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!).Distinct().ToList();
            var trovati = Trova(nomi, persona, n => n);
            if (trovati.Count == 0)
                return CommandResult.Ok(AssetsMessages.Get("ChatAssignedGoodsNoneFor", persona));
            if (trovati.Count > 1)
                return CommandResult.Fail(AssetsMessages.Get("ChatPersonAmbiguous", trovati.Count, persona, string.Join(", ", trovati.Take(8))));

            var nome = trovati[0];
            var righe = beni.Data.Bene.Where(b => b.Assegnatario == nome).OrderBy(b => b.Descrizione)
                .Select(b => RigaBene(b.Descrizione, b.CodiceInventario, b.DataConsegna, null))
                .Concat(veicoli.Data.Veicolo.Where(v => v.Assegnatario == nome).Select(v => AssetsMessages.Get("ChatAssignedVehicle", v.Descrizione)))
                .ToList();
            return CommandResult.Ok(Elenco(AssetsMessages.Get("ChatAssignedGoodsTitleFor", nome, righe.Count), righe, 25));
        }

        private async Task<CommandResult> MieiAsync()
        {
            var beni = await _beniService.Ast_MieiBeniGet(new GenericRequest<Ast_EmptyInModel>(new Ast_EmptyInModel()), false);
            if (!beni.Success || beni.Data == null) return Fallito(beni);
            var veicoli = await _flottaService.Ast_MieiVeicoliGet(new GenericRequest<Ast_EmptyInModel>(new Ast_EmptyInModel()), false);
            if (!veicoli.Success || veicoli.Data == null) return Fallito(veicoli);

            var oggi = DateTime.Today;
            var righe = beni.Data.Assegnazione.Where(a => a.DataRestituzione == null).OrderBy(a => a.Bene)
                .Select(a => RigaBene(a.Bene, a.CodiceInventario, a.DataConsegna, a.VerbaleConsegna))
                .Concat(veicoli.Data.Veicolo.Where(v => v.DataInizio.Date <= oggi && (v.DataFine == null || v.DataFine.Value.Date >= oggi))
                    .Select(v => AssetsMessages.Get("ChatAssignedVehicle", v.Descrizione)))
                .ToList();
            if (righe.Count == 0)
                return CommandResult.Ok(AssetsMessages.Get("ChatAssignedGoodsNoneMine"));
            return CommandResult.Ok(Elenco(AssetsMessages.Get("ChatAssignedGoodsTitleMine", righe.Count), righe, 25));
        }

        // "Notebook Dell (INV-001) dal 01/02/2026 — verbale da confermare"
        private static string RigaBene(string? bene, string? codice, DateTime? consegna, Ast_VerbaleModel? verbale)
        {
            var testo = string.IsNullOrWhiteSpace(codice) ? bene ?? string.Empty : $"{bene} ({codice})";
            if (consegna != null) testo += " " + AssetsMessages.Get("ChatAssignedSince", Data(consegna.Value));
            if (verbale is { Richiesto: true, DataConferma: null }) testo += " — " + AssetsMessages.Get("ChatAssignedToConfirm");
            return testo;
        }
    }
}
