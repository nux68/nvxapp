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
                Name              = "employeeName",
                Type              = "string",
                Required          = true,
                PromptDescription = "(string, obbligatorio)",
                Question          = "Per quale dipendente?",
                Label             = "Dipendente",
                Validator         = v =>
                {
                    if (v == null)
                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            "Il valore non può essere nullo.",
                            "employeeName");

                    if (v.Trim().Length >= 2 && v.Any(char.IsLetter))
                    {
                        var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
                        var res   =  _dip_AnagraficaService.GetAll(req, true).Result;
                        if(res.Success && res.Data!=null)
                        {

                            if(res.Data.Dip_Anagrafica.Count>0)
                            {
                                return null;
                            }
                            else
                            {
                                return SlotValidationResult.Failed(
                                                                SlotValidationError.BusinessRuleViolation,
                                                                "Nessu dipendente disponibile",
                                                                "employeeName");
                            }
                        }
                        else
                        {
                            return SlotValidationResult.Failed(
                                                                SlotValidationError.BusinessRuleViolation,
                                                                "Errore nella lettura dipendenti",
                                                                "employeeName");
                        }
                    }

                    return SlotValidationResult.Failed(
                        SlotValidationError.InvalidFormat,
                        $"'{v}' non sembra un nome valido. Inserire nome e cognome del dipendente.",
                        "employeeName");
                }
            };
        }
    }
}
