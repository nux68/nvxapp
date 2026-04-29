using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models
{
    public class VacationPlanModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public List<string> SelectedUserId { get; set; } = new List<string>();
    }

    public class VacationPlan_GetInModel
    {
        public VacationPlanModel VacationPlan { get; set; } = new VacationPlanModel();
    }

    public class VacationPlan_GetOutModel : ModelResult
    {
        public List<VacationPlanModel> VacationPlan { get; set; } = new List<VacationPlanModel>();
    }
}
