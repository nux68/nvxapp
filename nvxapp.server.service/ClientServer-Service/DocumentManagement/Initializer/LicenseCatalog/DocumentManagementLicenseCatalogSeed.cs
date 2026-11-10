using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Initializer.LicenseCatalog
{
    // Listini iniziali del Documentale (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): uno per gli studi (valido anche per le
    // loro aziende) e uno per le aziende che si abbonano da sole. Creati una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class DocumentManagementLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "Documentale";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.DocumentManagement, "Piano base documentale", disponibileSuCompany: true,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, null, maxSpazioMB: 1024),
                Piano(LivelloLicenza.Standard, "Standard", null, 30.00m, 300.00m, null, maxSpazioMB: 5120),
                Piano(LivelloLicenza.Pro, "Pro", null, 60.00m, 600.14m, null, maxSpazioMB: 20480));

            // per le aziende (sottoscrizione in autonomia): stessi piani dello studio con 5 euro in piu' su Standard e Pro
            yield return Listino(TipoSoggettoLicenza.Company, AmbitoLicenza.DocumentManagement, "Piano base documentale aziende", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, null, maxSpazioMB: 1024),
                Piano(LivelloLicenza.Standard, "Standard", null, 35.00m, 305.00m, null, maxSpazioMB: 5120),
                Piano(LivelloLicenza.Pro, "Pro", null, 65.00m, 605.14m, null, maxSpazioMB: 20480));
        }
    }
}
