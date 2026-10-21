using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMove.Models
{
    public class CompanyMoveDealerModel
    {
        public int IdDealer { get; set; }
        public string? Descrizione { get; set; }
    }

    public class CompanyMoveFinancialAdvisorModel
    {
        public int IdFinancialAdvisor { get; set; }
        public int IdDealer { get; set; }
        public string? Descrizione { get; set; }
    }

    public class CompanyMoveCompanyModel
    {
        public int IdCompany { get; set; }
        public int IdFinancialAdvisor { get; set; }
        public string? Descrizione { get; set; }
    }


    // tutta la gerarchia centri -> studi -> aziende (non filtrata sull'utente corrente)
    public class CompanyMoveTreeInModel
    {
    }

    public class CompanyMoveTreeOutModel : ModelResult
    {
        public List<CompanyMoveDealerModel> Dealers { get; set; } = new List<CompanyMoveDealerModel>();
        public List<CompanyMoveFinancialAdvisorModel> FinancialAdvisors { get; set; } = new List<CompanyMoveFinancialAdvisorModel>();
        public List<CompanyMoveCompanyModel> Companies { get; set; } = new List<CompanyMoveCompanyModel>();
    }


    // sposta l'azienda sotto un altro studio
    public class CompanyMovePutInModel
    {
        public int IdCompany { get; set; }
        public int IdFinancialAdvisor { get; set; }
    }

    public class CompanyMovePutOutModel : ModelResult
    {
        public CompanyMoveCompanyModel? Company { get; set; }
    }
}
