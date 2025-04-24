using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models
{
    public class Dip_GG_GiustificativiModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public int IdJustificationType { get; set; }
        public JustificationInputType InputType { get; set; }
        public TimeSpan? Hours { get; set; }  // se InputType=manual
        public TimeSpan? From { get; set; }   // se InputType=manual  (dalle)
        public required int IdPar_Giustificativi { get; set; }
        public StatoRichiesta RichiestaStato { get; set; }
        public int? IdDip_Richiesta { get; set; }


    }

    public class Dip_GG_GiustificativiInModel
    {

    }

    public class Dip_GG_GiustificativiOutModel : ModelResult 
    {
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi = new List<Dip_GG_GiustificativiModel>();

        public Dip_GG_GiustificativiOutModel() 
        {
        
        }
    }

   


}
