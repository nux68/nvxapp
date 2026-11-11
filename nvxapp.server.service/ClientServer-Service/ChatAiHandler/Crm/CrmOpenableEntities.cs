using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm
{
    // Schede del CRM che la chat sa aprire con il comando unico OpenEntity ("apri la scheda cliente Rossi",
    // "apri l'offerta 12/2026"). Le pagine di modifica del CRM leggono history.state.id.
    // Gli elenchi vengono dai servizi del CRM, quindi rispettano la visibilità dell'utente.
    // La chiave del cliente è "cliente crm": "cliente" resta quello delle presenze (ANALISI_CRM_VOCALI.md §3.3).
    public abstract class OpenableCrmEntity : IOpenableEntity
    {
        private readonly string _labelKey;

        protected OpenableCrmEntity(string key, string labelKey, string editRoute, string listRoute, params string[] words)
        {
            Key = key;
            _labelKey = labelKey;
            EditRoute = editRoute;
            Pages = new[] { listRoute, editRoute };
            Words = new[] { key }.Concat(words).ToArray();
        }

        public string Key { get; }
        public string DisplayName => CrmMessages.Get(_labelKey).ToLower(CultureInfo.CurrentUICulture);
        public IReadOnlyList<string> Words { get; }
        public IReadOnlyList<string> Roles { get; } = ChatCrmHelper.Ruoli;
        public string EditRoute { get; }
        public IReadOnlyList<string> Pages { get; }
        public ApplicationType? Application => ApplicationType.Crm;

        public abstract Task<List<OpenableItem>> LoadAsync();

        // elenco del servizio (vuoto se la lettura non riesce)
        protected static List<OpenableItem> Elementi<T>(GenericResult<T> result, Func<T, IEnumerable<OpenableItem>?> elementi) =>
            result.Success && result.Data != null ? elementi(result.Data)?.ToList() ?? new() : new();
    }

    public class OpenableCrmCliente : OpenableCrmEntity
    {
        private readonly ICrm_ClienteService _service;
        public OpenableCrmCliente(ICrm_ClienteService service)
            : base("cliente crm", "ChatOpenCustomerLabel", "/crmcustomeredit", "/crmcustomerlist",
                   "scheda cliente", "clienti crm", "cliente", "crm customer", "customer card", "customer") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Crm_ClienteList(new GenericRequest<Crm_ClienteListInModel>(new Crm_ClienteListInModel()), false),
                d => d.Clienti.Select(x => new OpenableItem(x.Id, x.Descrizione)));
    }

    public class OpenableCrmOpportunita : OpenableCrmEntity
    {
        private readonly ICrm_OpportunitaService _service;
        public OpenableCrmOpportunita(ICrm_OpportunitaService service)
            : base("opportunità", "ChatOpenOpportunityLabel", "/crmopportunityedit", "/crmpipeline",
                   "opportunita", "trattativa", "opportunity", "deal") => _service = service;

        // il cliente come codice: "apri l'opportunità di Rossi"
        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Crm_OpportunitaList(new GenericRequest<Crm_OpportunitaListInModel>(new Crm_OpportunitaListInModel { AncheChiuse = true }), false),
                d => d.Opportunita.Select(x => new OpenableItem(x.Id, x.Titolo, x.Cliente)));
    }

    public class OpenableCrmLead : OpenableCrmEntity
    {
        private readonly ICrm_LeadService _service;
        public OpenableCrmLead(ICrm_LeadService service)
            : base("lead", "ChatOpenLeadLabel", "/crmleadedit", "/crmleadlist",
                   "leads", "potenziale cliente", "prospect") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Crm_LeadList(new GenericRequest<Crm_LeadListInModel>(new Crm_LeadListInModel { AncheChiusi = true }), false),
                d => d.Lead.Select(x => new OpenableItem(x.Id, $"{x.Cognome} {x.Nome}".Trim() is { Length: > 0 } n ? n : x.Azienda ?? string.Empty, x.Azienda)));
    }

    public class OpenableCrmOfferta : OpenableCrmEntity
    {
        private readonly ICrm_OffertaService _service;
        public OpenableCrmOfferta(ICrm_OffertaService service)
            : base("offerta", "ChatOpenOfferLabel", "/crmofferedit", "/crmofferlist",
                   "offerte", "preventivo", "offer", "quote", "quotation") => _service = service;

        // numero ("12/2026") come codice; nella descrizione oggetto e cliente
        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Crm_OffertaList(new GenericRequest<Crm_OffertaListInModel>(new Crm_OffertaListInModel()), false),
                d => d.Offerte.Select(x => new OpenableItem(x.Id, $"{x.NumeroCompleto} {x.Oggetto} {x.Cliente}".Trim(), x.NumeroCompleto)));
    }
}
