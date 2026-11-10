using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Initializer.LicenseCatalog
{
    // Listini iniziali delle Presenze (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): uno per gli studi (valido anche per le
    // loro aziende) e uno per le aziende che si abbonano da sole. Creati una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class GestionePresenzeLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "Presenze";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.AttendanceTracking, "Piano base presenze", disponibileSuCompany: true,
                Piano(LivelloLicenza.Free, "Free", "Piano Base", 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 50.00m, 500.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 100.00m, 999.95m, null));

            // per le aziende (sottoscrizione in autonomia): stessi piani dello studio con 5 euro in piu' su Standard e Pro
            yield return Listino(TipoSoggettoLicenza.Company, AmbitoLicenza.AttendanceTracking, "Piano base presenze aziende", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", "Piano Base", 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 55.00m, 505.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 105.00m, 1004.95m, null));
        }
    }
}
