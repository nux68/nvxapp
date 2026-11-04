using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage
{
    /*
     Spazio reale occupato dai file di un applicativo per un'azienda, per il riallineamento notturno dello spazio
     (StorageMaintenanceService). Lo implementa l'applicativo che salva file con IFileStorage (il documentale);
     lo schema da usare lo sceglie l'implementazione (come IApplicationDataDeleter). Viene chiamato solo per le
     aziende che hanno usato l'applicativo (riga in CompanyApplication).
    */
    public interface IStorageUsageProvider
    {
        ApplicationType Application { get; }

        /// <summary>Byte e numero di file realmente occupati dall'azienda nell'applicativo.</summary>
        Task<(long Bytes, int Files)> UsageAsync(int idCompany, CancellationToken cancellationToken = default);
    }
}
