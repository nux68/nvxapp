using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{
    public class UserCompanyModel
    {
        public string? IdAspNetUsers { get; set; }
        public int IdUserCompany { get; set; }
        public string? Descrizione { get; set; }
        public Boolean MainUser { get; set; }
        
        public List<string> Roles { get; set; } = new List<string>();
    }
    public class UserCompanyListInModel
    {
        public List<string> FilteredRoles { get; set; } = new List<string>();
        
    }
    public class UserCompanyListOutModel : ModelResult
    {
        public List<UserCompanyModel> UserCompanyList { get; set; }

        public UserCompanyListOutModel()
        {
            UserCompanyList = new List<UserCompanyModel>();
        }
    }



    public class UserCompanyEditModel
    {
        public int IdUserCompany { get; set; }
        public string? Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public string? RoleId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();

        
        
    }
    
    
    public class UserCompanyGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class UserCompanyGetOutModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }
  
    
    public class UserCompanyPutInModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }
    public class UserCompanyPutOutModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }

}
