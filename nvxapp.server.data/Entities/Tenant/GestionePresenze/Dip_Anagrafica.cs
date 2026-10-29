using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    /*
     dati anagrafici
     */
    public class Dip_Anagrafica : BaseEntity
    {
        [Required]
        [ForeignKey(nameof(AspNetUsersNavigation))]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public string? Cognome { get; set; }
        public string? Nome { get; set; }

        // matricola paghe e codice fiscale (facoltativi; unici tra i dipendenti dell'azienda, controllo in Dip_AnagraficaService)
        [MaxLength(20)]
        public string? Matricola { get; set; }
        [MaxLength(16)]
        public string? CodiceFiscale { get; set; }

        public ICollection<Dip_RapportoLavoro>? Dip_RapportoLavoro { get; set; }
        public ICollection<Dip_Competenza>? Dip_Competenza { get; set; }
        

    }



}
