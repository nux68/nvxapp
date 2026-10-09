using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per l'applicativo AttendanceTracking (vedi Note/Migrazioni.txt).
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class AttendanceTrackingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AttendanceTrackingDbContext>
    {
        public AttendanceTrackingDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AttendanceTrackingDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.AttendanceTracking);
            return new AttendanceTrackingDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
