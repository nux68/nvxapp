using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Classe base per gli handler che operano su un dipendente.
    // Aggiunge lo slot employeeName condiviso da tutti gli handler derivati.
    public abstract class BaseCommandDipeHandler : BaseCommandHandler
    {
        protected static readonly SlotDefinition EmployeeNameSlot = new()
        {
            Name              = "employeeName",
            Type              = "string",
            Required          = true,
            PromptDescription = "nome e cognome del dipendente — NON usare questa frase come valore. Devi estrarre SOLO un nome realmente scritto dall'utente.",
            Question          = "Per quale dipendente?",
            Label             = "Dipendente",
            Validator         = v => v.Trim().Length >= 2 && v.Any(char.IsLetter) ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                    $"'{v}' non sembra un nome valido. Inserire nome e cognome del dipendente.", "employeeName")
        };
    }
}
