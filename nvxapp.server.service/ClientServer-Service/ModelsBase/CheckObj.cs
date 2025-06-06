using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{
    public class CheckObjOn_Id_Text
    {
        public string Id { get; set; } = string.Empty;
        public bool Checked { get; set; }

    }

      public class CheckObjOn_Id_Number
   {
       public int Id { get; set; } = 0;
       public bool Checked { get; set; }

   }
}
