using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 1 - Moke (applicativo di esempio).
     multi-tenant: tenant_<IdAzienda>_1     modalita' singola: public
     Migration in Migrations/Moke.
    */
    public class MokeDbContext : ApplicationDbContextBase
    {
        public MokeDbContext(DbContextOptions<MokeDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.Moke;

        public DbSet<MyTable> MyTables { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Gen_MyTable(modelBuilder);
        }

        private void Gen_MyTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyTable>(entity =>
            {
                entity.ToTable("MyTable");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();

                // Crea un indice univoco
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });
        }
    }
}
