using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class TimeClock : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Clocking { get; set; }
        public DateTime ClockingOriginal { get; set; }
        public DateTime? ClockingRounded { get; set; }
        public DateTime DateReference { get; set; } // girno al quale viene agganciata la timbratura (servita per cavallo notte montanti /smontanti)
        public ClockingType Type { get; set; }
    }


    public enum ClockingType
    {
        Enter ,
        Exit ,
        WithoutVerse ,
        Activity 
    }


}
