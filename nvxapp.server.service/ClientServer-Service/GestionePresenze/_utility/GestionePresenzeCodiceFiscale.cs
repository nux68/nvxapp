using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /*
     Codice fiscale del dipendente: 16 caratteri (persona fisica, anche con omocodia: le cifre possono essere
     sostituite dalle lettere LMNPQRSTUV) con il carattere di controllo, oppure 11 cifre (codice provvisorio,
     stesso controllo della partita IVA).
    */
    public static class GestionePresenzeCodiceFiscale
    {
        private static readonly Regex Formato16 =
            new(@"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[ABCDEHLMPRST][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$", RegexOptions.Compiled);
        private static readonly Regex Formato11 = new(@"^[0-9]{11}$", RegexOptions.Compiled);

        // valori dei caratteri in posizione dispari (1a, 3a, ...) per il carattere di controllo
        private static readonly int[] DispariCifre = { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21 };
        private static readonly int[] DispariLettere = { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23 };

        /// <summary>Maiuscolo e senza spazi; null se vuoto.</summary>
        public static string? Normalizza(string? codiceFiscale)
        {
            var cf = new string((codiceFiscale ?? string.Empty).Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
            return cf.Length == 0 ? null : cf;
        }

        /// <summary>Codice gia' normalizzato: formato e carattere di controllo.</summary>
        public static bool IsValido(string codiceFiscale)
        {
            if (Formato16.IsMatch(codiceFiscale))
                return CarattereControllo(codiceFiscale) == codiceFiscale[15];
            if (Formato11.IsMatch(codiceFiscale))
                return CifraControllo11(codiceFiscale) == codiceFiscale[10] - '0';
            return false;
        }

        private static char CarattereControllo(string cf)
        {
            var somma = 0;
            for (var i = 0; i < 15; i++)
            {
                var c = cf[i];
                if (i % 2 == 0)
                    somma += char.IsDigit(c) ? DispariCifre[c - '0'] : DispariLettere[c - 'A'];
                else
                    somma += char.IsDigit(c) ? c - '0' : c - 'A';
            }
            return (char)('A' + somma % 26);
        }

        private static int CifraControllo11(string cf)
        {
            var somma = 0;
            for (var i = 0; i < 10; i++)
            {
                var n = cf[i] - '0';
                if (i % 2 == 1)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                somma += n;
            }
            return (10 - somma % 10) % 10;
        }
    }
}
