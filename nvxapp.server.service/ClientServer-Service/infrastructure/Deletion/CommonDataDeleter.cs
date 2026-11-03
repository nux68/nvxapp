using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion
{
    /*
     Dati comuni dell'azienda (schema Common: Az_Anagrafica, Az_Cliente, Az_ClienteContatto).
     DeletionService lo usa per ogni azienda (non dipende da CompanyApplication) e sempre DOPO i deleter
     degli applicativi, che hanno chiavi esterne verso queste tabelle.
     In multi-tenant lo schema tenant_<IdAzienda>_0 si elimina con gli altri: qui solo i conteggi.
    */
    public class CommonDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancySettings;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly ITenantProvisioningService _tenantProvisioningService;

        public CommonDataDeleter(TenancySettings tenancySettings,
                                 IApplicationDbContextFactory applicationDbContextFactory,
                                 ITenantProvisioningService tenantProvisioningService)
        {
            _tenancySettings = tenancySettings;
            _applicationDbContextFactory = applicationDbContextFactory;
            _tenantProvisioningService = tenantProvisioningService;
        }

        public ApplicationType Application => ApplicationType.Common;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var context = await ContextAsync(idCompany, cancellationToken);
            if (context == null)
                return new List<DeletionCountModel>();

            var clienti = await context.Az_Cliente.CountAsync(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany, cancellationToken);
            return new List<DeletionCountModel>
            {
                new DeletionCountModel { Label = InfrastructureMessages.Get("DeletionCountCustomers"), Count = clienti }
            };
        }

        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancySettings.MultiTenant)
                return;

            await using var context = await ContextAsync(idCompany, cancellationToken);
            if (context == null)
                return;

            // clienti e contatti in cascata
            await context.Az_Anagrafica.Where(x => x.IdCompany == idCompany).ExecuteDeleteAsync(cancellationToken);
        }

        // i dati comuni non sono legati agli utenti
        public Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        // contesto sullo schema Common dell'azienda; null se lo schema non esiste (multi-tenant)
        private async Task<CommonDbContext?> ContextAsync(int idCompany, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (_tenancySettings.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.Common);
                if (!(await _tenantProvisioningService.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (CommonDbContext)_applicationDbContextFactory.Create(ApplicationType.Common, schema);
        }
    }
}
