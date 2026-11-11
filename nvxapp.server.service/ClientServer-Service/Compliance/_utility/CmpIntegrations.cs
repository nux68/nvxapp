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

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    /*
     Agganci di Compliance ai meccanismi comuni di Infrastructure:
        - inizializzazione all'attivazione (configurazione predefinita)
        - cancellazione fisica (IApplicationDataDeleter): dati dell'azienda in modalita' singola, utente rimosso
     Lo schema lo sceglie ogni classe (tenant_<IdAzienda>_6 o public), come per gli altri applicativi.
    */
    public static class CmpSchema
    {
        public static async Task<ComplianceDbContext?> ContextAsync(int idCompany, TenancySettings tenancy, IApplicationDbContextFactory factory,
                                                             ITenantProvisioningService provisioning, CancellationToken cancellationToken)
        {
            string schema = TenantSchemaName.Public;
            if (tenancy.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.Compliance);
                if (!(await provisioning.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return null;
            }
            return (ComplianceDbContext)factory.Create(ApplicationType.Compliance, schema);
        }
    }

    public class CmpCompanyInitializer : ICompanyInitializer
    {
        private readonly ICmpUtility _utility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ILogger<CmpCompanyInitializer> _logger;

        public string Name => "Compliance";
        public int Priority => 60;
        public ApplicationType? Application => ApplicationType.Compliance;

        public CmpCompanyInitializer(ICmpUtility utility, ICommonCompanyUtility commonCompanyUtility, ILogger<CmpCompanyInitializer> logger)
        {
            _utility = utility;
            _commonCompanyUtility = commonCompanyUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, true);
            await _utility.EnsureDefaultsAsync(az!.Id);
            _logger.LogInformation("[CmpCompanyInitializer] Azienda {CompanyId}: Compliance inizializzato.", company.Id);
        }
    }

    public class CmpDataDeleter : IApplicationDataDeleter
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public CmpDataDeleter(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                              IServiceProvider sp, IHttpContextAccessor http)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
        }

        public ApplicationType Application => ApplicationType.Compliance;

        // dati principali del modulo (sicurezza e registro GDPR)
        public async Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await CmpSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<DeletionCountModel>();
            var rep = new CmpRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (idAz == 0)
                return new List<DeletionCountModel>();
            return new List<DeletionCountModel>
            {
                new() { Label = ComplianceMessages.Get("DeletionCountTraining"), Count = await rep.Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken) },
                new() { Label = ComplianceMessages.Get("DeletionCountVisits"), Count = await rep.Cmp_Visita.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken) },
                new() { Label = ComplianceMessages.Get("DeletionCountPpe"), Count = await rep.Cmp_DpiConsegna.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken) },
                new() { Label = ComplianceMessages.Get("DeletionCountGdpr"), Count = await rep.Cmp_Trattamento.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken)
                                                                                    + await rep.Cmp_Responsabile.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken)
                                                                                    + await rep.Cmp_Violazione.FindAll(x => x.IdAz_Anagrafica == idAz).CountAsync(cancellationToken) },
            };
        }

        // multi-tenant: lo schema si elimina con gli altri (DeletionService)
        public async Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (_tenancy.MultiTenant)
                return;
            await using var db = await CmpSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var rep = new CmpRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (idAz == 0)
                return;
            // figli prima dei padri (consegne prima del catalogo DPI, corsi richiesti e assegnazioni prima delle mansioni)
            await rep.Cmp_DpiConsegna.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Dpi.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_MansioneCorso.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_DipendenteMansione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Mansione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Visita.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Nomina.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Documento.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Trattamento.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Responsabile.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Violazione.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
            await rep.Cmp_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).ExecuteDeleteAsync(cancellationToken);
        }

        // utente rimosso: esce dagli abilitati ai dati sanitari (i dati dei dipendenti seguono Dip_Anagrafica in Common)
        public async Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default)
        {
            await using var db = await CmpSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return;
            var rep = new CmpRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            var cfg = await rep.Cmp_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).AsTracking().FirstOrDefaultAsync(cancellationToken);
            if (cfg == null)
                return;
            var sanitari = CmpUtility.UtentiSanitari(cfg);
            if (!sanitari.Remove(idAspNetUsers))
                return;
            cfg.UtentiDatiSanitari = sanitari.Count == 0 ? null : string.Join(';', sanitari);
            await rep.Cmp_Cfg.SaveChange();
        }
    }
}
