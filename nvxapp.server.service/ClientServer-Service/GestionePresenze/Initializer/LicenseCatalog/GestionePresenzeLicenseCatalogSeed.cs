using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Initializer.LicenseCatalog
{
    // Listino iniziale delle Presenze (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): per gli studi, valido anche per le loro aziende.
    // Creato una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class GestionePresenzeLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "Presenze";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.AttendanceTracking, "Piano base presenze", disponibileSuCompany: true,
                Piano(LivelloLicenza.Free, "Free", "Piano Base", 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 50.00m, 500.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 100.00m, 999.95m, null));
        }
    }
}
