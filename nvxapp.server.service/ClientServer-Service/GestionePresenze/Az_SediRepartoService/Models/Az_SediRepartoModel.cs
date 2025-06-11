using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models
{
    public class Az_SediRepartoModel
    {
        public int Id { get; set; }
        public int IdAz_Sedi { get; set; }
        public string? Descrizione { get; set; }
        public int? IdAz_SediReparto { get; set; }
        public Boolean Default { get; set; }
    }


    public class Az_SediReparto_GetAll_InModel
    {

    }
    public class Az_SediReparto_GetAll_OutModel : ModelResult
    {
        public List<Az_SediRepartoModel> Az_SediReparto { get; set; } = new List<Az_SediRepartoModel>();

    }


    public class Az_SediReparto_Get4User_InModel
    {
        public string? IdAspNetUsers { get; set; }
    }
    public class Az_SediReparto_Get4User_OutModel : ModelResult
    {
        public List<Az_SediRepartoModel> Az_SediReparto { get; set; } = new List<Az_SediRepartoModel>();

    }

    public class Az_SediReparto_Get4Admin_InModel
    {
        public string? IdAspNetUsers { get; set; }
    }
    public class Az_SediReparto_Get4Admin_OutModel : ModelResult
    {
        public List<Az_SediRepartoModel> Az_SediReparto { get; set; } = new List<Az_SediRepartoModel>();

    }

    public class Az_SediReparto_Get4AdminApproval_InModel
    {
        public string? IdAspNetUsers { get; set; }
    }
    public class Az_SediReparto_Get4AdminApproval_OutModel : ModelResult
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
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Number> Az_SediRepartoAttivita { get; set; } = new List<CheckObjOn_Id_Number>();

    }

    public class CheckObjOn_Id_Text_4ApprovalZorder : CheckObjOn_Id_Text
    {
        public Boolean EnabledToApproval { get; set; }
        public int ApprovalZOrder { get; set; }
    }



    public class Az_SediRepartoPutInModel
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Number> Az_SediRepartoAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
    }
    public class Az_SediRepartoPutOutModel : ModelResult
    {
        public Az_SediRepartoModel Az_SediReparto { get; set; } = new Az_SediRepartoModel();
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedAdmin { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Text_4ApprovalZorder> SelectedUser { get; set; } = new List<CheckObjOn_Id_Text_4ApprovalZorder>();
        public List<CheckObjOn_Id_Number> Az_SediRepartoAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
    }

    public class Az_SediRepartoDeleteInModel
    {
        public int Id { get; set; }
    }

    public class Az_SediRepartoDeleteOutModel : ModelResult
    {
        public Az_SediRepartoModel? Az_SediReparto { get; set; }
    }
}
