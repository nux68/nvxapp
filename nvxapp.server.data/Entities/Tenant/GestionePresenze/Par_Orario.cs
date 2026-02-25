using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_Orario : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public int NumeroCoppie { get; set; }  = 0;  // stabilisce quante righe di Par_OrarioIntervalloHH co devo essere

        [Column(TypeName = "numeric(4,2)")] 
        public decimal SogliaHHStrao { get; set; }  // definisce dopo quante ore le ore diventano strao
        /* definire meccanismo x ore strao */

        public ICollection<Par_OrarioIntervalloHH>? Par_OrarioIntervalloHH { get; set; }

        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }


    }





}
