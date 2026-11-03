using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /*
     Uso delle tabelle comuni (schema Common) da parte delle presenze: un cliente con commesse non si cancella.
     Lo schema si sceglie qui (anche con l'applicativo disattivato i dati restano e le chiavi esterne bloccano).
    */
    public class AttendanceTrackingCommonEntityUsage : ICommonEntityUsage
    {
        private readonly TenancySettings _tenancySettings;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly ITenantProvisioningService _tenantProvisioningService;

        public AttendanceTrackingCommonEntityUsage(TenancySettings tenancySettings,
                                                   IApplicationDbContextFactory applicationDbContextFactory,
                                                   ITenantProvisioningService tenantProvisioningService)
        {
            _tenancySettings = tenancySettings;
            _applicationDbContextFactory = applicationDbContextFactory;
            _tenantProvisioningService = tenantProvisioningService;
        }

        public ApplicationType Application => ApplicationType.AttendanceTracking;

        public async Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default)
        {
            var usi = new List<string>();

            string schema = TenantSchemaName.Public;
            if (_tenancySettings.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.AttendanceTracking);
                if (!(await _tenantProvisioningService.CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                    return usi;
            }

            await using var context = (AttendanceTrackingDbContext)_applicationDbContextFactory.Create(ApplicationType.AttendanceTracking, schema);
            int commesse = await context.Az_Commessa.CountAsync(x => x.IdAz_Cliente == idAz_Cliente, cancellationToken);
            if (commesse > 0)
                usi.Add(AttendanceTrackingMessages.Get("CustomerUsedByJobs", commesse));
            return usi;
        }
    }
}
