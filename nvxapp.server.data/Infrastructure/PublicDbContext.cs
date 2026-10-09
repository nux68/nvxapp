using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;



namespace nvxapp.server.data.Infrastructure
{
    /*
     Contesto delle tabelle CONDIVISE (sempre nello schema public):
     Identity, gerarchia Dealer -> FinancialAdvisor -> Company, associazioni utente, impostazioni.

     Le tabelle degli applicativi stanno nei rispettivi contesti (ApplicationDbContextBase).
     Ogni modulo aggiunge le proprie tabelle condivise con un file partial PublicDbContext_<Modulo>.cs.
    */
    public partial class PublicDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public const string Schema = "public";
        public const string MigrationsHistoryTable = "__EFMigrationsHistory";

        static PublicDbContext()
        {
            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public PublicDbContext(DbContextOptions<PublicDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            Define_Table_DbContext_Infrastructure(modelBuilder);
        }

    }


}
