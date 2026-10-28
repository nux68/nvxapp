using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using System.Globalization;
using System.Text;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Logica condivisa di ricerca dipendente per nome/cognome.
    // Usata da BaseCommandDipeHandler (slot validator + Execute) e da handler
    // che non derivano da BaseCommandDipeHandler (es. TimeSheetPowerAdminHandler).
    public static class DipendenteLookupHelper
    {
        // ---------------------------------------------------------------------------
        // Validator slot — da usare come SlotDefinition.Validator
        // ---------------------------------------------------------------------------

        // currentUserId: utente della richiesta, per risolvere ChatSelf.Token ("@me") nel suo dipendente
        public static SlotValidationResult? ValidateEmployeeSlot(string v, IDip_AnagraficaService dip_AnagraficaService,
                                                                 string? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(v) || !v.Any(char.IsLetter))
                return SlotValidationResult.Failed(
                    SlotValidationError.InvalidFormat,
                    AttendanceTrackingMessages.Get("ChatEmployeeInvalidName", v),
                    "employeeName");

            var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
            var res = dip_AnagraficaService.GetAll(req, true).Result;

            if (!res.Success || res.Data == null)
                return SlotValidationResult.Failed(
                    SlotValidationError.BusinessRuleViolation,
                    AttendanceTrackingMessages.Get("ChatEmployeesReadError"),
                    "employeeName");

            var all = res.Data.Dip_Anagrafica;
            if (all.Count == 0)
                return SlotValidationResult.Failed(
                    SlotValidationError.BusinessRuleViolation,
                    AttendanceTrackingMessages.Get("ChatNoEmployeesAvailable"),
                    "employeeName");

            // "@me": l'utente parla di sé -> il dipendente collegato al suo utente
            if (v.Trim() == ChatSelf.Token)
            {
                var self = string.IsNullOrEmpty(currentUserId)
                    ? null
                    : all.FirstOrDefault(d => d.IdAspNetUsers == currentUserId);
                return self != null
                    ? SlotValidationResult.Ok($"{self.Cognome} {self.Nome}".Trim())
                    : SlotValidationResult.Failed(
                        SlotValidationError.EntityNotFound,
                        AttendanceTrackingMessages.Get("ChatEmployeeSelfNotFound"),
                        "employeeName");
            }

            var token = v.Trim().ToLowerInvariant();
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            bool Like(string? field, string word) =>
                !string.IsNullOrWhiteSpace(field) &&
                field.Contains(word, StringComparison.OrdinalIgnoreCase);

            List<Dip_AnagraficaModel> found;

            if (parts.Length >= 2)
            {
                found = all.Where(d => Like(d.Cognome, parts[0]) && Like(d.Nome, parts[1])).ToList();
                if (found.Count == 0)
                    found = all.Where(d => Like(d.Nome, parts[0]) && Like(d.Cognome, parts[1])).ToList();
            }
            else
            {
                found = new List<Dip_AnagraficaModel>();
            }

            if (found.Count == 0)
                found = all.Where(d => Like(d.Cognome, token) || Like(d.Nome, token)).ToList();

            if (found.Count == 0 && parts.Length >= 2)
                found = all.Where(d =>
                    parts.Any(p => Like(d.Cognome, p) || Like(d.Nome, p))).ToList();

            // Nessuna corrispondenza esatta: ricerca tollerante agli errori di battitura
            // e di trascrizione vocale ("Russi" -> "Rossi", "Bianki" -> "Bianchi")
            bool fuzzy = false;
            if (found.Count == 0)
            {
                found = all.Where(d => parts.All(p => FuzzyLike(d.Cognome, p) || FuzzyLike(d.Nome, p))).ToList();
                fuzzy = found.Count > 0;
            }

            if (found.Count == 0)
                return SlotValidationResult.Failed(
                    SlotValidationError.EntityNotFound,
                    AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", v),
                    "employeeName");

            if (found.Count == 1)
            {
                var canonical = $"{found[0].Cognome} {found[0].Nome}".Trim();
                // messaggio solo con la ricerca tollerante: il nome trovato è diverso da quello detto
                var info = fuzzy ? AttendanceTrackingMessages.Get("ChatEmployeeMatched", canonical) : "";
                return SlotValidationResult.Ok(canonical, info);
            }

            var suggestions = found
                .Select(d => $"{d.Cognome} {d.Nome}".Trim())
                .OrderBy(s => s)
                .ToList();

            return SlotValidationResult.Failed(
                SlotValidationError.AmbiguousEntity,
                AttendanceTrackingMessages.Get("ChatEmployeesAmbiguous", found.Count, v),
                "employeeName",
                suggestions);
        }

        // Parola simile a una del campo dopo la normalizzazione fonetica (accenti, h, doppie):
        // distanza di modifica al massimo 1 per parole fino a 6 lettere, 2 per quelle più lunghe.
        private static bool FuzzyLike(string? field, string word)
        {
            if (string.IsNullOrWhiteSpace(field) || word.Length < 3) return false;

            var w = Phonetic(word);
            return field.Split(new[] { ' ', '\'', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(f => EditDistance(Phonetic(f), w) <= (w.Length <= 6 ? 1 : 2));
        }

        // minuscolo, senza accenti, senza "h", lettere doppie ridotte a una ("Bianchi" -> "bianci")
        private static string Phonetic(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark || c == 'h') continue;
                if (sb.Length > 0 && sb[^1] == c) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        // distanza di Levenshtein
        private static int EditDistance(string a, string b)
        {
            var prev = Enumerable.Range(0, b.Length + 1).ToArray();
            for (int i = 1; i <= a.Length; i++)
            {
                var cur = new int[b.Length + 1];
                cur[0] = i;
                for (int j = 1; j <= b.Length; j++)
                    cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
                prev = cur;
            }
            return prev[b.Length];
        }

        // ---------------------------------------------------------------------------
        // Lookup per Execute — restituisce il modello del dipendente da nome canonico
        // ---------------------------------------------------------------------------

        public static Dip_AnagraficaModel? GetDipendente(string employeeName, IDip_AnagraficaService dip_AnagraficaService)
        {
            if (string.IsNullOrWhiteSpace(employeeName))
                return null;

            var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
            var res = dip_AnagraficaService.GetAll(req, true).Result;

            if (!res.Success || res.Data == null)
                return null;

            var token = employeeName.Trim().ToLowerInvariant();
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return null;

            string cognome = parts[0];
            string nome    = parts[1];

            return res.Data.Dip_Anagrafica
                .FirstOrDefault(x =>
                    (x.Cognome?.ToLowerInvariant() ?? "") == cognome &&
                    (x.Nome?.ToLowerInvariant()    ?? "") == nome);
        }
    }
}
