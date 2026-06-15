using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Initializer.CompanyInit
{
    // Inizializzatore per il dominio GestionePresenze (azienda).
    // Crea le strutture dati tenant necessarie per una nuova azienda:
    // - Az_Anagrafica (anagrafica azienda)
    // - Az_Sedi (sede default)
    // - Az_Reparto (reparto default)
    // - Az_Cfg (configurazioni di default)
    // - Competenze, Attività, Cliente, Commessa, SubCommessa di default
    // Priority=20: eseguito dopo Infrastructure per avere le strutture
    // infrastrutturali pronte.
    public class GestionePresenzeCompanyInitializer : ICompanyInitializer
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly ILogger<GestionePresenzeCompanyInitializer> _logger;

        public string Name => "GestionePresenze";
        public int Priority => 20;

        public GestionePresenzeCompanyInitializer(
            IGestionePresenzeUserUtility gestionePresenzeUserUtility,
            ILogger<GestionePresenzeCompanyInitializer> logger)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            _logger.LogInformation(
                "[GestionePresenzeCompanyInitializer] Inizializzazione GestionePresenze " +
                "per azienda {CompanyId}, Descrizione={Descrizione}",
                company.Id,
                company.Descrizione);

            // Inizializzazione strutture azienda (Az_Anagrafica, Sedi, Reparto, Cfg,
            // Competenze, Attività, Cliente, Commessa, SubCommessa, ecc.)
            // Il metodo Get_AzAna_AzSedi_AzReparto_Az_Cfg con InitIfNotExsist=true
            // si occupa di creare tutto il necessario se è la prima volta.
            var companyData = await _gestionePresenzeUserUtility
                .Get_AzAna_AzSedi_AzReparto_Az_Cfg(company.Id, InitIfNotExsist: true);

            _logger.LogInformation(
                "[GestionePresenzeCompanyInitializer] Strutture azienda pronte. " +
                "Az_AnagraficaId={AzAnaId}, SedeId={SedeId}, RepartoId={RepartoId}",
                companyData.az_Anagrafica?.Id,
                companyData.az_Sedi?.Id,
                companyData.az_SediReparto?.Id);

            _logger.LogInformation(
                "[GestionePresenzeCompanyInitializer] Completato per azienda {CompanyId}.",
                company.Id);
        }
    }
}