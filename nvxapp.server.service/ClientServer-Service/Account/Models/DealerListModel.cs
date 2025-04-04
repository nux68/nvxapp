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

    public class DealerListModel
    {
        public int IdDealer { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }

    }
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




    public class DealerEditModel
    {
        public int IdDealer { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
    }
    public class DealerGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class DealerGetOutModel : ModelResult
    {
        public DealerEditModel DealerEdit { get; set; }  = new DealerEditModel();
    }
    public class DealerPutInModel : ModelResult
    {
        public DealerEditModel DealerEdit { get; set; } = new DealerEditModel();
    }
    public class DealerPutOutModel : ModelResult
    {
        public DealerEditModel DealerEdit { get; set; } = new DealerEditModel();
    }


}
