using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources
{
    // Schede di Risorse Umane che la chat sa aprire con il comando unico OpenEntity ("apri la scheda hr di Rossi",
    // "apri il corso primo soccorso"). Le pagine di modifica del modulo leggono history.state.id.
    // Gli elenchi vengono dai servizi del modulo (solo ufficio del personale), quindi rispettano i permessi dell'utente.
    // "dipendente" resta l'entità delle presenze: qui "dipendente hr" / "scheda hr" (ANALISI_HUMANRESOURCES_VOCALI.md §3.3).
    public abstract class OpenableHrEntity : IOpenableEntity
    {
        private readonly string _labelKey;

        protected OpenableHrEntity(string key, string labelKey, string editRoute, string listRoute, params string[] words)
        {
            Key = key;
            _labelKey = labelKey;
            EditRoute = editRoute;
            Pages = new[] { listRoute, editRoute };
            Words = new[] { key }.Concat(words).ToArray();
        }

        public string Key { get; }
        public string DisplayName => HumanResourcesMessages.Get(_labelKey).ToLower(CultureInfo.CurrentUICulture);
        public IReadOnlyList<string> Words { get; }
        public IReadOnlyList<string> Roles { get; } = ChatHrHelper.RuoliUfficio;
        public string EditRoute { get; }
        public IReadOnlyList<string> Pages { get; }
        public ApplicationType? Application => ApplicationType.HumanResources;

        public abstract Task<List<OpenableItem>> LoadAsync();

        // elenco del servizio (vuoto se la lettura non riesce)
        protected static List<OpenableItem> Elementi<T>(GenericResult<T> result, Func<T, IEnumerable<OpenableItem>?> elementi) =>
            result.Success && result.Data != null ? elementi(result.Data)?.ToList() ?? new() : new();
    }

    public class OpenableHrDipendente : OpenableHrEntity
    {
        private readonly IHr_DipendenteService _service;
        public OpenableHrDipendente(IHr_DipendenteService service)
            : base("dipendente hr", "ChatOpenEmployeeLabel", "/hremployeeedit", "/hremployeelist",
                   "scheda hr", "anagrafica hr", "dipendenti hr", "scheda risorse umane", "hr employee", "hr record", "employee hr record") => _service = service;

        // matricola come codice: "apri la scheda hr della matricola 125"
        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Hr_DipendenteList(new GenericRequest<Hr_DipendenteListInModel>(new Hr_DipendenteListInModel { SoloAttivi = false }), false),
                d => d.Dipendenti.Select(x => new OpenableItem(x.Id, ChatHrHelper.NomeDipendente(x.Cognome, x.Nome), x.Matricola)));
    }

    public class OpenableHrCorso : OpenableHrEntity
    {
        private readonly IHr_FormazioneService _service;
        public OpenableHrCorso(IHr_FormazioneService service)
            : base("corso", "ChatOpenCourseLabel", "/hrcourseedit", "/hrcourselist",
                   "corsi", "corso di formazione", "course", "training course") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Hr_CorsoList(new GenericRequest<Hr_EmptyInModel>(new Hr_EmptyInModel()), false),
                d => d.Corsi.Select(x => new OpenableItem(x.Id, x.Titolo, x.Ente)));
    }

    public class OpenableHrPiano : OpenableHrEntity
    {
        private readonly IHr_FormazioneService _service;
        public OpenableHrPiano(IHr_FormazioneService service)
            : base("piano di formazione", "ChatOpenPlanLabel", "/hrplanedit", "/hrplanlist",
                   "piano formazione", "piani di formazione", "training plan") => _service = service;

        // anno come codice: "apri il piano di formazione 2026"
        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Hr_PianoList(new GenericRequest<Hr_PianoListInModel>(new Hr_PianoListInModel()), false),
                d => d.Piani.Select(x => new OpenableItem(x.Id, $"{x.Descrizione} {x.Anno}", x.Anno.ToString(CultureInfo.InvariantCulture))));
    }

    public class OpenableHrChecklist : OpenableHrEntity
    {
        private readonly IHr_OnboardingService _service;
        public OpenableHrChecklist(IHr_OnboardingService service)
            : base("checklist hr", "ChatOpenChecklistLabel", "/hrchecklistedit", "/hrchecklistlist",
                   "checklist onboarding", "checklist di assunzione", "checklist di cessazione", "onboarding", "offboarding",
                   "hr checklist", "onboarding checklist") => _service = service;

        // il dipendente come codice: "apri la checklist di onboarding di Rossi"
        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Hr_ChecklistList(new GenericRequest<Hr_ChecklistListInModel>(new Hr_ChecklistListInModel { SoloAperte = false }), false),
                d => d.Checklist.Select(x => new OpenableItem(x.Id, $"{x.Dipendente} {x.Descrizione}".Trim(), x.Dipendente)));
    }
}
