using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Initializer.LicenseCatalog
{
    // Listino iniziale del Documentale (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): per gli studi, valido anche per le loro aziende.
    // Creato una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class DocumentManagementLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "Documentale";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.DocumentManagement, "Piano base documentale", disponibileSuCompany: true,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 30.00m, 300.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 60.00m, 600.14m, null));
        }
    }
}
