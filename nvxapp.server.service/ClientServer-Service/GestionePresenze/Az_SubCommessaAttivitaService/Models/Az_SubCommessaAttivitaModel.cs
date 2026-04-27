using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models
{
    public class Az_SubCommessaAttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public int IdPar_Attivita { get; set; }
        public Boolean Default { get; set; }
    }



    public class Az_SubCommessaAttivita_GetAll_InModel { }

    public class Az_SubCommessaAttivita_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivitaModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivitaModel>();
    }

    public class Az_SubCommessaAttivita4EditModel : Az_SubCommessaAttivitaModel, ICheckObj<int>
    {
        public bool Checked { get; set; }
    }

    public class Az_SubCommessaAttivita_Get4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
    }

    public class Az_SubCommessaAttivita_Get4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }

    public class Az_SubCommessaAttivita_Put4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }

    public class Az_SubCommessaAttivita_Put4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }



    public class Az_SubCommessaAttivita_4FullListModel
    {
        public int Commessa_Id { get; set; }
        public int Commessa_IdAz_Cliente { get; set; }
        public string Commessa_Decrizione { get; set; } = string.Empty;
        public Boolean Commessa_Default { get; set; }

        public int SubCommessa_Id { get; set; }
        public string SubCommessa_Decrizione { get; set; } = string.Empty;
        public Boolean SubCommessa_Default { get; set; }

        public int SubCommessaAttivita_Id { get; set; }
        public string SubCommessaAttivita_Decrizione { get; set; } = string.Empty;
        public Boolean SubCommessaAttivita_Default { get; set; }
        public int SubCommessaAttivita_IdPar_Attivita { get; set; }
    }

    public class Az_SubCommessaAttivita_GetAll_4FullList_InModel { }

    public class Az_SubCommessaAttivita_GetAll_4FullList_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivita_4FullListModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita_4FullListModel>();
    }


}
