using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /*
     Valori predefiniti dei limiti dei piani (ANALISI_LICENZE.md §5.1), null = illimitati:
        - Presenze    -> utenti   10 / 50 / illimitati
        - CRM         -> clienti  100 / 1000 / illimitati
        - Documentale -> spazio   1024 / 5120 / 20480 MB (1 / 5 / 20 GB)
        - Cumulativo  -> tutti e tre
     Li usano le impostazioni (se mancano in AppSetting) e i listini iniziali dei rami (LicenseCatalogSeedData.Listino).
     Gli stessi valori sono i dati iniziali di AppSetting in PublicDbContext (Licenze_MaxUtenti_*, Licenze_MaxClienti_*, Licenze_MaxSpazioMB_*).
    */
    public static class LicenseDefaults
    {
        public static int? MaxUtenti(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => 10,
            LivelloLicenza.Standard => 50,
            _ => null
        };

        public static int? MaxClienti(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => 100,
            LivelloLicenza.Standard => 1000,
            _ => null
        };

        public static int? MaxSpazioMB(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => 1024,
            LivelloLicenza.Standard => 5120,
            _ => 20480
        };

        /// <summary>Il listino della funzione usa il limite della funzione; il Cumulativo li usa tutti.</summary>
        public static bool UsaLimite(AmbitoLicenza listino, AmbitoLicenza funzione) => listino == funzione || listino == AmbitoLicenza.Cumulativo;

        /// <summary>Assegna al piano i limiti predefiniti della funzione del listino (gli altri restano vuoti).</summary>
        public static void Applica(Lic_Piano piano, AmbitoLicenza ambito)
        {
            piano.MaxUtenti = UsaLimite(ambito, AmbitoLicenza.AttendanceTracking) ? MaxUtenti(piano.Livello) : null;
            piano.MaxClienti = UsaLimite(ambito, AmbitoLicenza.Crm) ? MaxClienti(piano.Livello) : null;
            piano.MaxSpazioMB = UsaLimite(ambito, AmbitoLicenza.DocumentManagement) ? MaxSpazioMB(piano.Livello) : null;
        }
    }
}
