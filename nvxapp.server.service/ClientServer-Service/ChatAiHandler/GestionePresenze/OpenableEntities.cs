using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Anagrafiche delle presenze che la chat sa aprire con il comando unico OpenEntity
    // ("apri la causale ferie"). Per aggiungerne una basta una nuova classe come queste:
    // servizio, parole (italiano e inglese), pagina di modifica ed elenco.
    // Solo HR, come le pagine di configurazione.
    public abstract class OpenablePresenzeEntity : IOpenableEntity
    {
        private readonly string _labelKey;

        protected OpenablePresenzeEntity(string key, string labelKey, string editRoute, string listRoute, params string[] words)
        {
            Key = key;
            _labelKey = labelKey;
            EditRoute = editRoute;
            Pages = new[] { listRoute, editRoute };
            Words = new[] { key }.Concat(words).ToArray();
        }

        public string Key { get; }
        public string DisplayName => AttendanceTrackingMessages.Get(_labelKey).ToLower(CultureInfo.CurrentUICulture);
        public IReadOnlyList<string> Words { get; }
        public IReadOnlyList<string> Roles { get; } = new[] { ChatPresenzeHelper.RuoloHR };
        public string EditRoute { get; }
        public IReadOnlyList<string> Pages { get; }

        public abstract Task<List<OpenableItem>> LoadAsync();

        // elenco del servizio (vuoto se la lettura non riesce)
        protected static List<OpenableItem> Elementi<T>(GenericResult<T> result, Func<T, IEnumerable<OpenableItem>?> elementi) =>
            result.Success && result.Data != null ? elementi(result.Data)?.ToList() ?? new() : new();
    }

    public class OpenableCausale : OpenablePresenzeEntity
    {
        private readonly IPar_CausaliService _service;
        public OpenableCausale(IPar_CausaliService service)
            : base("causale", "ChatOpenCausaleLabel", "/causaliedit", "/causalilist", "causali", "attendance code", "pay code") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_CausaliInModel>(new Par_CausaliInModel()), false),
                d => d.Par_Causali?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty, x.Codice)));
    }

    public class OpenableGiustificativo : OpenablePresenzeEntity
    {
        private readonly IPar_GiustificativiService _service;
        public OpenableGiustificativo(IPar_GiustificativiService service)
            : base("giustificativo", "ChatOpenGiustificativoLabel", "/justificationedit", "/justificationlist", "giustificativi", "leave type", "absence type", "justification") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_GiustificativiInModel>(new Par_GiustificativiInModel()), false),
                d => d.Par_Giustificativi?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty, x.Codice)));
    }

    public class OpenableAttivita : OpenablePresenzeEntity
    {
        private readonly IPar_AttivitaService _service;
        public OpenableAttivita(IPar_AttivitaService service)
            : base("attività", "ChatOpenAttivitaLabel", "/activityedit", "/activitylist", "attivita", "activity", "activities") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_Attivita_GetAll_InModel>(new Par_Attivita_GetAll_InModel()), false),
                d => d.Par_Attivita?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty)));
    }

    public class OpenableCliente : OpenablePresenzeEntity
    {
        private readonly IAz_ClienteService _service;
        public OpenableCliente(IAz_ClienteService service)
            : base("cliente", "ChatOpenClienteLabel", "/customeredit", "/customerlist", "clienti", "customer", "client") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Az_Cliente_GetAll_InModel>(new Az_Cliente_GetAll_InModel()), false),
                d => d.Az_Cliente?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty)));
    }

    public class OpenableCommessa : OpenablePresenzeEntity
    {
        private readonly IAz_CommessaService _service;
        public OpenableCommessa(IAz_CommessaService service)
            : base("commessa", "ChatOpenCommessaLabel", "/commessaedit", "/commessalist", "commesse", "project", "job order") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Az_Commessa_GetAll_InModel>(new Az_Commessa_GetAll_InModel()), false),
                d => d.Az_Commessa?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty)));
    }

    public class OpenableCompetenza : OpenablePresenzeEntity
    {
        private readonly IPar_CompetenzaService _service;
        public OpenableCompetenza(IPar_CompetenzaService service)
            : base("competenza", "ChatOpenCompetenzaLabel", "/competenceedit", "/competencelist", "competenze", "skill", "competence") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_Competenza_GetAll_InModel>(new Par_Competenza_GetAll_InModel()), false),
                d => d.Par_Competenza?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty)));
    }

    public class OpenableModelloExport : OpenablePresenzeEntity
    {
        private readonly IPar_ExportCauService _service;
        public OpenableModelloExport(IPar_ExportCauService service)
            : base("modello export", "ChatOpenModelloExportLabel", "/exportcauedit", "/exportcaulist", "modelli export", "export template", "export layout") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_ExportCau_GetAll_InModel>(new Par_ExportCau_GetAll_InModel()), false),
                d => d.Par_ExportCau?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty, x.Codice)));
    }

    public class OpenableOrario : OpenablePresenzeEntity
    {
        private readonly IPar_OrarioService _service;
        public OpenableOrario(IPar_OrarioService service)
            : base("orario", "ChatOpenOrarioLabel", "/orariedit", "/orarilist", "orari", "work schedule", "working hours") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_Orario_GetAllInModel>(new Par_Orario_GetAllInModel()), false),
                d => d.Par_Orario?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty, x.Codice)));
    }

    public class OpenableProfiloOrario : OpenablePresenzeEntity
    {
        private readonly IPar_ProfiloOrarioService _service;
        public OpenableProfiloOrario(IPar_ProfiloOrarioService service)
            : base("profilo orario", "ChatOpenProfiloOrarioLabel", "/profiliorariedit", "/profiliorarilist", "profili orari", "schedule profile") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Par_ProfiloOrario_GetAllInModel>(new Par_ProfiloOrario_GetAllInModel()), false),
                d => d.Par_ProfiloOrario?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty, x.Codice)));
    }

    public class OpenableReparto : OpenablePresenzeEntity
    {
        private readonly IAz_SediRepartoService _service;
        public OpenableReparto(IAz_SediRepartoService service)
            : base("reparto", "ChatOpenRepartoLabel", "/departmentedit", "/departmentlist", "reparti", "department") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.GetAll(new GenericRequest<Az_SediReparto_GetAll_InModel>(new Az_SediReparto_GetAll_InModel()), false),
                d => d.Az_SediReparto?.Select(x => new OpenableItem(x.Id, x.Descrizione ?? string.Empty)));
    }
}
