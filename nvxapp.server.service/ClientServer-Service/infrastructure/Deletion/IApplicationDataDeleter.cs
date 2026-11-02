using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion
{
    /*
     Dati di un applicativo nella cancellazione fisica (DeletionService). Ogni applicativo con dati legati ad aziende
     o utenti ne registra una implementazione (registrazione automatica dei servizi: basta la classe nel modulo).
     Lo schema da usare (tenant_<IdAzienda>_<App> o public) lo sceglie l'implementazione, con TenancySettings e
     IApplicationDbContextFactory. Le chiamate avvengono prima della transazione di DeletionService (contesti e
     connessioni diversi): ogni metodo deve poter essere ripetuto trovando i dati gia' eliminati.
    */
    public interface IApplicationDataDeleter
    {
        ApplicationType Application { get; }

        /// <summary>Conteggi per l'anteprima dell'eliminazione dell'azienda (es. "Dipendenti: 12"), nella lingua dell'utente.</summary>
        Task<List<DeletionCountModel>> CompanyCountsAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina i dati dell'azienda nell'applicativo in modalita' singola (tabelle in public).
        /// In multi-tenant non viene chiamato: DeletionService elimina lo schema dell'azienda.
        /// </summary>
        Task DeleteCompanyDataAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina i dati di un utente nell'azienda (es. il dipendente con presenze e richieste), quando l'utente viene
        /// tolto dall'azienda o eliminato. Non viene chiamato per un'azienda che si sta eliminando.
        /// </summary>
        Task DeleteUserDataAsync(int idCompany, string idAspNetUsers, CancellationToken cancellationToken = default);
    }
}
