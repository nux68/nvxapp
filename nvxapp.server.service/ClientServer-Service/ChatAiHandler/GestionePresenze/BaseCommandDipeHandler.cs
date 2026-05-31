using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze;
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
                Validator = v => DipendenteLookupHelper.ValidateEmployeeSlot(v, _dip_AnagraficaService)
            };
        }

        protected Dip_AnagraficaModel? Get_Dip_Anagrafica(string employeeName)
            => DipendenteLookupHelper.GetDipendente(employeeName, _dip_AnagraficaService);



    }
}
