using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication.Models
{
    public class CompanyApplicationModel
    {
        public int IdCompany { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
        // licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §15.6): l'azienda e' coperta da una licenza per l'applicativo (propria, dello studio
        // o cumulativa; sempre true con il sistema licenze spento) e l'utente corrente puo' cambiarne lo stato
        public bool Licensed { get; set; } = true;
        public bool CanChange { get; set; }
    }


    // elenco di tutti gli applicativi con lo stato per l'azienda
    public class CompanyApplicationListInModel
    {
        public int IdCompany { get; set; }
    }

    public class CompanyApplicationListOutModel : ModelResult
    {
        public List<CompanyApplicationModel> CompanyApplication { get; set; }

        public CompanyApplicationListOutModel()
        {
            CompanyApplication = new List<CompanyApplicationModel>();
        }
    }


    // attivazione / disattivazione di un applicativo per l'azienda
    public class CompanyApplicationPutInModel
    {
        public int IdCompany { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public bool Active { get; set; }
    }

    public class CompanyApplicationPutOutModel : ModelResult
    {
        public CompanyApplicationModel? CompanyApplication { get; set; }
    }
}
