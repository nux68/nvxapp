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
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Agganci del documentale ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (impostazioni, tipologie e cartelle predefinite, spazio con il pacchetto predefinito)
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

        public DocumentManagementDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var documenti = await db.Doc_Documento.CountAsync(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany, cancellationToken);
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
            var idAz = await db.Az_Anagrafica.Where(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync(cancellationToken);
            if (!idAz.HasValue)
                return;
            await db.Doc_Documento.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Doc_UtenteAbilitazione.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Doc_Cartella.Where(x => x.IdAz_Anagrafica == idAz && x.IdDoc_Cartella != null).ExecuteDeleteAsync(cancellationToken);
            await db.Doc_Cartella.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Doc_Par_Tipo.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await db.Doc_Cfg.Where(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        // utente tolto dall'azienda: i documenti del personale restano (conservazione); si tolgono solo le sue abilitazioni
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var idAz = await db.Az_Anagrafica.Where(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync(cancellationToken);
            await db.Doc_UtenteAbilitazione.Where(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsers == idAspNetUsers).ExecuteDeleteAsync(cancellationToken);
        }
    }

    public class DocumentManagementStorageUsage : IStorageUsageProvider
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;

        public DocumentManagementStorageUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<(long Bytes, int Files)> UsageAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return (0, 0);
            var q = db.Doc_Versione.Where(v => v.Doc_DocumentoNavigation!.Az_AnagraficaNavigation!.IdCompany == idCompany);
            return (await q.SumAsync(v => (long?)v.Dimensione, cancellationToken) ?? 0, await q.CountAsync(cancellationToken));
        }
    }

    public class DocumentManagementCommonEntityUsage : ICommonEntityUsage
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;

        public DocumentManagementCommonEntityUsage(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
        }

        public ApplicationType Application => ApplicationType.DocumentManagement;

        public async Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default)
        {
            var usi = new List<string>();
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return usi;
            var documenti = await db.Doc_Documento.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken);
            if (documenti > 0)
                usi.Add(DocumentManagementMessages.Get("CustomerUsedByDocuments", documenti));
            return usi;
        }
    }
}
