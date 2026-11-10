using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User
{
    // Inizializzatore per il dominio Infrastructure.
    // Gestisce le strutture dati infrastrutturali necessarie
    // per un nuovo utente, comuni a tutti gli applicativi:
    // - Dip_Anagrafica (anagrafica dipendente comune, con Cognome/Nome iniziali):
    //   inizializzazione unica in ICommonDipendenteUtility, gli applicativi aggiungono poi i propri dati
    // Priority=10: eseguito prima di GestionePresenze.
    public class InfrastructureUserInitializer : IUserInitializer
    {
        private readonly ICommonDipendenteUtility _commonDipendenteUtility;
        private readonly ILogger<InfrastructureUserInitializer> _logger;

        public string Name => "Infrastructure";
        public int Priority => 10;
        public ApplicationType? Application => null; // comune a tutti gli applicativi

        public InfrastructureUserInitializer(ICommonDipendenteUtility commonDipendenteUtility,
                                             ILogger<InfrastructureUserInitializer> logger)
        {
            _commonDipendenteUtility = commonDipendenteUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(UserCompany userCompany)
        {
            _logger.LogInformation(
                "[InfrastructureUserInitializer] Inizializzazione infrastructure " +
                "per utente {UserId}, Company={CompanyId}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany);

            // Anagrafica dipendente comune (creata se manca, Cognome/Nome completati se provvisori)
            var dip_Anagrafica = await _commonDipendenteUtility.GetOrCreateDip_AnagraficaAsync(userCompany.IdCompany, userCompany.IdAspNetUsers);

            _logger.LogInformation(
                "[InfrastructureUserInitializer] Completato per utente {UserId}. Dip_AnagraficaId={DipAnaId}",
                userCompany.IdAspNetUsers,
                dip_Anagrafica.Id);
        }
    }
}
