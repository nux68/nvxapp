using System.Globalization;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Converte un importo in euro scritto dall'utente in decimal (2 decimali).
    // Formati accettati: "25", "25,50", "25.50", "1.250,00", "1,250.00", "€ 25", "25 euro", "25€"
    public static class ImportoSlotHelper
    {
        // Descrizione dello slot importo per il LLM: copia il numero, la conversione la fa Normalize
        public const string PromptDescription =
            "importo in euro: copia il numero usato dall'utente cosi' come l'ha scritto (es. '25', '25,50', '12.30 euro'). Non convertirlo.";

        // Normalizzatore dello slot: espressione -> "25.50" (separatore punto), null se non riconosciuta
        public static string? Normalize(string value) =>
            TryParse(value, out var importo) ? importo.ToString("0.00", CultureInfo.InvariantCulture) : null;

        public static bool TryParse(string? value, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var testo = Regex.Replace(value.Trim(), @"(?i)€|euro|eur", string.Empty).Replace(" ", string.Empty);
            if (!Regex.IsMatch(testo, @"^\d{1,3}([.,]\d{3})*([.,]\d{1,2})?$|^\d+([.,]\d{1,2})?$"))
                return false;

            // l'ultimo separatore seguito da 1-2 cifre e' quello dei decimali, gli altri sono delle migliaia
            var decimali = Regex.Match(testo, @"[.,](\d{1,2})$");
            var intero = decimali.Success ? testo[..decimali.Index] : testo;
            intero = intero.Replace(".", string.Empty).Replace(",", string.Empty);
            var numero = decimali.Success ? $"{intero}.{decimali.Groups[1].Value}" : intero;

            if (!decimal.TryParse(numero, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result) || result <= 0)
                return false;
            result = Math.Round(result, 2);
            return true;
        }

        /// <summary>importo da mostrare all'utente, es. "€ 25,50"</summary>
        public static string Testo(decimal importo) => importo.ToString("€ #,##0.00", CultureInfo.GetCultureInfo("it-IT"));
    }
}
