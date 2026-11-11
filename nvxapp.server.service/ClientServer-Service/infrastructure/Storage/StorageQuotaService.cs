using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage
{
    /*
     Spazio a disposizione delle aziende (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §5.3), uso interno degli applicativi.
     Conta solo i file del documentale; spazio solo per azienda; oltre quota si bloccano i caricamenti.

     Caricamento di un file:
        1. ReserveAsync  : PRENOTA lo spazio con una sola istruzione atomica
                           (due caricamenti contemporanei non superano mai la quota)
        2. salvataggio del file e dei metadati
        3. ConfirmAsync  (riuscito) oppure ReleaseAsync (errore)
     Eliminazione definitiva: FreeAsync. Di notte RecalculateAsync riallinea con la somma reale.
     Quota (ANALISI_LICENZE.md §5.1, §15.7): quella della licenza Documentale (ILicenseGuard.StorageQuotaAsync).
        - sistema licenze spento           : spazio illimitato
        - sistema acceso, azienda coperta  : quota della licenza
        - sistema acceso, nessuna copertura: quota zero, caricamenti bloccati (StorageNoLicense). Il Documentale senza
          licenza e' solo nascosto dal menu e le sue API non controllano la licenza: senza questo blocco si caricherebbe senza limite.
     Non c'e' un limite sulla dimensione del singolo file: conta solo lo spazio totale. I pacchetti di spazio (StoragePlan) sono stati eliminati.
    */
    public interface IStorageQuotaService
    {
        /// <summary>Riga dello spazio dell'azienda, creata se manca.</summary>
        Task<CompanyStorage> EnsureAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>Spazio dell'azienda con i limiti calcolati.</summary>
        Task<StorageStatus> StatusAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>Prenota lo spazio per un file; UserMessageException se manca la licenza Documentale o lo spazio non basta.</summary>
        Task ReserveAsync(int idCompany, long bytes, CancellationToken cancellationToken = default);

        /// <summary>Il file e' salvato: lo spazio prenotato diventa occupato; avvisi alla soglia e al 100%.</summary>
        Task ConfirmAsync(int idCompany, long bytes, CancellationToken cancellationToken = default);

        /// <summary>Caricamento non riuscito: annulla la prenotazione.</summary>
        Task ReleaseAsync(int idCompany, long bytes, CancellationToken cancellationToken = default);

        /// <summary>File eliminati definitivamente: libera lo spazio.</summary>
        Task FreeAsync(int idCompany, long bytes, int files = 1, CancellationToken cancellationToken = default);

        /// <summary>Riallinea lo spazio occupato con quello reale e azzera le prenotazioni rimaste appese.</summary>
        Task RecalculateAsync(int idCompany, long usedBytes, int fileCount, CancellationToken cancellationToken = default);
    }

    public sealed class StorageStatus
    {
        public int IdCompany { get; init; }
        // descrizione della quota: piano della licenza Documentale, "illimitato" (licenze spente) o "senza licenza"
        public string Pacchetto { get; init; } = string.Empty;
        public long QuotaBytes { get; init; }
        public bool Illimitato { get; init; }
        // sistema licenze acceso e azienda non coperta per il Documentale: quota zero, caricamenti bloccati
        public bool SenzaLicenza { get; init; }
        public long UsedBytes { get; init; }
        public long ReservedBytes { get; init; }
        public int FileCount { get; init; }
        public int WarningPercent { get; init; }
        public int QuotaMB => (int)Math.Min(int.MaxValue, QuotaBytes / (1024L * 1024L));
        public long AvailableBytes => Math.Max(0, QuotaBytes - UsedBytes - ReservedBytes);
        public int UsedPercent => QuotaBytes <= 0 ? 0 : (int)Math.Min(999, UsedBytes * 100 / QuotaBytes);
    }

    public class StorageQuotaService : IStorageQuotaService
    {
        private const long MB = 1024L * 1024L;

        private readonly ICompanyStorageRepository _companyStorageRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IUserDealerRepository _userDealerRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<StorageQuotaService> _logger;
        private readonly ILicenseGuard _licenseGuard;

        public StorageQuotaService(ICompanyStorageRepository companyStorageRepository,
                                   IAppSettingRepository appSettingRepository,
                                   ICompanyRepository companyRepository,
                                   IUserCompanyRepository userCompanyRepository,
                                   IUserDealerRepository userDealerRepository,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IAspNetRolesRepository aspNetRolesRepository,
                                   IAspNetUserRolesRepository aspNetUserRolesRepository,
                                   IEmailService emailService,
                                   ILogger<StorageQuotaService> logger,
                                   ILicenseGuard licenseGuard)
        {
            _licenseGuard = licenseGuard;
            _companyStorageRepository = companyStorageRepository;
            _appSettingRepository = appSettingRepository;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            _userDealerRepository = userDealerRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<CompanyStorage> EnsureAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            var row = await _companyStorageRepository.FindAll(x => x.IdCompany == idCompany).FirstOrDefaultAsync(cancellationToken);
            if (row != null)
                return row;

            // inserimento idempotente (due richieste contemporanee per la stessa azienda)
            await _companyStorageRepository.InsertIfMissingAsync(idCompany, cancellationToken);

            return await _companyStorageRepository.FindAll(x => x.IdCompany == idCompany).FirstAsync(cancellationToken);
        }

        public async Task<StorageStatus> StatusAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            var row = await EnsureAsync(idCompany, cancellationToken);
            // quota della licenza del documentale; licenze spente = illimitato; licenze accese senza copertura = zero
            var licenza = await _licenseGuard.StorageQuotaAsync(idCompany);
            bool senzaLicenza = licenza == null && await _licenseGuard.EnabledAsync();
            long quota = licenza?.QuotaBytes ?? (senzaLicenza ? 0 : LicenseGuard.SpazioIllimitatoBytes);
            return new StorageStatus
            {
                IdCompany = idCompany,
                Pacchetto = licenza != null ? InfrastructureMessages.Get("StorageLicensePlan", licenza.Descrizione)
                          : senzaLicenza ? InfrastructureMessages.Get("StorageNoLicensePlan")
                          : InfrastructureMessages.Get("StorageUnlimited"),
                QuotaBytes = quota,
                Illimitato = quota >= LicenseGuard.SpazioIllimitatoBytes,
                SenzaLicenza = senzaLicenza,
                UsedBytes = row.UsedBytes,
                ReservedBytes = row.ReservedBytes,
                FileCount = row.FileCount,
                WarningPercent = row.WarningPercent ?? await SettingAsync(PublicDbContext.Key_StorageWarningPercent, 80, cancellationToken),
            };
        }

        public async Task ReserveAsync(int idCompany, long bytes, CancellationToken cancellationToken = default)
        {
            var status = await StatusAsync(idCompany, cancellationToken);
            if (status.SenzaLicenza)
                throw new UserMessageException(InfrastructureMessages.Get("StorageNoLicense"));
            // controllo e prenotazione nella stessa istruzione atomica, con la quota calcolata (licenza o illimitato)
            if (!await _companyStorageRepository.ReserveAsync(idCompany, bytes, status.QuotaBytes, cancellationToken))
                throw new UserMessageException(InfrastructureMessages.Get("StorageQuotaExceeded", Mb(bytes), Mb(status.AvailableBytes), Mb(status.QuotaBytes)));
        }

        public async Task ConfirmAsync(int idCompany, long bytes, CancellationToken cancellationToken = default)
        {
            await _companyStorageRepository.ConfirmAsync(idCompany, bytes, cancellationToken);

            await CheckWarningsAsync(idCompany, cancellationToken);
        }

        public async Task ReleaseAsync(int idCompany, long bytes, CancellationToken cancellationToken = default)
        {
            await _companyStorageRepository.ReleaseAsync(idCompany, bytes, cancellationToken);
        }

        public async Task FreeAsync(int idCompany, long bytes, int files = 1, CancellationToken cancellationToken = default)
        {
            await _companyStorageRepository.FreeAsync(idCompany, bytes, files, cancellationToken);

            await CheckWarningsAsync(idCompany, cancellationToken);
        }

        public async Task RecalculateAsync(int idCompany, long usedBytes, int fileCount, CancellationToken cancellationToken = default)
        {
            var row = await EnsureAsync(idCompany, cancellationToken);
            if (row.UsedBytes != usedBytes || row.FileCount != fileCount || row.ReservedBytes != 0)
                _logger.LogWarning("[Storage] Azienda {IdCompany}: riallineato lo spazio ({OldBytes} -> {NewBytes} byte, {OldFiles} -> {NewFiles} file, prenotati {Reserved}).",
                                   idCompany, row.UsedBytes, usedBytes, row.FileCount, fileCount, row.ReservedBytes);

            await _companyStorageRepository.RecalculateAsync(idCompany, usedBytes, fileCount, cancellationToken);

            await CheckWarningsAsync(idCompany, cancellationToken);
        }

        /*
         Avviso alla soglia (80% predefinito) e al 100%, al massimo una volta per soglia finche' lo spazio non scende:
         email al CompanyPowerAdmin dell'azienda e al DealerPowerAdmin del centro (se l'email e' configurata) e log.
        */
        private async Task CheckWarningsAsync(int idCompany, CancellationToken cancellationToken)
        {
            var status = await StatusAsync(idCompany, cancellationToken);
            var row = await _companyStorageRepository.FindAll(x => x.IdCompany == idCompany).FirstAsync(cancellationToken);

            // senza licenza Documentale non si carica niente: nessun avviso di soglia
            int livello = status.SenzaLicenza ? 0 : status.UsedPercent >= 100 ? 100 : status.UsedPercent >= status.WarningPercent ? status.WarningPercent : 0;
            if (livello == row.LastWarningPercent || (livello > 0 && livello < row.LastWarningPercent))
            {
                // stesso livello gia' avvisato, oppure in discesa ma ancora sopra una soglia: si aggiorna solo il livello
                if (livello < row.LastWarningPercent)
                {
                    row.LastWarningPercent = livello;
                    await _companyStorageRepository.UpdateAsync(row);
                }
                return;
            }

            row.LastWarningPercent = livello;
            if (livello > 0)
                row.LastWarningDate = DateTime.Now;
            await _companyStorageRepository.UpdateAsync(row);

            if (livello == 0)
                return;

            var company = await _companyRepository.FindAll(x => x.Id == idCompany).Include(x => x.FinancialAdvisorNavigation).FirstOrDefaultAsync(cancellationToken);
            _logger.LogWarning("[Storage] Azienda {IdCompany} ({Company}): spazio al {Percent}% ({Used} di {Quota} MB).",
                               idCompany, company?.Descrizione, status.UsedPercent, Mb(status.UsedBytes), Mb(status.QuotaBytes));

            if (!_emailService.Configured || company == null)
                return;

            var destinatari = await EmailRuoloAsync(nameof(RoleCode.CompanyPowerAdmin),
                                                    _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).Select(x => x.IdAspNetUsers), cancellationToken);
            if (company.FinancialAdvisorNavigation != null)
                destinatari.AddRange(await EmailRuoloAsync(nameof(RoleCode.DealerPowerAdmin),
                                                           _userDealerRepository.FindAll(x => x.IdDealer == company.FinancialAdvisorNavigation.IdDealer && !x.Disabled).Select(x => x.IdAspNetUsers), cancellationToken));

            var oggetto = InfrastructureMessages.Get("StorageWarningSubject", company.Descrizione, status.UsedPercent);
            var testo = InfrastructureMessages.Get("StorageWarningText", company.Descrizione, status.UsedPercent, Mb(status.UsedBytes), Mb(status.QuotaBytes), status.Pacchetto);
            foreach (var email in destinatari.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    await _emailService.SendAsync(email, oggetto, System.Net.WebUtility.HtmlEncode(testo), testo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Storage] Avviso spazio non inviato a {Email}.", email);
                }
            }
        }

        private async Task<List<string>> EmailRuoloAsync(string ruolo, IQueryable<string> utenti, CancellationToken cancellationToken)
        {
            var idRuolo = await _aspNetRolesRepository.FindAll(r => r.Name == ruolo).Select(r => r.Id).FirstOrDefaultAsync(cancellationToken);
            if (idRuolo == null)
                return new List<string>();
            var conRuolo = _aspNetUserRolesRepository.FindAll(r => r.RoleId == idRuolo).Select(r => r.UserId);
            return await _aspNetUsersRepository.FindAll(u => utenti.Contains(u.Id) && conRuolo.Contains(u.Id) && u.Email != null && u.Email != "")
                                               .Select(u => u.Email!)
                                               .ToListAsync(cancellationToken);
        }

        private async Task<int> SettingAsync(string key, int defaultValue, CancellationToken cancellationToken)
        {
            var value = await _appSettingRepository.GetValueAsync(key, cancellationToken);
            return int.TryParse(value, out var n) && n > 0 ? n : defaultValue;
        }

        private static string Mb(long bytes) => (bytes / (double)MB).ToString("0.#");
    }
}
