using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed
{
    /*
     Listini iniziali delle licenze (ANALISI_LICENZE.md §15.5): ogni ramo dichiara i propri.
        - Infrastructure: il listino Cumulativo (InfrastructureLicenseCatalogSeed)
        - ogni ramo applicativo: il listino della propria funzione (Presenze, CRM, Documentale), nella propria cartella
     Basta una classe che implementa ILicenseCatalogSeed: la registra lo scanner dei servizi e LicenseCatalogSeeder
     la esegue all'avvio, dopo le migration. Sono listini globali (del SuperUser, IdDealer null).
    */
    public interface ILicenseCatalogSeed
    {
        /// <summary>Nome per il log (es. "Presenze").</summary>
        string Name { get; }

        /// <summary>Listini iniziali, ognuno con i suoi tre piani (Free, Standard, Pro).</summary>
        IEnumerable<Lic_Listino> Listini();
    }

    /// <summary>Costruzione dei listini iniziali, per scriverli in modo compatto nei rami.</summary>
    public static class LicenseCatalogSeedData
    {
        public static Lic_Listino Listino(TipoSoggettoLicenza tipoSoggetto, AmbitoLicenza ambito, string descrizione, bool disponibileSuCompany,
                                          params Lic_Piano[] piani)
            => new()
            {
                IdDealer = null,
                TipoSoggetto = tipoSoggetto,
                Ambito = ambito,
                Descrizione = descrizione,
                Valuta = "EUR",
                DisponibileSuCompany = tipoSoggetto == TipoSoggettoLicenza.FinancialAdvisor && disponibileSuCompany,
                Attivo = true,
                Lic_Piano = piani.ToList()
            };

        /// <summary>Piano del listino; maxUtenti null = illimitati.</summary>
        public static Lic_Piano Piano(LivelloLicenza livello, string codice, string? descrizione, decimal prezzoMensile, decimal prezzoAnnuale, int? maxUtenti)
            => new()
            {
                Livello = livello,
                Codice = codice,
                Descrizione = descrizione,
                Ordine = (int)livello * 10,
                PrezzoMensile = livello == LivelloLicenza.Free ? 0 : prezzoMensile,
                PrezzoAnnuale = livello == LivelloLicenza.Free ? 0 : prezzoAnnuale,
                MaxUtenti = maxUtenti,
                Attivo = true
            };
    }
}
