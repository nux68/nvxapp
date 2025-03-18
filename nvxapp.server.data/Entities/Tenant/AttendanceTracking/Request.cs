using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Request : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public RequestType Type { get; set; }
        public RequestState State { get; set; }

        // Campo per oggetto JSON
        public string? DataJson { get; set; }

        // Campo per oggetto JSON
        public string? ApprovalCrnologyJson { get; set; }
    }


    public enum RequestType
    {
        TimeClock,
        Just,
        ExpenseReport
    }

    public enum RequestState
    {
        Direct,
        Entered,
        Deleted,
        Rejected,
        ApprovalInProgress,
        Approved,
        PartialApproved,
    }


}
