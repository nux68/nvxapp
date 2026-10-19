using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction.Models
{
    // voce di menu (url) nascosta a un ruolo dell'azienda
    public class CompanyMenuRestrictionModel
    {
        public string Url { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }


    // esclusioni dell'azienda corrente
    public class CompanyMenuRestrictionListInModel
    {
    }

    public class CompanyMenuRestrictionListOutModel : ModelResult
    {
        public List<CompanyMenuRestrictionModel> CompanyMenuRestriction { get; set; } = new List<CompanyMenuRestrictionModel>();
    }


    // sostituisce tutte le esclusioni dell'azienda corrente
    public class CompanyMenuRestrictionPutInModel
    {
        public List<CompanyMenuRestrictionModel> CompanyMenuRestriction { get; set; } = new List<CompanyMenuRestrictionModel>();
    }

    public class CompanyMenuRestrictionPutOutModel : ModelResult
    {
        public List<CompanyMenuRestrictionModel> CompanyMenuRestriction { get; set; } = new List<CompanyMenuRestrictionModel>();
    }
}
