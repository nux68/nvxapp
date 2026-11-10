using nvxapp.server.data.Entities.Public;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed
{
    // Listino iniziale di Infrastructure: il Cumulativo (tutte le funzioni), per gli studi, valido anche per le loro aziende.
    // I listini delle singole funzioni li dichiarano i rami applicativi.
    public class InfrastructureLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "Infrastructure";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.Cumulativo, "Piano base ALL", disponibileSuCompany: true,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 80.00m, 800.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 160.00m, 1600.00m, null));
        }
    }
}
