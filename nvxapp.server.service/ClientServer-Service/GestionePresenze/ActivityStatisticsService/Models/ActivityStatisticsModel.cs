using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models
{
    public class ActivityStatisticsModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public List<string> SelectedUserId { get; set; } = new List<string>();
    }

    public class ActivityStatistics_GetInModel
    {
        public ActivityStatisticsModel ActivityStatistics { get; set; } = new ActivityStatisticsModel();
    }

    public class ActivityStatistics_GetOutModel : ModelResult
    {
        public List<ActivityStatisticsModel> ActivityStatistics { get; set; } = new List<ActivityStatisticsModel>();
    }
}
