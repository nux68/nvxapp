using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    /*
     Chi sta lavorando in Compliance (azienda e ruoli), ANALISI_COMPLIANCE_APPLICATIVO.md:
        - CompanyPowerAdmin: configurazione del modulo e tutto il resto
        - CompanyAdmin     : legge la configurazione (le funzioni arrivano con le aree del modulo)
        - User             : nessuna funzione per ora
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class CmpContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }

        public bool Amministratore => PowerAdmin || Admin;
    }

    public interface ICmpUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.</summary>
        Task<CmpContesto> ContestoAsync();

        /// <summary>Configurazione predefinita dell'azienda (se manca).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        Task<Cmp_Cfg> CfgAsync(CmpContesto ctx);
    }

    public class CmpUtility : ICmpUtility
    {
        private readonly ICmp_CfgRepository _Cmp_Cfg;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        private CmpContesto? _contesto;

        public CmpUtility(ICmp_CfgRepository cmp_CfgRepository, ICommonCompanyUtility commonCompanyUtility,
                          IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _Cmp_Cfg = cmp_CfgRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<CmpContesto> ContestoAsync()
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
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);

            _contesto = new CmpContesto
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
            if (await _Cmp_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;
            await _Cmp_Cfg.CreateAsync(new Cmp_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = DateTime.Now });
        }

        public Task<Cmp_Cfg> CfgAsync(CmpContesto ctx) => _Cmp_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
    }
}
