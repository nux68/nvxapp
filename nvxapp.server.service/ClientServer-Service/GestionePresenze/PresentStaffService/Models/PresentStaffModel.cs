using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService.Models
{
    public class PresentStaffModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<string> SelectedUserId { get; set; } = new List<string>();
    }



    public class PresentStaff_GetInModel
    {
        public PresentStaffModel PresentStaff { get; set; } = new PresentStaffModel();
    }

    public class PresentStaff_DaySlot
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public bool IsPresent { get; set;}
    }


    public class PresentStaff_GetOutModel : ModelResult
    {
        public List<PresentStaff_DaySlot> DaySlots { get; set; } = new List<PresentStaff_DaySlot>();
    }
}
