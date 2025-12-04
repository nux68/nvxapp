using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models
{
    public class Dip_RapportoLavoroModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
    }

    public class Dip_RapportoLavoro_Get_InModel
    {
        public int Id { get; set; }  // = IdDip_Anagrafica
    }
    public class Dip_RapportoLavoro_Get_OutModel : ModelResult
    {
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();
    }

    public class Dip_RapportoLavoro_Put_InModel
    {
        public int Id { get; set; } // = IdDip_Anagrafica
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();
    }
    public class Dip_RapportoLavoro_Put_OutModel : ModelResult
    {
        public int Id { get; set; } // = IdDip_Anagrafica
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();
    }


}
