using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{
    public class UserLoadInModel
    {
        public string? Id { get; set; }
    }

    public class UserLoadOutModel : ModelResult
    {
        public UserDataModel UserData { get; set; }
        public string? Token { get; set; }

        public UserLoadOutModel(){
            UserData = new UserDataModel();
        }
    }

    public class UserDataModel 
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public List<AspNetRolesModel> Roles { get; set; }

        // applicativi attivi per l'azienda dell'utente (vuoto per utenti senza azienda)
        public List<ApplicationType> ActiveApplications { get; set; }

        // applicativi abilitati per lo studio dell'utente (funzioni lato studio; vuoto per chi non e' di uno studio)
        public List<ApplicationType> FinancialAdvisorApplications { get; set; } = new List<ApplicationType>();

        // voci di menu nascoste ai ruoli dell'azienda dell'utente
        public List<CompanyMenuRestrictionModel> MenuRestrictions { get; set; }

        // licenze dell'azienda (o dello studio) dell'utente, per menu e pulsanti (ANALISI_LICENZE.md §8.1.1, §8.1.2)
        public UserLicenseModel License { get; set; } = new UserLicenseModel();

        public UserDataModel()
        {
            Roles = new List<AspNetRolesModel>();
            ActiveApplications = new List<ApplicationType>();
            MenuRestrictions = new List<CompanyMenuRestrictionModel>();
        }
    }

    public class UserLicenseModel
    {
        // false = sistema licenze non attivo: tutto come Pro, utenti illimitati
        public bool Enabled { get; set; }
        public List<UserLicenseFunctionModel> Function { get; set; } = new List<UserLicenseFunctionModel>();
        public int ActiveUsers { get; set; }
        // limite utenti del soggetto (il piu' alto tra le funzioni coperte); null = illimitati
        public int? UserLimit { get; set; }
        public bool CanAddUser { get; set; } = true;
    }

    public class UserLicenseFunctionModel
    {
        public ApplicationType Application { get; set; }
        // 1 Free, 2 Standard, 3 Pro; null = nessuna licenza valida
        public int? Livello { get; set; }
        // 0 NotSubscribed, 1 Licensed, 2 Grace, 3 Expired
        public int Stato { get; set; }
        public int? MaxUtenti { get; set; }
        public DateTime? DataScadenza { get; set; }
    }

}
