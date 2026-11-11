using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;
        private readonly IFileStorage _fileStorage;

        public CrmDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning, IFileStorage fileStorage,
                              IServiceProvider sp, IHttpContextAccessor http)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
            _fileStorage = fileStorage;
        }

        public ApplicationType Application => ApplicationType.Crm;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var rep = new CrmRepositorySet(db, _sp, _http);
            var idAz = await IdAzAsync(rep, idCompany, cancellationToken);
            return new List<DeletionCountModel>
            {
                new() { Label = CrmMessages.Get("DeletionCountOpportunities"), Count = await rep.Crm_Opportunita.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountActivities"), Count = await rep.Crm_Attivita.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountOffers"), Count = await rep.Crm_Offerta.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountLeads"), Count = await rep.Crm_Lead.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
                new() { Label = CrmMessages.Get("DeletionCountRequests"), Count = await rep.Crm_Richiesta.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == idAz, cancellationToken) },
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
            var rep = new CrmRepositorySet(db, _sp, _http);
            var idAz = await IdAzAsync(rep, idCompany, cancellationToken);
            if (idAz == 0)
                return;
            await rep.Crm_Allegato.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            // richieste di assistenza (i messaggi in cascata)
            await rep.Crm_Richiesta.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Nota.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Attivita.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Offerta.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Opportunita.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Lead.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_ClienteTag.FindAll(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_ClienteDati.FindAll(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_Fase.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_TipoAttivita.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_MotivoPerdita.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_Fonte.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_Prodotto.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Par_Tag.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Crm_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
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
            var rep = new CrmRepositorySet(db, _sp, _http);
            var idAz = await IdAzAsync(rep, idCompany, cancellationToken);
            await rep.Crm_ClienteDati.FindAll(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await rep.Crm_Opportunita.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await rep.Crm_Lead.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersResponsabile == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, (string?)null), cancellationToken);
            await rep.Crm_Attivita.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAssegnatario == idAspNetUsers && x.Stato == Crm_StatoAttivita.DaFare)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Stato, Crm_StatoAttivita.Annullata), cancellationToken);
            await rep.Crm_Nota.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAutore == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAutore, (string?)null), cancellationToken);
            await rep.Crm_Allegato.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAutore == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAutore, (string?)null), cancellationToken);
            // richieste di assistenza: restano senza assegnatario, i messaggi senza autore
            await rep.Crm_Richiesta.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsersAssegnatario == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAssegnatario, (string?)null), cancellationToken);
            await rep.Crm_RichiestaMessaggio.FindAll(x => x.Crm_RichiestaNavigation!.IdAz_Anagrafica == idAz && x.IdAspNetUsersAutore == idAspNetUsers)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAutore, (string?)null), cancellationToken);
        }

        private static Task<int> IdAzAsync(CrmRepositorySet rep, int idCompany, CancellationToken cancellationToken)
            => rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    }

    public class CrmCommonEntityUsage : ICommonEntityUsage
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public CrmCommonEntityUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                                    IServiceProvider sp, IHttpContextAccessor http)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
        }

        public ApplicationType Application => ApplicationType.Crm;

        public async Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default)
        {
            var usi = new List<string>();
            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return usi;
            var rep = new CrmRepositorySet(db, _sp, _http);
            void Aggiungi(int n, string chiave) { if (n > 0) usi.Add(CrmMessages.Get(chiave, n)); }
            Aggiungi(await rep.Crm_Opportunita.FindAll(_ => true).CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByOpportunities");
            Aggiungi(await rep.Crm_Offerta.FindAll(_ => true).CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByOffers");
            Aggiungi(await rep.Crm_Attivita.FindAll(_ => true).CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByActivities");
            Aggiungi(await rep.Crm_Allegato.FindAll(_ => true).CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByAttachments");
            Aggiungi(await rep.Crm_Richiesta.FindAll(_ => true).CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken), "CustomerUsedByRequests");
            return usi;
        }
    }
}
