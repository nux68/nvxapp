using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Normalizzazione e validazione dei valori (slot) di un comando, comuni ai due ingressi dei comandi:
    // la chat (ChatAIService) e l'endpoint MCP (McpToolService, ANALISI_INFRASTRUCTURE_MCP.md punto MCP-A4).
    // Nessuno stato: lavora sulla definizione dell'intent e sul dizionario dei valori.
    public static class CommandSlotProcessor
    {
        // Normalizzatore dello slot: il suo, oppure quello del tipo ("dd/MM/yyyy" -> data, "HH:mm" -> orario).
        // La data di fine di un periodo usa la variante "fine" (un mese vale il suo ultimo giorno).
        public static Func<string, string?>? NormalizerFor(IntentDefinition intentDef, SlotDefinition slotDef) =>
            slotDef.Normalizer
            ?? (slotDef.Type == SlotNormalizers.DateFormat && intentDef.Slots.Any(s => s.PeriodEndSlot == slotDef.Name)
                ? SlotNormalizers.DateEnd
                : SlotNormalizers.ForType(slotDef.Type));

        // Inizio di un periodo detto come mese o settimana ("le ferie di ottobre", "la prima settimana di ottobre"):
        // se la fine non c'e', e' l'ultimo giorno del periodo
        public static void FillPeriodEnd(Dictionary<string, string> slots, SlotDefinition slotDef, string value)
        {
            if (string.IsNullOrEmpty(slotDef.PeriodEndSlot) || slots.ContainsKey(slotDef.PeriodEndSlot)) return;
            if (SlotNormalizers.TryParsePeriod(value, DateTime.Today, out _, out var end))
                slots[slotDef.PeriodEndSlot] = end.ToString(SlotNormalizers.DateFormat, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Valida i valori presenti (Validator dello slot, poi CrossValidator dell'intent) e sostituisce
        // nel dizionario quelli che il validator ha reso canonici (es. nome del dipendente).
        public static SlotValidationResult Validate(IntentDefinition intentDef, Dictionary<string, string> slots)
        {
            string? pendingInfo = null;

            foreach (var slotDef in intentDef.Slots)
            {
                if (slotDef.Validator == null) continue;
                if (!slots.TryGetValue(slotDef.Name, out var value)) continue;
                var result = slotDef.Validator(value);
                if (result == null) continue;
                if (result.IsValid)
                {
                    if (result.NormalizedValue != null)
                        slots[slotDef.Name] = result.NormalizedValue;
                    if (result.InfoMessage != null)
                        pendingInfo = result.InfoMessage;
                    continue;
                }
                return result;
            }

            if (intentDef.CrossValidator != null)
            {
                var crossResult = intentDef.CrossValidator(slots);
                if (crossResult != null) return crossResult;
            }

            return SlotValidationResult.Ok(slots.GetValueOrDefault("employeeName", string.Empty), pendingInfo);
        }

        // Nomi degli slot obbligatori ancora senza valore
        public static List<string> MissingRequired(IntentDefinition intentDef, Dictionary<string, string> slots) =>
            intentDef.Slots
                .Where(s => s.Required && !slots.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
    }
}
