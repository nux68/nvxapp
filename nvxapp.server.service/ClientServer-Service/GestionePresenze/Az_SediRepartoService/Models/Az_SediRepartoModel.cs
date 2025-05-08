using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models
{
    public class Az_SediRepartoModel
    {
        public int Id { get; set; }
        public int IdAz_Sedi { get; set; }
        public string? Descrizione { get; set; }
        public int? IdAz_SediReparto { get; set; }
    }


    public class Az_SediReparto_GetAll_InModel
    {

    }
    public class Az_SediReparto_GetAll_OutModel : ModelResult 
    {
        public List<Az_SediRepartoModel> Az_SediReparto { get; set; } = new List<Az_SediRepartoModel>();
    }

    public class Az_SediRepartoGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class Az_SediRepartoGetOutModel : ModelResult
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
    }

    public class Az_SediRepartoPutInModel
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
    }
    public class Az_SediRepartoPutOutModel : ModelResult
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
    }
}
