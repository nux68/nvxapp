using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /*
     Agganci del CRM ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (impostazioni e tabelle predefinite)
        - cancellazione fisica (IApplicationDataDeleter): dati dell'azienda in modalita' singola, utente rimosso
        - clienti e contatti comuni usati dal CRM (ICommonEntityUsage): blocca con un messaggio chiaro la cancellazione
     Lo schema lo sceglie ogni classe (tenant_<IdAzienda>_3 o public), come per gli altri applicativi.
    */
    public static class CrmSchema
    {
        public static async Task<CrmDbContext?> ContextAsync(int idCompany, TenancySettings tenancy, IApplicationDbContextFactory factory,
                                                             ITenantProvisioningService provisioning, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (tenancy.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.Crm);
                if (!(await provisioning.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (CrmDbContext)factory.Create(ApplicationType.Crm, schema);
        }
    }

    public class CrmCompanyInitializer : ICompanyInitializer
    {
        private readonly ICrmUtility _utility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ILogger<CrmCompanyInitializer> _logger;

        public string Name => "Crm";
        public int Priority => 40;
        public ApplicationType? Application => ApplicationType.Crm;

        public CrmCompanyInitializer(ICrmUtility utility, ICommonCompanyUtility commonCompanyUtility, ILogger<CrmCompanyInitializer> logger)
        {
            _utility = utility;
            _commonCompanyUtility = commonCompanyUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, true);
            await _utility.EnsureDefaultsAsync(az!.Id);
            _logger.LogInformation("[CrmCompanyInitializer] Azienda {CompanyId}: CRM inizializzato.", company.Id);
        }
    }

    public class CrmDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IFileStorage _fileStorage;

        public CrmDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning, IFileStorage fileStorage)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _fileStorage = fileStorage;
        }

        public ApplicationType Application => ApplicationType.Crm;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var idAz = await IdAzAsync(db, idCompany, cancellationToken);
            return new List<DeletionCountModel>
            {
                new() { Label = CrmMessages.Get("DeletionCountOpportunities"), Count = await db.Crm_Opportunita.CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountActivities"), Count = await db.Crm_Attivita.CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountOffers"), Count = await db.Crm_Offerta.CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountLeads"), Count = await db.Crm_Lead.CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
            };
        }

        // multi-tenant: lo schema si elimina con gli altri e i file con DeletionService (IFileStorage.DeleteCompanyAsync)
        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancy.MultiTenant)
                return;
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var idAz = await IdAzAsync(db, idCompany, cancellationToken);
            if (idAz == 0)
                return;
            await db.Crm_Allegato.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Nota.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Attivita.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Offerta.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Opportunita.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Lead.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_ClienteTag.Where(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_ClienteDati.Where(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_Fase.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_TipoAttivita.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_MotivoPerdita.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_Fonte.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_Prodotto.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Par_Tag.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Crm_Cfg.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        /*
         Utente tolto dall'azienda: i clienti, le opportunita' e i lead restano senza responsabile (non si cancellano),
         le sue attivita' da fare si annullano; note e allegati restano (senza autore).
        */
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var idAz = await IdAzAsync(db, idCompany, cancellationToken);
            await db.Crm_ClienteDati.Where(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await db.Crm_Opportunita.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await db.Crm_Lead.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await db.Crm_Attivita.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAssegnatario == idAspNetUsers && x.Stato == Crm_StatoAttivita.DaFare)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Stato, Crm_StatoAttivita.Annullata), cancellationToken);
            await db.Crm_Nota.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAutore == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAutore, (string?)null), cancellationToken);
            await db.Crm_Allegato.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAutore == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAutore, (string?)null), cancellationToken);
        }

        private static Task<int> IdAzAsync(CrmDbContext db, int idCompany, CancellationToken cancellationToken)
            => db.Az_Anagrafica.Where(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    }

    public class CrmCommonEntityUsage : ICommonEntityUsage
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;

        public CrmCommonEntityUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
        }

        public ApplicationType Application => ApplicationType.Crm;

        public async Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default)
        {
            var usi = new List<string>();
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return usi;
            void Aggiungi(int n, string chiave) { if (n > 0) usi.Add(CrmMessages.Get(chiave, n)); }
            Aggiungi(await db.Crm_Opportunita.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByOpportunities");
            Aggiungi(await db.Crm_Offerta.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByOffers");
            Aggiungi(await db.Crm_Attivita.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByActivities");
            Aggiungi(await db.Crm_Allegato.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByAttachments");
            return usi;
        }
    }
}
