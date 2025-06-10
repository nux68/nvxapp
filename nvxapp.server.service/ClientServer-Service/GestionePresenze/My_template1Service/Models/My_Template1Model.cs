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

    // Per GetAll
    public class My_template1InModel
    {
    }

    public class My_template1OutModel : ModelResult
    {
        public List<My_Template1Model> my_Template1 { get; set; }
        public My_template1OutModel()
        {
            my_Template1 = new List<My_Template1Model>();
        }
    }

    // Per Get (singolo)
    public class My_template1_GetInModel
    {
        public int Id { get; set; }
    }

    public class My_template1_GetOutModel : ModelResult
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }

    // Per Put (salvataggio)
    public class My_template1_PutInModel
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }

    public class My_template1_PutOutModel : ModelResult
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }
}
