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

        public static SlotValidationResult? ValidateEmployeeSlot(string v, IDip_AnagraficaService dip_AnagraficaService)
        {
            if (string.IsNullOrWhiteSpace(v) || !v.Any(char.IsLetter))
                return SlotValidationResult.Failed(
                    SlotValidationError.InvalidFormat,
                    $"'{v}' non sembra un nome valido. Inserire nome e/o cognome del dipendente.",
                    "employeeName");

            var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
            var res = dip_AnagraficaService.GetAll(req, true).Result;

            if (!res.Success || res.Data == null)
                return SlotValidationResult.Failed(
                    SlotValidationError.BusinessRuleViolation,
                    "Errore nella lettura dei dipendenti.",
                    "employeeName");

            var all = res.Data.Dip_Anagrafica;
            if (all.Count == 0)
                return SlotValidationResult.Failed(
                    SlotValidationError.BusinessRuleViolation,
                    "Nessun dipendente disponibile.",
                    "employeeName");

            var token = v.Trim().ToLowerInvariant();
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            bool Like(string? field, string word) =>
                !string.IsNullOrWhiteSpace(field) &&
                field.Contains(word, StringComparison.OrdinalIgnoreCase);

            List<Dip_AnagraficaModel> found;
            bool exactMatch = false;

            if (parts.Length >= 2)
            {
                found = all.Where(d => Like(d.Cognome, parts[0]) && Like(d.Nome, parts[1])).ToList();
                if (found.Count == 0)
                    found = all.Where(d => Like(d.Nome, parts[0]) && Like(d.Cognome, parts[1])).ToList();
                if (found.Count > 0) exactMatch = true;
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

            if (found.Count == 0)
                return SlotValidationResult.Failed(
                    SlotValidationError.EntityNotFound,
                    $"Nessun dipendente trovato per '{v}'. Riprova con nome e/o cognome.",
                    "employeeName");

            if (found.Count == 1)
            {
                var canonical = $"{found[0].Cognome} {found[0].Nome}".Trim();
                var info = !exactMatch ? $"Dipendente agganciato: {canonical}" : null;
                info = "";
                return SlotValidationResult.Ok(canonical, info);
            }

            var suggestions = found
                .Select(d => $"{d.Cognome} {d.Nome}".Trim())
                .OrderBy(s => s)
                .ToList();

            return SlotValidationResult.Failed(
                SlotValidationError.AmbiguousEntity,
                $"Trovati {found.Count} dipendenti per '{v}'. Seleziona:",
                "employeeName",
                suggestions);
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
