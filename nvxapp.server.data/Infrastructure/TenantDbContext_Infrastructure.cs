using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.data.Infrastructure
{
    public partial class TenantDbContext : DbContext
    {
        public DbSet<MyTable> MyTables { get; set; }


        private void Define_Table_TenantDbContext_Infrastructure(ModelBuilder modelBuilder)
        {
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
