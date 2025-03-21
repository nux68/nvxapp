using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    /*
     dati anagrafici
     */
    public class Dip_Anagrafica : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public string? Cognome { get; set; }
        public string? Nome { get; set; }

        public ICollection<Dip_RapportoLavoro>? Dip_RapportoLavoro { get; set; }
        

    }



}
