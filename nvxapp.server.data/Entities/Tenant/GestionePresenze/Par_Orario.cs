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

        

         
        public int? IdCausale_HH_Lav_MonteOre { get; set; }
        [ForeignKey("IdCausale_HH_Lav_MonteOre")]
        public virtual Par_Causali? Causale_HH_Lav_MonteOreNavigation { get; set; }


        public int? IdAz_SubCommessaAttivita_MonteOre { get; set; }
        [ForeignKey("IdAz_SubCommessaAttivita_MonteOre")]
        public virtual Az_SubCommessaAttivita? Az_SubCommessaAttivita_MonteOreNavigation { get; set; }





        public OrarioTimbratureTipo TimbratureTipo { get; set; }  = 0;

        public TimeOnly Hh_Teo_MonteOre { get; set; } = new TimeOnly(0, 0);


        public ICollection<Par_OrarioIntervalloHH>? Par_OrarioIntervalloHH { get; set; }

        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }


    }


    public enum OrarioTimbratureTipo
    {
        IntervalloOrario,
        MonteOre,
        MonteOreValore
    }


}
