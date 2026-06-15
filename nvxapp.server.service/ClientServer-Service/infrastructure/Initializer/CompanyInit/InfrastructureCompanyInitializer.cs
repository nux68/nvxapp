using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit
{
    // Inizializzatore per il dominio Infrastructure (azienda).
    // Gestisce le strutture dati infrastrutturali necessarie
    // per una nuova azienda (es. configurazioni, tenant settings, notifiche).
    // Priority=10: eseguito prima di GestionePresenze.
    public class InfrastructureCompanyInitializer : ICompanyInitializer
    {
        private readonly ILogger<InfrastructureCompanyInitializer> _logger;

        public string Name => "Infrastructure";
        public int Priority => 10;

        public InfrastructureCompanyInitializer(ILogger<InfrastructureCompanyInitializer> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            _logger.LogInformation(
                "[InfrastructureCompanyInitializer] Inizializzazione infrastructure " +
                "per azienda {CompanyId}, Descrizione={Descrizione}",
                company.Id,
                company.Descrizione);

            // TODO: In futuro qui si potranno inizializzare:
            // - Configurazioni tenant di default
            // - Sottoscrizioni notifiche aziendali
            // - Template e configurazioni personali

            await Task.CompletedTask;

            _logger.LogInformation(
                "[InfrastructureCompanyInitializer] Completato per azienda {CompanyId}.",
                company.Id);
        }
    }
}