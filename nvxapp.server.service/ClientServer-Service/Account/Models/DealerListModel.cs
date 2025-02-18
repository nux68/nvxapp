using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Account.Models
{
    public class DealerListInModel
    {
    }

    public class DealerListOutModel : ModelResult
    {
        public List<DealerListModel> DealerList { get; set; }

        public DealerListOutModel()
        {
            DealerList = new List<DealerListModel>();
        }
    }

    public class DealerListModel
    {
        public int IdDealer { get; set; }
        
        public required string IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }
        
    }

}
