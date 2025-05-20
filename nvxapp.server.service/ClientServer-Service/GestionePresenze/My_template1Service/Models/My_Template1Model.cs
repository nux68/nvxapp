using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models
{
    public class My_Template1Model
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
    }

    public class My_template1InModel
    {
    }

    public class My_template1OutModel : ModelResult
    {
        public My_template1OutModel()
        {
        }
    }
}
