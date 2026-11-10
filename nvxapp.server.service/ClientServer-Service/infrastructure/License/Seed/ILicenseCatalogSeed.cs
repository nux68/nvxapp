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
        /// <summary>
        /// Listino con i suoi piani; i limiti dei piani sono i valori predefiniti della funzione del listino (LicenseDefaults):
        /// Presenze utenti, CRM clienti, Documentale spazio, Cumulativo tutti e tre.
        /// </summary>
        public static Lic_Listino Listino(TipoSoggettoLicenza tipoSoggetto, AmbitoLicenza ambito, string descrizione, bool disponibileSuCompany,
                                          params Lic_Piano[] piani)
        {
            foreach (var piano in piani)
                LicenseDefaults.Applica(piano, ambito);
            return new()
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
        }

        /// <summary>Piano del listino (prezzi; i limiti li assegna Listino con i valori predefiniti).</summary>
        public static Lic_Piano Piano(LivelloLicenza livello, string codice, string? descrizione, decimal prezzoMensile, decimal prezzoAnnuale)
            => new()
            {
                Livello = livello,
                Codice = codice,
                Descrizione = descrizione,
                Ordine = (int)livello * 10,
                PrezzoMensile = livello == LivelloLicenza.Free ? 0 : prezzoMensile,
                PrezzoAnnuale = livello == LivelloLicenza.Free ? 0 : prezzoAnnuale,
                Attivo = true
            };
    }
}
