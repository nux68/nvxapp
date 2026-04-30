using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models
{
    public class Par_GiustificativiModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public string? Codice { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public JustTipoInput TipoInput { get; set; }
        public int? IdCausale { get; set; }
        public SignWithNeutral Segno { get; set; }
        public bool VisualizzaInPianoFerie { get; set; }
    }


    public class Par_GiustificativiInModel
    {

    }
    public class Par_GiustificativiOutModel : ModelResult 
    {
        public List<Par_GiustificativiModel> Par_Giustificativi { get; set; } = new List<Par_GiustificativiModel>();



        public Par_GiustificativiOutModel() 
        {
        
        }
    }


    public class Par_GiustificativiGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class Par_GiustificativiGetOutModel : ModelResult
    {
        public Par_GiustificativiModel Par_Giustificativi { get; set; } = new Par_GiustificativiModel();
    }
    public class Par_GiustificativiPutInModel : ModelResult
    {
        public Par_GiustificativiModel Par_Giustificativi { get; set; } = new Par_GiustificativiModel();
    }
    public class Par_GiustificativiPutOutModel : ModelResult
    {
        public Par_GiustificativiModel Par_Giustificativi { get; set; } = new Par_GiustificativiModel();
    }

    public class Par_Giustificativi_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Giustificativi_DeleteOutModel : ModelResult
    {
    }
}
