using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Agganci di Risorse aziendali ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (configurazione predefinita)
        - cancellazione fisica (IApplicationDataDeleter): dati dell'azienda in modalita' singola, utente rimosso
     Lo schema lo sceglie ogni classe (tenant_<IdAzienda>_7 o public), come per gli altri applicativi.
    */
    public static class AstSchema
    {
        public static async Task<AssetsDbContext?> ContextAsync(int idCompany, TenancySettings tenancy, IApplicationDbContextFactory factory,
                                                             ITenantProvisioningService provisioning, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (tenancy.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.Assets);
                if (!(await provisioning.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (AssetsDbContext)factory.Create(ApplicationType.Assets, schema);
        }
    }

    public class AstCompanyInitializer : ICompanyInitializer
    {
        private readonly IAstUtility _utility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ILogger<AstCompanyInitializer> _logger;

        public string Name => "Assets";
        public int Priority => 70;
        public ApplicationType? Application => ApplicationType.Assets;

        public AstCompanyInitializer(IAstUtility utility, ICommonCompanyUtility commonCompanyUtility, ILogger<AstCompanyInitializer> logger)
        {
            _utility = utility;
            _commonCompanyUtility = commonCompanyUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, true);
            await _utility.EnsureDefaultsAsync(az!.Id);
            _logger.LogInformation("[AstCompanyInitializer] Azienda {CompanyId}: Risorse aziendali inizializzato.", company.Id);
        }
    }

    public class AstDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public AstDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                              IServiceProvider sp, IHttpContextAccessor http)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
        }

        public ApplicationType Application => ApplicationType.Assets;

        // per ora solo la configurazione: i conteggi arrivano con le aree del modulo
        public Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<DeletionCountModel>());

        // multi-tenant: lo schema si elimina con gli altri (DeletionService)
        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancy.MultiTenant)
                return;
            await using var db = await AstSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var rep = new AstRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (idAz == 0)
                return;
            await rep.Ast_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        // nessun dato legato all'utente per ora
        public Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
