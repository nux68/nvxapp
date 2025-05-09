using nvxapp.server.service.ClientServer_Service.ModelsBase;

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
        public List<CheckObjOn_Id_Text> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text>();
        public List<CheckObjOn_Id_Text> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text>();


    }

    public class Az_SediRepartoPutInModel
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
        public List<CheckObjOn_Id_Text> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text>();
        public List<CheckObjOn_Id_Text> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text>();
    }
    public class Az_SediRepartoPutOutModel : ModelResult
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
        public List<CheckObjOn_Id_Text> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text>();
        public List<CheckObjOn_Id_Text> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text>();
    }
}
