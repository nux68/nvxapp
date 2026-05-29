using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Classe base per gli handler che operano su un dipendente.
    // Aggiunge lo slot employeeName condiviso da tutti gli handler derivati.
    public abstract class BaseCommandDipeHandler : BaseCommandHandler
    {

        protected readonly IDip_AnagraficaService _dip_AnagraficaService;

        // Non più static: deve catturare _dip_AnagraficaService dalla specifica istanza.
        protected readonly SlotDefinition EmployeeNameSlot;

        protected BaseCommandDipeHandler(IDip_AnagraficaService dip_AnagraficaService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;

            // Inizializzato nel costruttore così può usare _dip_AnagraficaService.
            EmployeeNameSlot = new SlotDefinition
            {
                Name = "employeeName",
                Type = "string",
                Required = true,
                PromptDescription = "(string, obbligatorio)",
                Question = "Per quale dipendente?",
                Label = "Dipendente",
                Validator = v =>
                {
                    if (string.IsNullOrWhiteSpace(v) || !v.Any(char.IsLetter))
                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            $"'{v}' non sembra un nome valido. Inserire nome e/o cognome del dipendente.",
                            "employeeName");

                    // Carica tutti i dipendenti
                    var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
                    var res = _dip_AnagraficaService.GetAll(req, true).Result;

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

                    // --- Strategia di ricerca ---
                    // Normalizza il token in input (minuscolo, senza spazi doppi)
                    var token = v.Trim().ToLowerInvariant();
                    var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    // Helper: confronto fuzzy — contiene la sottostringa (case-insensitive)
                    bool Like(string? field, string word) =>
                        !string.IsNullOrWhiteSpace(field) &&
                        field.Contains(word, StringComparison.OrdinalIgnoreCase);

                    List<Dip_AnagraficaModel> found;
                    bool exactMatch = false;

                    if (parts.Length >= 2)
                    {
                        // FASE 1a: cognome=parts[0] AND nome=parts[1]
                        found = all.Where(d =>
                            Like(d.Cognome, parts[0]) && Like(d.Nome, parts[1])).ToList();

                        // FASE 1b: nome=parts[0] AND cognome=parts[1]
                        if (found.Count == 0)
                            found = all.Where(d =>
                                Like(d.Nome, parts[0]) && Like(d.Cognome, parts[1])).ToList();

                        if (found.Count > 0) exactMatch = true;
                    }
                    else
                    {
                        found = new List<Dip_AnagraficaModel>();
                    }

                    // FASE 2: se non trovato con due token, cerca per cognome O nome su tutto il token
                    if (found.Count == 0)
                        found = all.Where(d =>
                            Like(d.Cognome, token) || Like(d.Nome, token)).ToList();

                    // FASE 3: se ancora nessun risultato e ci sono più parti, prova ogni singola parte
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
                        // Match fuzzy: salva il nome canonico e informa l'utente, senza chiedere conferma
                        var info = !exactMatch ? $"Dipendente agganciato: {canonical}" : null;
                        info = "";
                        return SlotValidationResult.Ok(canonical, info);
                    }

                    // Più di uno: restituisce la lista come suggestions (Cognome Nome)
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
            };
        }

        protected Dip_AnagraficaModel? Get_Dip_Anagrafica(string employeeName)
        {
            if (string.IsNullOrWhiteSpace(employeeName))
                return null;

            var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
            var res = _dip_AnagraficaService.GetAll(req, true).Result;

            if (!res.Success || res.Data == null)
                return null;

            var token = employeeName.Trim().ToLowerInvariant();
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return null;

            string cognome = parts[0];
            string nome = parts[1];

            var dip = res.Data.Dip_Anagrafica
                .FirstOrDefault(x =>
                    (x.Cognome?.ToLowerInvariant() ?? "") == cognome &&
                    (x.Nome?.ToLowerInvariant() ?? "") == nome
                );

            return dip;
        }



    }
}
