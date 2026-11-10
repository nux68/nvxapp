using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.Crm.Initializer.LicenseCatalog
{
    // Listino iniziale del CRM (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): per gli studi, non esteso alle loro aziende.
    // Creato una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class CrmLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "CRM";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.Crm, "Piano base CRM", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, 10),
                Piano(LivelloLicenza.Standard, "Standard", null, 20.00m, 200.00m, 50),
                Piano(LivelloLicenza.Pro, "Pro", null, 40.00m, 400.00m, null));
        }
    }
}
