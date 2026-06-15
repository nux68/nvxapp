using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit
{
    // Contratto base per tutti gli inizializzatori di azienda.
    // Ogni inizializzatore è responsabile di creare le strutture dati
    // necessarie per una nuova azienda nel proprio dominio.
    // CompanyInitializerRegistry si auto-costruisce a runtime
    // dall'IEnumerable<ICompanyInitializer> iniettato dalla DI.
    // Per aggiungere un nuovo inizializzatore basta creare una classe
    // che implementa ICompanyInitializer e registrarla nella DI.
    public interface ICompanyInitializer
    {
        // Nome identificativo dell'inizializzatore (per logging e diagnostica)
        string Name { get; }

        // Priorità di esecuzione (numeri più bassi = eseguiti prima).
        // Esempio: Infrastructure=10, GestionePresenze=20.
        int Priority { get; }

        // Esegue l'inizializzazione per la nuova azienda.
        // Riceve la Company appena creata con Id popolato.
        Task InitializeAsync(Company company);
    }
}