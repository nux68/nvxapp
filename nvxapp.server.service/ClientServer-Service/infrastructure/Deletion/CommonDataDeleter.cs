using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion
{
    /*
     Dati comuni dell'azienda (schema Common: Az_Anagrafica, Az_Cliente, Az_ClienteContatto, Dip_Anagrafica).
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
            var dipendenti = await context.Dip_Anagrafica.CountAsync(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany, cancellationToken);
            return new List<DeletionCountModel>
            {
                new DeletionCountModel { Label = InfrastructureMessages.Get("DeletionCountCustomers"), Count = clienti },
                new DeletionCountModel { Label = InfrastructureMessages.Get("DeletionCountEmployees"), Count = dipendenti }
            };
        }

        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancySettings.MultiTenant)
                return;

            await using var context = await ContextAsync(idCompany, cancellationToken);
            if (context == null)
                return;

            // clienti, contatti e dipendenti in cascata
            await context.Az_Anagrafica.Where(x => x.IdCompany == idCompany).ExecuteDeleteAsync(cancellationToken);
        }

        /*
         Utente tolto dall'azienda (o eliminato): il dipendente resta, per la conservazione dei suoi documenti,
         ma viene scollegato dall'utente e segnato come cessato (ANALISI_DOCUMENTALE.md §2.1).
         Se l'utente viene eliminato, la chiave esterna (SetNull) lo scollega comunque in tutti gli schemi.
        */
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var context = await ContextAsync(idCompany, cancellationToken);
            if (context == null)
                return;

            var oggi = DateTime.Today;
            await context.Dip_Anagrafica
                         .Where(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany && x.IdAspNetUsers == idAspNetUsers)
                         .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsers, (string?)null)
                                                   .SetProperty(x => x.Attivo, false)
                                                   .SetProperty(x => x.DataCessazione, x => x.DataCessazione ?? oggi)
                                                   .SetProperty(x => x.ModifiedDate, DateTime.Now),
                                             cancellationToken);
        }

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
