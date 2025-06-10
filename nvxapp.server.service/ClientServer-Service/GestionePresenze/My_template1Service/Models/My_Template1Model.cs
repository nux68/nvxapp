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
        public int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    
    public class My_template1_GetAllInModel
    {
    }
    public class My_template1_GetAllOutModel : ModelResult
    {
        public List<My_Template1Model> my_Template1 { get; set; }
        public My_template1_GetAllOutModel()
        {
            my_Template1 = new List<My_Template1Model>();
        }
    }

    
    public class My_template1_GetInModel
    {
        public int Id { get; set; }
    }
    public class My_template1_GetOutModel : ModelResult
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }

    
    public class My_template1_PutInModel
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }
    public class My_template1_PutOutModel : ModelResult
    {
        public My_Template1Model my_Template1 { get; set; } = new My_Template1Model();
    }
}
