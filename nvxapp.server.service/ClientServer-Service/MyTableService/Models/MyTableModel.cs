using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Service.MyTableService.Models
{
    public class MyTableModel
    {
        public string? Descrizione { get; set; }
    }

    public class MyTableInModel
    {

    }

    public class MyTableOutModel : ModelResult 
    {
        public List<MyTableModel> MyTable { get; set; }

        public MyTableOutModel() 
        {
            MyTable = new List<MyTableModel>();
        }
    }

   


}
