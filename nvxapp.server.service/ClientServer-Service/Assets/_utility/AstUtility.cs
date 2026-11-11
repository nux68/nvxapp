using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Chi sta lavorando in Risorse aziendali (azienda e ruoli), ANALISI_ASSETS_APPLICATIVO.md:
        - CompanyPowerAdmin: configurazione del modulo e tutto il resto
        - CompanyAdmin     : legge la configurazione (le funzioni arrivano con le aree del modulo)
        - User             : nessuna funzione per ora
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class AstContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }

        public bool Amministratore => PowerAdmin || Admin;
    }

    public interface IAstUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.</summary>
        Task<AstContesto> ContestoAsync();

        /// <summary>Configurazione predefinita dell'azienda (se manca).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        Task<Ast_Cfg> CfgAsync(AstContesto ctx);
    }

    public class AstUtility : IAstUtility
    {
        private readonly IAst_CfgRepository _Ast_Cfg;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        private AstContesto? _contesto;

        public AstUtility(IAst_CfgRepository ast_CfgRepository, ICommonCompanyUtility commonCompanyUtility,
                          IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _Ast_Cfg = ast_CfgRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<AstContesto> ContestoAsync()
        {
            if (_contesto != null)
                return _contesto;

            var user = _httpContextAccessor.HttpContext?.User;
            var idUtente = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (!int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            var appUser = string.IsNullOrEmpty(idUtente) ? null : await _userManager.FindByIdAsync(idUtente);
            var ruoli = appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            bool utente = ruoli.Contains(nameof(RoleCode.User));
            if (!powerAdmin && !admin && !utente)
                throw new UserMessageException(AssetsMessages.Get("NotAllowed"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);

            _contesto = new AstContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                PowerAdmin = powerAdmin,
                Admin = admin,
                User = utente,
            };
            return _contesto;
        }

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _Ast_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;
            await _Ast_Cfg.CreateAsync(new Ast_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = DateTime.Now });
        }

        public Task<Ast_Cfg> CfgAsync(AstContesto ctx) => _Ast_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
    }
}
