using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models
{
    public class VacationPlanModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<string> SelectedUserId { get; set; } = new List<string>();
    }

    public class VacationPlan_DaySlot
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public List<Dip_GG_RichiestaModel> Dip_GG_Richieste { get; set; } = new List<Dip_GG_RichiestaModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
    }

    public class VacationPlan_GetInModel
    {
        public VacationPlanModel VacationPlan { get; set; } = new VacationPlanModel();
    }

    public class VacationPlan_GetOutModel : ModelResult
    {
        public List<VacationPlan_DaySlot> DaySlots { get; set; } = new List<VacationPlan_DaySlot>();
    }
}
