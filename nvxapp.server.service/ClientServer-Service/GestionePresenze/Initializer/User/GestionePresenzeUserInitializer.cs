using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User;


namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Initializer.User
{
    // Inizializzatore per il dominio GestionePresenze.
    // Crea le strutture dati tenant necessarie per un nuovo utente:
    // - Dip_Anagrafica (anagrafica dipendente collegata all'IdentityUser)
    // - Dip_RapportoLavoro (rapporto di lavoro con commessa/attività default)
    // Priority=20: eseguito dopo Infrastructure per avere le strutture
    // infrastrutturali pronte.
    // Per aggiungere nuove strutture (es. profilo orario default) basta
    // estendere questo metodo senza toccare AccountService.
    public class GestionePresenzeUserInitializer : IUserInitializer
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly ILogger<GestionePresenzeUserInitializer> _logger;

        public string Name => "GestionePresenze";
        public int Priority => 20;

        public GestionePresenzeUserInitializer(
            IGestionePresenzeUserUtility gestionePresenzeUserUtility,
            ILogger<GestionePresenzeUserInitializer> logger)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(UserCompany userCompany)
        {
            _logger.LogInformation(
                "[GestionePresenzeUserInitializer] Inizializzazione GestionePresenze " +
                "per utente {UserId}, Company={CompanyId}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany);

            // 1. Inizializzazione strutture azienda (Az_Anagrafica, Sedi, Reparto, Cfg,
            //    Competenze, Attività, Cliente, Commessa, SubCommessa, ecc.)
            //    Il metodo Get_AzAna_AzSedi_AzReparto_Az_Cfg con InitIfNotExsist=true
            //    si occupa di creare tutto il necessario se è la prima volta per l'azienda.
            var companyData = await _gestionePresenzeUserUtility
                .Get_AzAna_AzSedi_AzReparto_Az_Cfg(userCompany.IdCompany, InitIfNotExsist: true);

            _logger.LogInformation(
                "[GestionePresenzeUserInitializer] Strutture azienda pronte. " +
                "Az_AnagraficaId={AzAnaId}, SedeId={SedeId}, RepartoId={RepartoId}",
                companyData.az_Anagrafica?.Id,
                companyData.az_Sedi?.Id,
                companyData.az_SediReparto?.Id);

            // 2. Inizializzazione strutture utente (Dip_Anagrafica + Dip_RapportoLavoro)
            //    Il metodo Get_DipAna_DipRapp con InitIfNotExsist=true crea:
            //    - Dip_Anagrafica collegata all'IdAspNetUsers
            //    - Dip_RapportoLavoro con commessa/attività default e data assunzione
            var userData = await _gestionePresenzeUserUtility
                .Get_DipAna_DipRapp(userCompany.IdAspNetUsers, InitIfNotExsist: true);

            _logger.LogInformation(
                "[GestionePresenzeUserInitializer] Strutture utente pronte. " +
                "Dip_AnagraficaId={DipAnaId}, Dip_RapportoLavoroId={DipRappId}, " +
                "SubCommessaAttivitaId={SubCommAttId}",
                userData.dip_Anagrafica?.Id,
                userData.dip_RapportoLavoro?.Id,
                userData.dip_RapportoLavoro?.IdAz_SubCommessaAttivita);

            _logger.LogInformation(
                "[GestionePresenzeUserInitializer] Completato per utente {UserId}.",
                userCompany.IdAspNetUsers);
        }
    }
}