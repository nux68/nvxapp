using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication.Models
{
    // stato di un applicativo per lo studio (funzioni lato studio)
    public class FinancialAdvisorApplicationModel
    {
        public int IdFinancialAdvisor { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
    }

    public class FinancialAdvisorApplicationListInModel
    {
        public int IdFinancialAdvisor { get; set; }
    }

    public class FinancialAdvisorApplicationListOutModel : ModelResult
    {
        public List<FinancialAdvisorApplicationModel> FinancialAdvisorApplication { get; set; } = new List<FinancialAdvisorApplicationModel>();
        // l'utente corrente puo' cambiarli (SuperUser o DealerPowerAdmin del centro dello studio)
        public bool CanManage { get; set; }
    }

    public class FinancialAdvisorApplicationPutInModel
    {
        public int IdFinancialAdvisor { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public bool Active { get; set; }
    }

    public class FinancialAdvisorApplicationPutOutModel : ModelResult
    {
        public FinancialAdvisorApplicationModel? FinancialAdvisorApplication { get; set; }
    }
}
