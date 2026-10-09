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
