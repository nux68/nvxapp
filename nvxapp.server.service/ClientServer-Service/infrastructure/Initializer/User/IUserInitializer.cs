using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User
{
    // Contratto base per tutti gli inizializzatori utente.
    // Ogni inizializzatore è responsabile di creare le strutture dati
    // necessarie per un nuovo utente nel proprio dominio.
    // UserInitializerRegistry si auto-costruisce a runtime
    // dall'IEnumerable<IUserInitializer> iniettato dalla DI.
    // Per aggiungere un nuovo inizializzatore basta creare una classe
    // che implementa IUserInitializer e registrarla nella DI.
    public interface IUserInitializer
    {
        // Nome identificativo dell'inizializzatore (per logging e diagnostica)
        string Name { get; }

        // Priorità di esecuzione (numeri più bassi = eseguiti prima).
        // Esempio: Infrastructure=10, GestionePresenze=20.
        int Priority { get; }

        // Esegue l'inizializzazione per il nuovo utente.
        // Riceve lo UserCompany appena creato con IdAspNetUsers e IdCompany popolati.
        Task InitializeAsync(UserCompany userCompany);
    }
}