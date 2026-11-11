using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using static nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed.LicenseCatalogSeedData;

namespace nvxapp.server.service.ClientServer_Service.Compliance.Initializer.LicenseCatalog
{
    // Listini iniziali di Compliance, con la stessa struttura del CRM: uno per gli studi (non esteso alle loro aziende) e uno per le
    // aziende che si abbonano da sole. Nessun limite (LimiteFunzione = null per il modulo, INF-APP.3).
    // PREZZI SEGNAPOSTO: livelli e prezzi sono da decidere con i listini (D8 / ANA.10, ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md).
    // Creati una sola volta all'avvio da LicenseCatalogSeeder (Infrastructure).
    public class CmpLicenseCatalogSeed : ILicenseCatalogSeed
    {
        public string Name => "COMPLIANCE";

        public IEnumerable<Lic_Listino> Listini()
        {
            yield return Listino(TipoSoggettoLicenza.FinancialAdvisor, AmbitoLicenza.Compliance, "Piano base Compliance", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0),
                Piano(LivelloLicenza.Standard, "Standard", null, 20.00m, 200.00m),
                Piano(LivelloLicenza.Pro, "Pro", null, 40.00m, 400.00m));

            // per le aziende (sottoscrizione in autonomia): stessi piani dello studio con 5 euro in piu' su Standard e Pro
            yield return Listino(TipoSoggettoLicenza.Company, AmbitoLicenza.Compliance, "Piano base Compliance aziende", disponibileSuCompany: false,
                Piano(LivelloLicenza.Free, "Free", null, 0, 0),
                Piano(LivelloLicenza.Standard, "Standard", null, 25.00m, 205.00m),
                Piano(LivelloLicenza.Pro, "Pro", null, 45.00m, 405.00m));
        }
    }
}
