using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common
{
    /*
     Uso delle tabelle comuni (schema Common) da parte di un applicativo, per bloccare con un
     messaggio chiaro la cancellazione di un record ancora usato (le chiavi esterne tra schemi la
     bloccherebbero comunque, ma con un errore tecnico).
     Ogni applicativo con tabelle che riferiscono quelle comuni ne registra un'implementazione
     (registrazione automatica dei servizi). Viene chiamata solo se l'azienda ha usato l'applicativo
     (riga in CompanyApplication, attiva o no): lo schema da usare lo sceglie l'implementazione,
     come per IApplicationDataDeleter.
    */
    public interface ICommonEntityUsage
    {
        ApplicationType Application { get; }

        /// <summary>Usi del cliente nell'applicativo, gia' tradotti (es. "Commesse: 3"); vuoto se non e' usato.</summary>
        Task<List<string>> Az_ClienteUsageAsync(int idCompany, int idAz_Cliente, CancellationToken cancellationToken = default);
    }
}
