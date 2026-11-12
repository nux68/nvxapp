using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources.ChatHrHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources
{
    // Scritture di Risorse Umane dalla chat: riepilogo e conferma (anche a voce) prima di eseguire. Niente cancellazioni.
    // ANALISI_HUMANRESOURCES_VOCALI.md §3.2

    // W1 — segna come completata una voce di una checklist aperta del dipendente (come la spunta nella pagina della checklist)
    public class HrCompleteChecklistItemHandler : BaseCommandHandler
    {
        private readonly IHr_OnboardingService _onboardingService;
        private readonly IntentDefinition _intentDefinition;

        public HrCompleteChecklistItemHandler(IHr_OnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrCompleteChecklistItem",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatCompleteItemDisplayName"),
                Description = HumanResourcesMessages.Get("ChatCompleteItemDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "la voce", "della checklist", "checklist di", "onboarding", "offboarding", "fatta", "fatto", "completata", "completato",
                    "completa la", "consegnato", "consegnata", "mark the item", "as done", "completed"
                },
                Examples = new()
                {
                    "segna come fatta la voce consegna badge della checklist di Rossi", "per Bianchi la visita medica è fatta",
                    "completa la voce dotazione pc di Verdi", "mark the item laptop delivered as done for Rossi"
                },
                SummaryTemplate = HumanResourcesMessages.Get("ChatCompleteItemSummary"),
                Pages = new() { "/hrchecklistedit", "/hrchecklistlist" },
                Slots = BuildSlots(
                    SlotDipendente(true),
                    SlotTesto("item", true,
                        "voce della checklist come detta dall'utente (es. 'consegna badge', 'visita medica', 'dotazione pc'), senza 'la voce' e senza il nome del dipendente",
                        HumanResourcesMessages.Get("ChatItemQuestion"), HumanResourcesMessages.Get("ChatItemLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var lista = await _onboardingService.Hr_ChecklistList(new GenericRequest<Hr_ChecklistListInModel>(new Hr_ChecklistListInModel { SoloAperte = true }), false);
            if (!lista.Success || lista.Data == null) return Fallito(lista);

            var nome = slots.GetValueOrDefault("employeeName");
            var (dip, errore) = UnNome(lista.Data.Checklist.Select(c => c.Dipendente), nome, "ChatOnboardingNoneEmployee");
            if (dip == null) return CommandResult.Fail(errore!);

            // voci da completare di tutte le checklist aperte del dipendente
            var voci = new List<(Hr_ChecklistModel Checklist, Hr_ChecklistVoceModel Voce)>();
            foreach (var c in lista.Data.Checklist.Where(c => string.Equals(c.Dipendente, dip, StringComparison.OrdinalIgnoreCase)))
            {
                var det = await _onboardingService.Hr_ChecklistGet(new GenericRequest<Hr_IdInModel>(new Hr_IdInModel { Id = c.Id }), false);
                if (!det.Success || det.Data == null) return Fallito(det);
                voci.AddRange(det.Data.Voci.Where(v => !v.Completata).Select(v => (c, v)));
            }
            if (voci.Count == 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatCompleteItemNoneOpen", dip));

            var voce = slots.GetValueOrDefault("item");
            var trovate = Trova(voci, voce, x => x.Voce.Descrizione);
            if (trovate.Count == 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatCompleteItemNotFound", voce, dip,
                    string.Join("; ", voci.Take(10).Select(x => x.Voce.Descrizione))));
            if (trovate.Count > 1)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatAmbiguous", trovate.Count, voce,
                    string.Join("; ", trovate.Take(8).Select(x => $"{x.Voce.Descrizione} ({TipoChecklist(x.Checklist.Tipo)})"))));

            var (checklist, v) = trovate[0];
            // le voci con un'azione sull'account si completano creando o disattivando l'account dalla pagina
            if (v.Azione != 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatCompleteItemAction", v.Descrizione));

            // si rimanda la voce com'è (responsabile, data prevista, note) con la sola spunta
            var res = await _onboardingService.Hr_ChecklistVocePut(new GenericRequest<Hr_ChecklistVocePutInModel>(new Hr_ChecklistVocePutInModel
            {
                Voce = new Hr_ChecklistVoceModel
                {
                    Id = v.Id,
                    Ordine = v.Ordine,
                    Descrizione = v.Descrizione,
                    IdAspNetUsersResponsabile = v.IdAspNetUsersResponsabile,
                    DataPrevista = v.DataPrevista,
                    Note = v.Note,
                    Documento = v.Documento,
                    Azione = v.Azione,
                    Completata = true
                }
            }), false);
            if (!res.Success) return Fallito(res);

            var rimaste = voci.Count(x => x.Checklist.Id == checklist.Id) - 1;
            return CommandResult.Ok(HumanResourcesMessages.Get("ChatCompleteItemDone", v.Descrizione, TipoChecklist(checklist.Tipo), dip, rimaste));
        }
    }

    // W2 — aggiunge un corso da fare per un dipendente al piano di formazione dell'anno
    public class HrAddPlanCourseHandler : BaseCommandHandler
    {
        private readonly IHr_FormazioneService _formazioneService;
        private readonly IntentDefinition _intentDefinition;

        public HrAddPlanCourseHandler(IHr_FormazioneService formazioneService)
        {
            _formazioneService = formazioneService;
            _intentDefinition = new IntentDefinition
            {
                Name = "HrAddPlanCourse",
                Application = ApplicationType.HumanResources,
                Category = HumanResourcesMessages.Get("ChatCategory"),
                CategoryOrder = CategoryOrder,
                DisplayName = HumanResourcesMessages.Get("ChatAddPlanDisplayName"),
                Description = HumanResourcesMessages.Get("ChatAddPlanDescription"),
                Usable4Role = RuoliUfficio,
                Keywords = new()
                {
                    "piano di formazione", "piano formazione", "nel piano", "al piano", "deve fare il corso", "devono fare il corso",
                    "training plan", "to the plan"
                },
                Examples = new()
                {
                    "aggiungi al piano di formazione il corso sicurezza per Rossi entro marzo", "Bianchi deve fare il corso primo soccorso entro il 30 giugno",
                    "metti nel piano 2027 il corso antincendio per Verdi", "add the first aid course for Rossi to the training plan"
                },
                SummaryTemplate = HumanResourcesMessages.Get("ChatAddPlanSummary"),
                Pages = new() { "/hrplanlist", "/hrplanedit", "/hrcourselist" },
                Slots = BuildSlots(
                    SlotCorso(true),
                    SlotDipendente(true),
                    SlotData("by", false, string.Empty, HumanResourcesMessages.Get("ChatByQuestion"), HumanResourcesMessages.Get("ChatByLabel"),
                        "data entro cui fare il corso se detta (es. 'entro marzo' -> 'fine marzo', 'entro il 30 giugno'); altrimenti ometti"),
                    SlotAnno(),
                    SlotTesto("plan", false, "nome del piano di formazione SOLO se l'utente lo dice (es. 'nel piano sicurezza'), altrimenti ometti",
                        HumanResourcesMessages.Get("ChatPlanQuestion"), HumanResourcesMessages.Get("ChatPlanLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var entro = string.IsNullOrWhiteSpace(slots.GetValueOrDefault("by")) ? (DateTime?)null : ParseData(slots["by"], DateTime.Today);
            var anno = int.TryParse(slots.GetValueOrDefault("year"), out var a) ? a : (entro?.Year ?? DateTime.Today.Year);

            var lista = await _formazioneService.Hr_PianoList(new GenericRequest<Hr_PianoListInModel>(new Hr_PianoListInModel { Anno = anno }), false);
            if (!lista.Success || lista.Data == null) return Fallito(lista);
            var piani = lista.Data.Piani;
            if (slots.TryGetValue("plan", out var nomePiano) && !string.IsNullOrWhiteSpace(nomePiano))
                piani = Trova(piani, nomePiano, p => p.Descrizione);
            if (piani.Count == 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatTrainingNoPlan", anno));
            if (piani.Count > 1)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatAddPlanWhichPlan", piani.Count, anno, string.Join(", ", piani.Select(p => p.Descrizione))));

            var get = await _formazioneService.Hr_PianoGet(new GenericRequest<Hr_IdInModel>(new Hr_IdInModel { Id = piani[0].Id }), false);
            if (!get.Success || get.Data == null) return Fallito(get);
            var piano = get.Data.Piano;

            var nomeCorso = slots.GetValueOrDefault("course");
            var corsi = Trova(get.Data.Corsi.Where(c => c.Attivo), nomeCorso, c => c.Titolo);
            if (corsi.Count == 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatCourseNotFound", nomeCorso));
            if (corsi.Count > 1)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatAmbiguous", corsi.Count, nomeCorso, string.Join(", ", corsi.Take(8).Select(c => c.Titolo))));
            var corso = corsi[0];

            var nomeDip = slots.GetValueOrDefault("employeeName");
            var dipendenti = Trova(get.Data.Dipendenti.Where(d => d.Attivo), nomeDip, d => NomeDipendente(d.Cognome, d.Nome));
            if (dipendenti.Count == 0)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatEmployeeNotFound", nomeDip));
            if (dipendenti.Count > 1)
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatAmbiguous", dipendenti.Count, nomeDip,
                    string.Join(", ", dipendenti.Take(8).Select(d => NomeDipendente(d.Cognome, d.Nome)))));
            var dip = dipendenti[0];
            var nome = NomeDipendente(dip.Cognome, dip.Nome);

            if (piano.Righe.Any(r => r.IdHr_Corso == corso.Id && r.IdDip_Anagrafica == dip.Id))
                return CommandResult.Fail(HumanResourcesMessages.Get("ChatAddPlanAlready", corso.Titolo, nome, piano.Descrizione));

            // il servizio riscrive le righe del piano: si rimandano tutte quelle esistenti (con l'id) più la nuova
            piano.Righe.Add(new Hr_PianoRigaModel { Id = 0, IdHr_Corso = corso.Id, IdDip_Anagrafica = dip.Id, EntroIl = entro });
            var put = await _formazioneService.Hr_PianoPut(new GenericRequest<Hr_PianoPutInModel>(new Hr_PianoPutInModel { Piano = piano }), false);
            if (!put.Success) return Fallito(put);

            var quando = entro.HasValue ? " " + HumanResourcesMessages.Get("ChatTrainingBy", Data(entro.Value)) : string.Empty;
            return CommandResult.Ok(HumanResourcesMessages.Get("ChatAddPlanDone", corso.Titolo, nome, piano.Descrizione,
                piano.Anno.ToString(CultureInfo.InvariantCulture), quando));
        }
    }
}
