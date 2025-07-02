using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models
{
    public class Az_SubCommessaModel
    {
        public int Id { get; set; }
        public int IdAz_Commessa { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }
    }

    public class Az_SubCommessa_GetAll_InModel { }

    public class Az_SubCommessa_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessaModel>();
    }




    //incorpora tutti i dati che compongono la sub commessa
    public class Az_SubCommessa_4EditModel : Az_SubCommessaModel
    {
        //public List<CheckObjOn_Id_Text> Az_SubCommessaUser { get; set; } = new List<CheckObjOn_Id_Text>();
        public List<Az_SubCommessaUser4EditModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUser4EditModel>();

        public List<CheckObjOn_Id_Number> Az_SubCommessaAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
        public List<CheckObjOn_Id_Number> Az_SubCommessaSediReparto { get; set; } = new List<CheckObjOn_Id_Number>();
    }


    public class Az_SubCommessa_GetAll_4Edit_InModel
    {
        public int Id { get; set; }  // id della commessa
    }

    public class Az_SubCommessa_GetAll_4Edit_OutModel : ModelResult
    {
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }


    public class Az_SubCommessa_PutAll_4Edit_InModel
    {
        public int Id { get; set; }  // id della commessa
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }

    public class Az_SubCommessa_PutAll_4Edit_OutModel : ModelResult
    {
        public int Id { get; set; }  // id della commessa
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }

}
