using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Justification : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Date { get; set; }
        public RequestState State { get; set; }

        public int IdJustificationType { get; set; }

        public JustificationInputType InputType { get; set; }

        public TimeSpan? Hours { get; set; }  // se InputType=manual

        public TimeSpan? From { get; set; }   // se InputType=manual  (dalle)



        public JustificationType? JustificationTypeNavigation { get; set; }

        public int? IdRequest { get; set; }
        public Request? RequestNavigation { get; set; }

    }

    public enum JustificationInputType
    {
        Manual,
        AllDay,
        IntegrateDay
    }

}
