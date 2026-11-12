using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets
{
    // Schede di Risorse aziendali che la chat sa aprire con il comando unico OpenEntity ("apri il veicolo AB123CD",
    // "apri il bene INV-001"). Le pagine di modifica leggono history.state.id. Elenchi dai servizi del modulo
    // (solo amministratori, come il menu "Veicoli" e "Beni"). ANALISI_ASSETS_VOCALI.md §3.3
    public abstract class OpenableAstEntity : IOpenableEntity
    {
        private readonly string _labelKey;

        protected OpenableAstEntity(string key, string labelKey, string editRoute, string listRoute, params string[] words)
        {
            Key = key;
            _labelKey = labelKey;
            EditRoute = editRoute;
            Pages = new[] { listRoute, editRoute };
            Words = new[] { key }.Concat(words).ToArray();
        }

        public string Key { get; }
        public string DisplayName => AssetsMessages.Get(_labelKey).ToLower(CultureInfo.CurrentUICulture);
        public IReadOnlyList<string> Words { get; }
        public IReadOnlyList<string> Roles { get; } = ChatAstHelper.RuoliAmministratori;
        public string EditRoute { get; }
        public IReadOnlyList<string> Pages { get; }
        public ApplicationType? Application => ApplicationType.Assets;

        public abstract Task<List<OpenableItem>> LoadAsync();

        protected static List<OpenableItem> Elementi<T>(GenericResult<T> result, Func<T, IEnumerable<OpenableItem>?> elementi) =>
            result.Success && result.Data != null ? elementi(result.Data)?.ToList() ?? new() : new();
    }

    // veicolo della flotta: targa come codice ("apri il veicolo AB123CD", "apri il veicolo Panda")
    public class OpenableAstVeicolo : OpenableAstEntity
    {
        private readonly IAst_FlottaService _service;
        public OpenableAstVeicolo(IAst_FlottaService service)
            : base("veicolo", "ChatOpenVehicleLabel", "/astvehicleedit", "/astvehicles",
                   "veicoli", "automezzo", "auto aziendale", "macchina aziendale", "targa", "vehicle", "company car", "plate") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Ast_VeicoloList(new GenericRequest<Ast_VeicoloListInModel>(new Ast_VeicoloListInModel { Dismessi = true }), false),
                d => d.Veicolo.Select(x => new OpenableItem(x.Id, x.Descrizione, x.Targa)));
    }

    // bene assegnabile (PC, telefono, badge, attrezzatura): codice di inventario come codice
    public class OpenableAstBene : OpenableAstEntity
    {
        private readonly IAst_BeniService _service;
        public OpenableAstBene(IAst_BeniService service)
            : base("bene", "ChatOpenGoodLabel", "/astgoodedit", "/astgoods",
                   "beni", "cespite", "cespiti", "bene aziendale", "inventario", "asset", "company asset") => _service = service;

        public override async Task<List<OpenableItem>> LoadAsync() =>
            Elementi(await _service.Ast_BeneList(new GenericRequest<Ast_BeneListInModel>(new Ast_BeneListInModel { Dismessi = true }), false),
                d => d.Bene.Select(x => new OpenableItem(x.Id, $"{x.Descrizione} {x.Assegnatario}".Trim(), x.CodiceInventario)));
    }
}
