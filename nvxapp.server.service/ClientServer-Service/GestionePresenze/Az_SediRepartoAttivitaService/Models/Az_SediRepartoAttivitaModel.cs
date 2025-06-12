using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models
{
    public class Az_SediRepartoAttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_SediReparto { get; set; }
        public int IdPar_Attivita { get; set; }
    }

    public class Az_SediRepartoAttivitaInModel
    {
    }

    public class Az_SediRepartoAttivitaOutModel : ModelResult
    {
        public List<Az_SediRepartoAttivitaModel> Az_SediRepartoAttivita { get; set; } = new List<Az_SediRepartoAttivitaModel>();
    }


    public class Az_SediRepartoAttivita_Selected_GetInModel
    {
        public int IdAz_SediReparto { get; set; }
    }
    public class Az_SediRepartoAttivita_Selected_GetOutModel: ModelResult
    {
        public List<Az_SediRepartoAttivitaModel> Az_SediRepartoAttivita { get; set; } = new List<Az_SediRepartoAttivitaModel>();
    }

    public class Az_SediRepartoAttivita_Selected_PutInModel
    {
        public int IdAz_SediReparto { get; set; }
        public List<Az_SediRepartoAttivitaModel> Az_SediRepartoAttivita { get; set; } = new List<Az_SediRepartoAttivitaModel>();
    }
    public class Az_SediRepartoAttivita_Selected_PutOutModel: ModelResult
    {
        public List<Az_SediRepartoAttivitaModel> Az_SediRepartoAttivita { get; set; } = new List<Az_SediRepartoAttivitaModel>();
    }

}
