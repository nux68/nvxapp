using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Agganci del documentale ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (impostazioni, tipologie e cartelle predefinite, riga dello spazio dell'azienda)
        - cancellazione fisica (IApplicationDataDeleter)
        - spazio reale occupato per il riallineamento notturno (IStorageUsageProvider)
        - clienti comuni usati dai documenti (ICommonEntityUsage)
     Lo schema lo sceglie ogni classe (tenant_<IdAzienda>_4 o public), come per gli altri applicativi.
    */
    internal static class DocumentManagementSchema
    {
        public static async Task<DocumentManagementDbContext?> ContextAsync(int idCompany, TenancySettings tenancy, IApplicationDbContextFactory factory,
                                                                            ITenantProvisioningService provisioning, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (tenancy.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.DocumentManagement);
                if (!(await provisioning.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (DocumentManagementDbContext)factory.Create(ApplicationType.DocumentManagement, schema);
        }
    }

    /*
     Repository del documentale su un contesto creato per uno schema preciso (fuori da una richiesta HTTP: cancellazione
     fisica, riallineamento dello spazio, uso dei clienti). Si accede ai dati solo da qui, mai dal contesto.
    */
    internal sealed class DocumentManagementRepos
    {
        public DocumentManagementRepos(DocumentManagementDbContext db, IServiceProvider sp, IHttpContextAccessor http)
        {
            Cfg = new Doc_CfgRepository(db, sp, http);
            Tipo = new Doc_Par_TipoRepository(db, sp, http);
            Cartella = new Doc_CartellaRepository(db, sp, http);
            Abilitazione = new Doc_UtenteAbilitazioneRepository(db, sp, http);
            Documento = new Doc_DocumentoRepository(db, sp, http);
            Versione = new Doc_VersioneRepository(db, sp, http);
            Anagrafica = new Doc_Az_AnagraficaReadRepository(db, sp, http);
        }

        public Doc_CfgRepository Cfg { get; }
        public Doc_Par_TipoRepository Tipo { get; }
        public Doc_CartellaRepository Cartella { get; }
        public Doc_UtenteAbilitazioneRepository Abilitazione { get; }
        public Doc_DocumentoRepository Documento { get; }
        public Doc_VersioneRepository Versione { get; }
        public Doc_Az_AnagraficaReadRepository Anagrafica { get; }
    }

    public class DocumentManagementCompanyInitializer : ICompanyInitializer
    {
        private readonly IDocumentManagementUtility _utility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IStorageQuotaService _quota;
        private readonly ILogger<DocumentManagementCompanyInitializer> _logger;

        public string Name => "DocumentManagement";
        public int Priority => 30;
        public ApplicationType? Application => ApplicationType.DocumentManagement;

        public DocumentManagementCompanyInitializer(IDocumentManagementUtility utility, ICommonCompanyUtility commonCompanyUtility,
                                                    IStorageQuotaService quota, ILogger<DocumentManagementCompanyInitializer> logger)
        {
            _utility = utility;
            _commonCompanyUtility = commonCompanyUtility;
            _quota = quota;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, true);
            await _utility.EnsureDefaultsAsync(az!.Id);
            await _quota.EnsureAsync(company.Id);
            _logger.LogInformation("[DocumentManagementCompanyInitializer] Azienda {CompanyId}: documentale inizializzato.", company.Id);
        }
    }

    public class DocumentManagementDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentManagementDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
            IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var r = new DocumentManagementRepos(db, _serviceProvider, _httpContextAccessor);
            var documenti = await r.Documento.FindAll(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany).CountAsync(cancellationToken);
            return new List<DeletionCountModel> { new DeletionCountModel { Label = DocumentManagementMessages.Get("DeletionCountDocuments"), Count = documenti } };
        }

        // multi-tenant: lo schema si elimina con gli altri; i file li elimina DeletionService (IFileStorage.DeleteCompanyAsync)
        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancy.MultiTenant)
                return;
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var r = new DocumentManagementRepos(db, _serviceProvider, _httpContextAccessor);
            var idAz = await r.Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync(cancellationToken);
            if (!idAz.HasValue)
                return;
            await r.Documento.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await r.Abilitazione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await r.Cartella.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdDoc_Cartella != null).ExecuteDeleteAsync(cancellationToken);
            await r.Cartella.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await r.Tipo.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await r.Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        // utente tolto dall'azienda: i documenti del personale restano (conservazione); si tolgono solo le sue abilitazioni
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var r = new DocumentManagementRepos(db, _serviceProvider, _httpContextAccessor);
            var idAz = await r.Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync(cancellationToken);
            await r.Abilitazione.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsers == idAspNetUsers).ExecuteDeleteAsync(cancellationToken);
        }
    }

    public class DocumentManagementStorageUsage : IStorageUsageProvider
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentManagementStorageUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
            IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<(long Bytes, int Files)> UsageAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return (0, 0);
            var r = new DocumentManagementRepos(db, _serviceProvider, _httpContextAccessor);
            var q = r.Versione.FindAll(v => v.Doc_DocumentoNavigation!.Az_AnagraficaNavigation!.IdCompany == idCompany);
            return (await q.SumAsync(v => (long?)v.Dimensione, cancellationToken) ?? 0, await q.CountAsync(cancellationToken));
        }
    }

    public class DocumentManagementCommonEntityUsage : ICommonEntityUsage
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentManagementCommonEntityUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
            IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default)
        {
            var usi = new List<string>();
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return usi;
            var r = new DocumentManagementRepos(db, _serviceProvider, _httpContextAccessor);
            var documenti = await r.Documento.FindAll(x => x.IdAz_Cliente == idAz_Cliente).CountAsync(cancellationToken);
            if (documenti > 0)
                usi.Add(DocumentManagementMessages.Get("CustomerUsedByDocuments", documenti));
            return usi;
        }
    }
}
