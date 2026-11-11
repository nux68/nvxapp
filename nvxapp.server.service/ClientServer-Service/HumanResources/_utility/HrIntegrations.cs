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

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Agganci di Risorse Umane ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (configurazione predefinita)
        - cancellazione fisica (IApplicationDataDeleter): dati dell'azienda in modalita' singola, utente rimosso
     Lo schema lo sceglie ogni classe (tenant_<IdAzienda>_5 o public), come per gli altri applicativi.
    */
    public static class HrSchema
    {
        public static async Task<HumanResourcesDbContext?> ContextAsync(int idCompany, TenancySettings tenancy, IApplicationDbContextFactory factory,
                                                             ITenantProvisioningService provisioning, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (tenancy.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.HumanResources);
                if (!(await provisioning.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (HumanResourcesDbContext)factory.Create(ApplicationType.HumanResources, schema);
        }
    }

    public class HrCompanyInitializer : ICompanyInitializer
    {
        private readonly IHrUtility _utility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ILogger<HrCompanyInitializer> _logger;

        public string Name => "HumanResources";
        public int Priority => 50;
        public ApplicationType? Application => ApplicationType.HumanResources;

        public HrCompanyInitializer(IHrUtility utility, ICommonCompanyUtility commonCompanyUtility, ILogger<HrCompanyInitializer> logger)
        {
            _utility = utility;
            _commonCompanyUtility = commonCompanyUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, true);
            await _utility.EnsureDefaultsAsync(az!.Id);
            _logger.LogInformation("[HrCompanyInitializer] Azienda {CompanyId}: Risorse Umane inizializzato.", company.Id);
        }
    }

    public class HrDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public HrDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                              IServiceProvider sp, IHttpContextAccessor http)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
        }

        public ApplicationType Application => ApplicationType.HumanResources;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await HrSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var rep = new HrRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            return new List<DeletionCountModel>
            {
                new() { Label = HumanResourcesMessages.Get("DeletionCountDocuments"), Count = await rep.Hr_DocDipendente.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = HumanResourcesMessages.Get("DeletionCountCourses"), Count = await rep.Hr_Corso.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = HumanResourcesMessages.Get("DeletionCountChecklists"), Count = await rep.Hr_Checklist.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
            };
        }

        // multi-tenant: lo schema si elimina con gli altri (DeletionService)
        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancy.MultiTenant)
                return;
            await using var db = await HrSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var rep = new HrRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (idAz == 0)
                return;
            // i file stanno in IFileStorage: li elimina DeletionService con quelli dell'azienda
            await rep.Hr_DocCaricamento.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Hr_PianoFormazione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Hr_Corso.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Hr_Checklist.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Hr_ChecklistModello.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Hr_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        /*
         Utente tolto dall'azienda: i documenti del dipendente (cedolini, attestati, checklist) restano all'azienda per gli obblighi
         di conservazione (HR-D4, scelta provvisoria); l'utente perde solo l'accesso. Le voci di cui era responsabile restano
         senza responsabile (avvisi agli amministratori).
        */
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var db = await HrSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var rep = new HrRepositorySet(db, _sp, _http);
            await rep.Hr_ChecklistModelloVoce.FindAll(x => x.IdAspNetUsersResponsabile == idAspNetUsers)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await rep.Hr_ChecklistVoce.FindAll(x => x.IdAspNetUsersResponsabile == idAspNetUsers)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
        }
    }
}
