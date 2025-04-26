using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models
{
    public class Dip_GG_RichiestaModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        
        public DateTime Data { get; set; }
        public DateTime DataA { get; set; }

        public TipoRichiesta RichiestaTipo { get; set; }
        public StatoRichiesta RichiestaStato { get; set; }

        // Campo per oggetto JSON
        public string? Dati { get; set; }

        // Campo per oggetto JSON
        public string? CronologiaApprovazione { get; set; }

    }



    public class Dip_GG_Richiesta_GetAll_InModel
    {
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }

    public class Dip_GG_Richiesta_GetAll_OutModel : ModelResult 
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_RichiestaModel { get; set; } = new List<Dip_GG_RichiestaModel>();
    }



    public class Dip_GG_Richiesta_Send_InModel
    {
        public Dip_GG_RichiestaModel? Dip_GG_RichiestaModel { get; set; } 
    }

    public class Dip_GG_Richiesta_Send_OutModel : ModelResult
    {
        public Dip_GG_RichiestaModel? Dip_GG_RichiestaModel { get; set; }
    }

}
