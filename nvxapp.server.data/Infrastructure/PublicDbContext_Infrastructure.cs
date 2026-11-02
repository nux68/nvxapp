using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.data.Infrastructure
{
    public partial class PublicDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IDataProtectionKeyContext
    {
        // chiavi DataProtection: firmano i token di Identity (link di conferma email e reset password).
        // Su DB sopravvivono a riavvii, ricicli dell'app pool, pubblicazioni e redeploy dei container.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<AppSetting> AppSetting { get; set; }
        public DbSet<CompanyApplication> CompanyApplication { get; set; }
        public DbSet<CompanyMenuRestriction> CompanyMenuRestriction { get; set; }
        public DbSet<CompanyRegistrationRequest> CompanyRegistrationRequest { get; set; }

        public DbSet<Dealer> Dealer { get; set; }
        public DbSet<UserDealer> UserDealer { get; set; }
        public DbSet<FinancialAdvisor> FinancialAdvisor { get; set; }
        public DbSet<UserFinancialAdvisor> UserFinancialAdvisor { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }

        // ticket di assistenza
        public DbSet<TicketType> TicketType { get; set; }
        public DbSet<TicketTypeLevel> TicketTypeLevel { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketCronology> TicketCronology { get; set; }
        public DbSet<TicketAttach> TicketAttach { get; set; }
        public DbSet<TicketTypeAiSetting> TicketTypeAiSetting { get; set; }
        public DbSet<TicketCronologyDocument> TicketCronologyDocument { get; set; }

        // documenti dei livelli (es. documentazione per le risposte AI ai ticket)
        public DbSet<Document> Document { get; set; }
        public DbSet<DocumentTicketType> DocumentTicketType { get; set; }

        // registro delle cancellazioni fisiche (centri, studi, aziende, utenti)
        public DbSet<DeletionLog> DeletionLog { get; set; }


        private void Define_Table_DbContext_Infrastructure(ModelBuilder modelBuilder)
        {
            //1
            Gen_InitDB(modelBuilder);
            Gen_DataProtectionKeys(modelBuilder);
            Gen_DealerAndCompany_Init(modelBuilder);
            Gen_AppSetting(modelBuilder);
            Gen_CompanyApplication(modelBuilder);
            Gen_CompanyMenuRestriction(modelBuilder);
            Gen_CompanyRegistrationRequest(modelBuilder);
            Gen_Ticket(modelBuilder);
            Gen_Document(modelBuilder);
            Gen_DeletionLog(modelBuilder);
        }

        private void Gen_DeletionLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeletionLog>(entity =>
            {
                entity.ToTable("DeletionLog", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Data);
            });
        }

        private void Gen_InitDB(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable("AspNetUsers", Schema); });
            modelBuilder.Entity<ApplicationRole>(entity => { entity.ToTable("AspNetRoles", Schema); });
            modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("AspNetUserRoles", Schema); });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims", Schema); });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("AspNetUserLogins", Schema); });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims", Schema); });
            modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("AspNetUserTokens", Schema); });
        }
        private void Gen_DataProtectionKeys(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataProtectionKey>(entity => { entity.ToTable("DataProtectionKeys", Schema); });
        }
        private void Gen_AppSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSetting", Schema);
                entity.HasKey(e => e.Key);
            });
        }
        private void Gen_CompanyApplication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyApplication>(entity =>
            {
                entity.ToTable("CompanyApplication", Schema);
                entity.HasKey(e => e.Id);

                // un solo record per azienda/applicativo
                entity.HasIndex(e => new { e.IdCompany, e.ApplicationType }).IsUnique();

                entity.HasOne(e => e.CompanyNavigation)
                      .WithMany(c => c.CompanyApplication)
                      .HasForeignKey(e => e.IdCompany)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        // richieste di registrazione dallo showcase in attesa della conferma dell'email
        private void Gen_CompanyRegistrationRequest(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyRegistrationRequest>(entity =>
            {
                entity.ToTable("CompanyRegistrationRequest", Schema);
                entity.HasKey(e => e.Id);

                // il link si cerca per hash del token
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => e.Descrizione);
            });
        }

        // ticket di assistenza: relazioni solo tra le tabelle dei ticket. Verso Company, Dealer, FinancialAdvisor
        // e AspNetUsers ci sono solo gli id (nessuna relazione): i contesti degli applicativi mappano Company
        // e scoprirebbero le tabelle, e la cronologia deve restare anche se l'azienda o l'utente spariscono
        private void Gen_Ticket(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketType>(entity =>
            {
                entity.ToTable("TicketType", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Codice).IsUnique();
            });

            modelBuilder.Entity<TicketTypeLevel>(entity =>
            {
                entity.ToTable("TicketTypeLevel", Schema);
                entity.HasKey(e => e.Id);

                // un livello abilitato una sola volta per tipo
                entity.HasIndex(e => new { e.IdTicketType, e.Level }).IsUnique();

                entity.HasOne(e => e.TicketTypeNavigation)
                      .WithMany(t => t.TicketTypeLevel)
                      .HasForeignKey(e => e.IdTicketType)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket", Schema);
                entity.HasKey(e => e.Id);

                // filtri degli elenchi: chi ha aperto, in carico, ticket dei nodi figli
                entity.HasIndex(e => e.IdAspNetUsersOpener);
                entity.HasIndex(e => new { e.CurrentLevel, e.State });
                entity.HasIndex(e => e.CurrentIdAspNetUsers);
                entity.HasIndex(e => e.IdDealer);
                entity.HasIndex(e => e.IdFinancialAdvisor);
                entity.HasIndex(e => e.IdCompany);

                // un tipo in uso non si cancella: si disattiva
                entity.HasOne(e => e.TicketTypeNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdTicketType)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TicketCronology>(entity =>
            {
                entity.ToTable("TicketCronology", Schema);
                entity.HasKey(e => e.Id);

                // assegnati in passato (sola lettura per l'Admin)
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.TicketNavigation)
                      .WithMany(t => t.TicketCronology)
                      .HasForeignKey(e => e.IdTicket)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TicketAttach>(entity =>
            {
                entity.ToTable("TicketAttach", Schema);
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.TicketCronologyNavigation)
                      .WithMany(c => c.TicketAttach)
                      .HasForeignKey(e => e.IdTicketCronology)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TicketTypeAiSetting>(entity =>
            {
                entity.ToTable("TicketTypeAiSetting", Schema, t =>
                    t.HasCheckConstraint("CK_TicketTypeAiSetting_Node", NodeCheck));
                entity.HasKey(e => e.Id);

                // una riga per tipo/livello/nodo. Indici parziali (NULLS NOT DISTINCT richiede PostgreSQL 15):
                // default del livello (nodo vuoto) e una riga per nodo (il livello e' implicito nel nodo)
                entity.HasIndex(e => new { e.IdTicketType, e.Level })
                      .IsUnique()
                      .HasFilter("\"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL");
                entity.HasIndex(e => new { e.IdTicketType, e.IdDealer }).IsUnique().HasFilter("\"IdDealer\" IS NOT NULL");
                entity.HasIndex(e => new { e.IdTicketType, e.IdFinancialAdvisor }).IsUnique().HasFilter("\"IdFinancialAdvisor\" IS NOT NULL");
                entity.HasIndex(e => new { e.IdTicketType, e.IdCompany }).IsUnique().HasFilter("\"IdCompany\" IS NOT NULL");

                entity.HasOne(e => e.TicketTypeNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdTicketType)
                      .OnDelete(DeleteBehavior.Cascade);

                NodeForeignKeys(entity);
            });

            modelBuilder.Entity<TicketCronologyDocument>(entity =>
            {
                entity.ToTable("TicketCronologyDocument", Schema);
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.TicketCronologyNavigation)
                      .WithMany(c => c.TicketCronologyDocument)
                      .HasForeignKey(e => e.IdTicketCronology)
                      .OnDelete(DeleteBehavior.Cascade);

                // il documento eliminato resta citato con il titolo copiato
                entity.HasOne(e => e.DocumentNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdDocument)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        /*
         Documenti dei livelli. Il nodo proprietario e' solo quello del livello (check constraint);
         chiavi esterne verso Dealer, FinancialAdvisor e Company senza collezioni sui nodi: i contesti
         degli applicativi mappano Company e scoprirebbero le tabelle seguendo le collezioni.
         Eliminando il nodo si eliminano i suoi documenti.
        */
        private void Gen_Document(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Document", Schema, t =>
                    t.HasCheckConstraint("CK_Document_Node", NodeCheck));
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.Section, e.Level, e.IdDealer, e.IdFinancialAdvisor, e.IdCompany, e.Active });

                NodeForeignKeys(entity);
            });

            modelBuilder.Entity<DocumentTicketType>(entity =>
            {
                entity.ToTable("DocumentTicketType", Schema);
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.IdDocument, e.IdTicketType }).IsUnique();
                entity.HasIndex(e => e.IdTicketType);

                entity.HasOne(e => e.DocumentNavigation)
                      .WithMany(d => d.DocumentTicketType)
                      .HasForeignKey(e => e.IdDocument)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TicketTypeNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdTicketType)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // nodo coerente con il livello (TicketLevel): SuperUser e Dev nessuno, poi solo l'id del livello
        private const string NodeCheck =
            "(\"Level\" IN (0, 1) AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL)" +
            " OR (\"Level\" = 2 AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL)" +
            " OR (\"Level\" = 3 AND \"IdDealer\" IS NULL AND \"IdCompany\" IS NULL)" +
            " OR (\"Level\" = 4 AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL)";

        // chiavi esterne verso il nodo proprietario, in cascata, senza navigation
        private static void NodeForeignKeys<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> entity) where T : class
        {
            entity.HasOne<Dealer>().WithMany().HasForeignKey("IdDealer").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<FinancialAdvisor>().WithMany().HasForeignKey("IdFinancialAdvisor").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey("IdCompany").OnDelete(DeleteBehavior.Cascade);
        }

        private void Gen_CompanyMenuRestriction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyMenuRestriction>(entity =>
            {
                entity.ToTable("CompanyMenuRestriction", Schema);
                entity.HasKey(e => e.Id);

                // un solo record per azienda/voce/ruolo
                entity.HasIndex(e => new { e.IdCompany, e.Url, e.Role }).IsUnique();

                // relazione senza collezione su Company: i contesti degli applicativi mappano Company
                // e scoprirebbero la tabella seguendo la collezione (tabella duplicata, avvio bloccato)
                entity.HasOne(e => e.CompanyNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdCompany)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        private void Gen_DealerAndCompany_Init(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dealer>(entity =>
            {
                entity.ToTable("Dealer", Schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserDealer>(entity =>
            {
                entity.ToTable("UserDealer", Schema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdDealer, e.IdAspNetUsers }).IsUnique();
            });


            modelBuilder.Entity<FinancialAdvisor>(entity =>
            {
                entity.ToTable("FinancialAdvisor", Schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserFinancialAdvisor>(entity =>
            {
                entity.ToTable("UserFinancialAdvisor", Schema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdFinancialAdvisor, e.IdAspNetUsers }).IsUnique();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company", Schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserCompany>(entity =>
            {
                entity.ToTable("UserCompany", Schema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdCompany, e.IdAspNetUsers }).IsUnique();
            });

            #region CONFIGURE RELATIONS ONE-TO-MANY 

            #region Dealer-FinancialAdvisor

            modelBuilder.Entity<FinancialAdvisor>()
                       .HasOne(md => md.DealerNavigation)
                       .WithMany(d => d.FinancialAdvisor)
                       .HasForeignKey(md => md.IdDealer);
            // nessuna cascata: un centro con studi si elimina solo dal servizio di cancellazione (DeletionService),
            // che elimina in ordine anche utenti, schemi degli applicativi e ticket del ramo
            modelBuilder.Entity<Dealer>()
                        .HasMany(o => o.FinancialAdvisor)
                        .WithOne(i => i.DealerNavigation)
                        .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region FinancialAdvisor-Company
            modelBuilder.Entity<Company>()
                        .HasOne(md => md.FinancialAdvisorNavigation)
                        .WithMany(d => d.Company)
                        .HasForeignKey(md => md.IdFinancialAdvisor);
            // nessuna cascata: vedi Dealer-FinancialAdvisor
            modelBuilder.Entity<FinancialAdvisor>()
                        .HasMany(o => o.Company)
                        .WithOne(i => i.FinancialAdvisorNavigation)
                        .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Dealer-UserDealer

            modelBuilder.Entity<UserDealer>()
                   .HasOne(md => md.DealerNavigation)
                   .WithMany(d => d.UserDealer)
                   .HasForeignKey(md => md.IdDealer);

            modelBuilder.Entity<Dealer>()
                        .HasMany(o => o.UserDealer)
                        .WithOne(i => i.DealerNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserDealer>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserDealer)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserDealer)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region FinancialAdvisor-UserFinancialAdvisor

            modelBuilder.Entity<UserFinancialAdvisor>()
                   .HasOne(md => md.FinancialAdvisorNavigation)
                   .WithMany(d => d.UserFinancialAdvisor)
                   .HasForeignKey(md => md.IdFinancialAdvisor);

            modelBuilder.Entity<FinancialAdvisor>()
                        .HasMany(o => o.UserFinancialAdvisor)
                        .WithOne(i => i.FinancialAdvisorNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserFinancialAdvisor>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserFinancialAdvisor)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserFinancialAdvisor)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Company-UserCompany

            modelBuilder.Entity<UserCompany>()
                        .HasOne(md => md.CompanyNavigation)
                        .WithMany(d => d.UserCompany)
                        .HasForeignKey(md => md.IdCompany);

            modelBuilder.Entity<Company>()
                        .HasMany(o => o.UserCompany)
                        .WithOne(i => i.CompanyNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserCompany>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserCompany)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserCompany)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #endregion

        }




    }
}
