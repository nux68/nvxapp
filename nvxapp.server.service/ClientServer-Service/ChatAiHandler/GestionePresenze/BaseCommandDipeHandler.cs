using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Classe base per gli handler che operano su un dipendente.
    // Aggiunge lo slot employeeName condiviso da tutti gli handler derivati.
    public abstract class BaseCommandDipeHandler : BaseCommandHandler
    {

        protected readonly IDip_AnagraficaService _dip_AnagraficaService;

        protected BaseCommandDipeHandler(IDip_AnagraficaService dip_AnagraficaService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
        }

        protected static readonly SlotDefinition EmployeeNameSlot = new()
        {
            Name = "employeeName",
            Type = "string",
            Required = true,
            //PromptDescription = "nome e cognome del dipendente — NON usare questa frase come valore. Devi estrarre SOLO un nome realmente scritto dall'utente.",
            PromptDescription = @"(string, obbligatorio)",

            Question = "Per quale dipendente?",
            Label = "Dipendente",
            //Validator         = v => v.Trim().Length >= 2 && v.Any(char.IsLetter) ? null
            //    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
            //        $"'{v}' non sembra un nome valido. Inserire nome e cognome del dipendente.", "employeeName")
            Validator = delegate (string v)
            {
                if (v == null)
                {
                    return SlotValidationResult.Failed(
                        SlotValidationError.InvalidFormat,
                        "Il valore non può essere nullo.",
                        "employeeName"
                    );
                }

                if (v.Trim().Length >= 2 && v.Any(char.IsLetter))
                {
                    return null;
                }
                else
                {
                    return SlotValidationResult.Failed(
                                                           SlotValidationError.InvalidFormat,
                                                           "'" + v + "' non sembra un nome valido. Inserire nome e cognome del dipendente.",
                                                           "employeeName"
                                                       );
                }


            }
        };
    }
}
