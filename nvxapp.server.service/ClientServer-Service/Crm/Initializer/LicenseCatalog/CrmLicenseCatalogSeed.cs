using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.Crm.Initializer.LicenseCatalog
{
    // Listini iniziali del CRM (ANALISI_LICENZE.md §15.5 [branch Infrastructure]): uno per gli studi (non esteso alle loro
    // aziende) e uno per le aziende che si abbonano da sole. Creati una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class CrmLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "CRM";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.Crm, "Piano base CRM", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, null, maxClienti: 100),
                Piano(LivelloLicenza.Standard, "Standard", null, 20.00m, 200.00m, null, maxClienti: 1000),
                Piano(LivelloLicenza.Pro, "Pro", null, 40.00m, 400.00m, null, maxClienti: null));

            // per le aziende (sottoscrizione in autonomia): stessi piani dello studio con 5 euro in piu' su Standard e Pro
            yield return Listino(TipoSoggettoLicenza.Company, AmbitoLicenza.Crm, "Piano base CRM aziende", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0, null, maxClienti: 100),
                Piano(LivelloLicenza.Standard, "Standard", null, 25.00m, 205.00m, null, maxClienti: 1000),
                Piano(LivelloLicenza.Pro, "Pro", null, 45.00m, 405.00m, null, maxClienti: null));
        }
    }
}
