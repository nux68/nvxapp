using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Events : BaseEntity
    {

        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public EventState State { get; set; }

        // Campo per oggetto JSON
        public string? DataJson { get; set; }


    }


    public enum EventType
    {
        ClockRequest = 1,
        JustRequest = 2,
        ExpenseReportRequest = 3
    }

    public enum EventState
    {
        Entered = 1,
        Deleted = 1,
        Rejected,
        Approved,

    }


}
