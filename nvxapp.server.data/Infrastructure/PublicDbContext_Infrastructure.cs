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
        public DbSet<FinancialAdvisorApplication> FinancialAdvisorApplication { get; set; }

        // spazio dei file delle aziende (documentale)
        public DbSet<CompanyStorage> CompanyStorage { get; set; }
        public DbSet<CompanyStorageHistory> CompanyStorageHistory { get; set; }
        public DbSet<CompanyMenuRestriction> CompanyMenuRestriction { get; set; }
        // widget della dashboard scelti da ogni utente (ANALISI_INFRASTRUCTURE_WIDGET.md)
        public DbSet<UserDashboard> UserDashboard { get; set; }
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

        // licenze d'uso degli applicativi (ANALISI_INFRASTRUCTURE_LICENZE.md)
        public DbSet<Lic_Listino> Lic_Listino { get; set; }
        public DbSet<Lic_Piano> Lic_Piano { get; set; }
        public DbSet<Lic_Abbonamento> Lic_Abbonamento { get; set; }
        public DbSet<Lic_Pagamento> Lic_Pagamento { get; set; }
        public DbSet<Pag_Rendiconto> Pag_Rendiconto { get; set; }
        public DbSet<Pag_DealerConfig> Pag_DealerConfig { get; set; }
        public DbSet<Pag_InvioEmail> Pag_InvioEmail { get; set; }


        private void Define_Table_DbContext_Infrastructure(ModelBuilder modelBuilder)
        {
            //1
            Gen_InitDB(modelBuilder);
            Gen_DataProtectionKeys(modelBuilder);
            Gen_DealerAndCompany_Init(modelBuilder);
            Gen_AppSetting(modelBuilder);
            Gen_CompanyApplication(modelBuilder);
            Gen_FinancialAdvisorApplication(modelBuilder);
            Gen_Storage(modelBuilder);
            Gen_CompanyMenuRestriction(modelBuilder);
            Gen_UserDashboard(modelBuilder);
            Gen_CompanyRegistrationRequest(modelBuilder);
            Gen_Ticket(modelBuilder);
            Gen_Document(modelBuilder);
            Gen_DeletionLog(modelBuilder);
            Gen_License(modelBuilder);
            Gen_Payments(modelBuilder);
        }

        /*
         Licenze d'uso (ANALISI_INFRASTRUCTURE_LICENZE.md).
         Impostazioni (AppSetting):
            - Licenze_Attive          : "true" = le regole delle licenze si applicano; altrimenti ILicenseGuard risponde
                                        in modo neutro (come Pro, utenti illimitati) e nessuno viene bloccato (§8.1.1).
                                        Acceso alla creazione del database (10/11/2026)
            - Licenze_MaxUtenti_*     : utenti predefiniti dei piani dei nuovi listini (Free 10, Standard 50, Pro illimitati) (§5.1)
            - Licenze_MaxClienti_*    : clienti predefiniti (CRM: Free 100, Standard 1000, Pro illimitati)
            - Licenze_MaxSpazioMB_*   : spazio predefinito in MB (Documentale: Free 1024, Standard 5120, Pro 20480)
            - Licenze_GraceGiorni     : giorni dopo la scadenza in cui la licenza vale ancora (si avvisa ma non si blocca)
            - Licenze_AvvisoGiorni    : giorni prima della scadenza in cui parte l'avviso
         Cancellazioni: gli abbonamenti spariscono con l'azienda o lo studio, i listini con il centro; i pagamenti restano.
        */
        public const string Key_LicenzeAttive = "Licenze_Attive";
        public const string Key_LicenzeMaxUtentiFree = "Licenze_MaxUtenti_Free";
        public const string Key_LicenzeMaxUtentiStandard = "Licenze_MaxUtenti_Standard";
        public const string Key_LicenzeMaxUtentiPro = "Licenze_MaxUtenti_Pro";
        public const string Key_LicenzeMaxClientiFree = "Licenze_MaxClienti_Free";
        public const string Key_LicenzeMaxClientiStandard = "Licenze_MaxClienti_Standard";
        public const string Key_LicenzeMaxClientiPro = "Licenze_MaxClienti_Pro";
        public const string Key_LicenzeMaxSpazioMBFree = "Licenze_MaxSpazioMB_Free";
        public const string Key_LicenzeMaxSpazioMBStandard = "Licenze_MaxSpazioMB_Standard";
        public const string Key_LicenzeMaxSpazioMBPro = "Licenze_MaxSpazioMB_Pro";
        public const string Key_LicenzeGraceGiorni = "Licenze_GraceGiorni";
        public const string Key_LicenzeAvvisoGiorni = "Licenze_AvvisoGiorni";

        private void Gen_License(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lic_Listino>(entity =>
            {
                entity.ToTable("Lic_Listino", Schema);
                entity.HasKey(e => e.Id);
                // un solo listino attivo per (centro, tipo soggetto, ambito); il globale ha IdDealer null
                entity.HasIndex(e => new { e.TipoSoggetto, e.Ambito }).IsUnique().HasFilter("\"Attivo\" AND \"IdDealer\" IS NULL")
                      .HasDatabaseName("IX_Lic_Listino_Globale_Attivo");
                entity.HasIndex(e => new { e.IdDealer, e.TipoSoggetto, e.Ambito }).IsUnique().HasFilter("\"Attivo\" AND \"IdDealer\" IS NOT NULL")
                      .HasDatabaseName("IX_Lic_Listino_Dealer_Attivo");
                entity.HasOne(e => e.DealerNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdDealer)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Lic_Piano>(entity =>
            {
                entity.ToTable("Lic_Piano", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdListino, e.Livello }).IsUnique();
                entity.Property(e => e.PrezzoMensile).HasPrecision(10, 2);
                entity.Property(e => e.PrezzoAnnuale).HasPrecision(10, 2);
                entity.HasOne(e => e.ListinoNavigation)
                      .WithMany(l => l.Lic_Piano)
                      .HasForeignKey(e => e.IdListino)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Lic_Abbonamento>(entity =>
            {
                // soggetto: esattamente uno tra azienda e studio
                entity.ToTable("Lic_Abbonamento", Schema, t => t.HasCheckConstraint("CK_Lic_Abbonamento_Soggetto",
                    "(\"IdCompany\" IS NULL) <> (\"IdFinancialAdvisor\" IS NULL)"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrezzoBloccato).HasPrecision(10, 2);
                entity.HasIndex(e => new { e.IdCompany, e.Ambito });
                entity.HasIndex(e => new { e.IdFinancialAdvisor, e.Ambito });
                entity.HasIndex(e => e.DataScadenza);
                entity.HasIndex(e => e.RiferimentoGateway).IsUnique().HasFilter("\"RiferimentoGateway\" IS NOT NULL");
                entity.HasOne(e => e.CompanyNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdCompany)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.FinancialAdvisorNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdFinancialAdvisor)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ListinoNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdListino)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.PianoNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdPiano)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Lic_Pagamento>(entity =>
            {
                entity.ToTable("Lic_Pagamento", Schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Importo).HasPrecision(10, 2);
                entity.HasIndex(e => e.IdAbbonamento);
                entity.HasIndex(e => e.IdCompany);
                entity.HasIndex(e => e.IdFinancialAdvisor);
                // callback del provider idempotenti (§7.3)
                entity.HasIndex(e => e.RiferimentoGateway).IsUnique().HasFilter("\"RiferimentoGateway\" IS NOT NULL");
                entity.HasOne(e => e.AbbonamentoNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdAbbonamento)
                      .OnDelete(DeleteBehavior.SetNull);
                // ripartizione ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md)
                entity.HasIndex(e => new { e.IdDealer, e.DataPagamento });
                entity.HasIndex(e => e.IdRendiconto);
                entity.HasIndex(e => e.IdRendicontoRettifica);
                entity.HasOne(e => e.RendicontoNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdRendiconto)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.RendicontoRettificaNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdRendicontoRettifica)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AppSetting>().HasData(
                new AppSetting { Key = Key_LicenzeAttive, Value = "true" },
                new AppSetting { Key = Key_LicenzeMaxUtentiFree, Value = "10" },
                new AppSetting { Key = Key_LicenzeMaxUtentiStandard, Value = "50" },
                new AppSetting { Key = Key_LicenzeMaxUtentiPro, Value = "" },
                new AppSetting { Key = Key_LicenzeMaxClientiFree, Value = "100" },
                new AppSetting { Key = Key_LicenzeMaxClientiStandard, Value = "1000" },
                new AppSetting { Key = Key_LicenzeMaxClientiPro, Value = "" },
                new AppSetting { Key = Key_LicenzeMaxSpazioMBFree, Value = "1024" },
                new AppSetting { Key = Key_LicenzeMaxSpazioMBStandard, Value = "5120" },
                new AppSetting { Key = Key_LicenzeMaxSpazioMBPro, Value = "20480" },
                new AppSetting { Key = Key_LicenzeGraceGiorni, Value = "7" },
                new AppSetting { Key = Key_LicenzeAvvisoGiorni, Value = "10" });
        }

        /*
         Ripartizione degli incassi delle licenze ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md).
         Impostazioni (AppSetting):
            - Pagamenti_PercentualePiattaforma : trattenuta della piattaforma (0-100) per i centri senza una propria
            - Pagamenti_RiepilogoAutomatico    : "true" = la manutenzione genera i rendiconti e invia i riepiloghi
            - Pagamenti_GiornoInvioMensile     : giorno del mese in cui si genera il rendiconto del mese precedente
            - Pagamenti_MeseInvioAnnuale       : mese in cui si invia il riepilogo dell'anno precedente
        */
        public const string Key_PagamentiPercentualePiattaforma = "Pagamenti_PercentualePiattaforma";
        public const string Key_PagamentiRiepilogoAutomatico = "Pagamenti_RiepilogoAutomatico";
        public const string Key_PagamentiGiornoInvioMensile = "Pagamenti_GiornoInvioMensile";
        public const string Key_PagamentiMeseInvioAnnuale = "Pagamenti_MeseInvioAnnuale";
        // ultimo mese di cui la manutenzione ha generato le bozze ("aaaa-mm"); scritta dalla manutenzione, non seminata
        public const string Key_PagamentiUltimaGenerazione = "Pagamenti_UltimaGenerazione";

        private void Gen_Payments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pag_Rendiconto>(entity =>
            {
                entity.ToTable("Pag_Rendiconto", Schema, t =>
                {
                    t.HasCheckConstraint("CK_Pag_Rendiconto_Mese", "\"Mese\" BETWEEN 1 AND 12");
                    t.HasCheckConstraint("CK_Pag_Rendiconto_Percentuale", "\"PercentualePiattaforma\" BETWEEN 0 AND 100");
                });
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotaleIncassato).HasPrecision(12, 2);
                entity.Property(e => e.TotaleRettifiche).HasPrecision(12, 2);
                entity.Property(e => e.QuotaPiattaforma).HasPrecision(12, 2);
                entity.Property(e => e.QuotaCentro).HasPrecision(12, 2);
                entity.Property(e => e.PercentualePiattaforma).HasPrecision(5, 2);
                // un solo rendiconto valido per centro e mese
                entity.HasIndex(e => new { e.IdDealer, e.Anno, e.Mese }).IsUnique().HasFilter("\"Stato\" <> 3");
                entity.HasIndex(e => new { e.Anno, e.Mese });
            });

            modelBuilder.Entity<Pag_DealerConfig>(entity =>
            {
                entity.ToTable("Pag_DealerConfig", Schema, t =>
                    t.HasCheckConstraint("CK_Pag_DealerConfig_Percentuale", "\"PercentualePiattaforma\" IS NULL OR \"PercentualePiattaforma\" BETWEEN 0 AND 100"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PercentualePiattaforma).HasPrecision(5, 2);
                entity.HasIndex(e => e.IdDealer).IsUnique();
                entity.HasOne(e => e.DealerNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdDealer)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Pag_InvioEmail>(entity =>
            {
                entity.ToTable("Pag_InvioEmail", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdDealer, e.Anno, e.Mese, e.Tipo });
            });

            modelBuilder.Entity<AppSetting>().HasData(
                new AppSetting { Key = Key_PagamentiPercentualePiattaforma, Value = "0" },
                new AppSetting { Key = Key_PagamentiRiepilogoAutomatico, Value = "false" },
                new AppSetting { Key = Key_PagamentiGiornoInvioMensile, Value = "5" },
                new AppSetting { Key = Key_PagamentiMeseInvioAnnuale, Value = "1" });
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
        /*
         Spazio dei file delle aziende (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md [branch DocumentManagement] §5.3).
         La quota e' quella della licenza Documentale (ANALISI_INFRASTRUCTURE_LICENZE.md §5.1): i pacchetti di spazio e la dimensione
         massima del file sono stati eliminati. Dato iniziale: soglia di avviso all'80%.
        */
        public const string Key_StorageWarningPercent = "Storage_WarningPercent";

        private void Gen_Storage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyStorage>(entity =>
            {
                entity.ToTable("CompanyStorage", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdCompany).IsUnique();
                entity.HasOne(e => e.CompanyNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdCompany)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // storico mensile: nessuna chiave esterna, resta anche dopo la cancellazione dell'azienda (fatturazione)
            modelBuilder.Entity<CompanyStorageHistory>(entity =>
            {
                entity.ToTable("CompanyStorageHistory", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdCompany, e.Anno, e.Mese }).IsUnique();
            });

            modelBuilder.Entity<AppSetting>().HasData(
                new AppSetting { Key = Key_StorageWarningPercent, Value = "80" });
        }

        // applicativi abilitati per studio (funzioni lato studio)
        private void Gen_FinancialAdvisorApplication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialAdvisorApplication>(entity =>
            {
                entity.ToTable("FinancialAdvisorApplication", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdFinancialAdvisor, e.ApplicationType }).IsUnique();
                entity.HasOne(e => e.FinancialAdvisorNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdFinancialAdvisor)
                      .OnDelete(DeleteBehavior.Cascade);
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
        // dashboard degli utenti: una riga per utente/azienda, cancellata con l'utente o con l'azienda
        private void Gen_UserDashboard(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDashboard>(entity =>
            {
                entity.ToTable("UserDashboard", Schema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdAspNetUsers, e.IdCompany }).IsUnique();

                // chiavi esterne senza navigation (vedi CompanyMenuRestriction)
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.IdAspNetUsers).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Company>().WithMany().HasForeignKey(e => e.IdCompany).OnDelete(DeleteBehavior.Cascade);
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
