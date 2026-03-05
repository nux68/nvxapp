using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_ProfiloOrario : BaseEntity
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

        public int NumGiorniCiclo { get; set; } = 0; // stabilisce quante righe di Par_ProfiloOrarioGG ci devono essere

        public int TipoProfilo { get; set; } = 0;  // 0= settimanale , 1= ciclico


        [Required]
        public required int IdPar_Orario_Festivo { get; set; }
        [ForeignKey("IdPar_Orario_Festivo")]
        public virtual Par_Orario? Par_Orario_FestivoNavigation { get; set; }

        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }
        public ICollection<Dip_ProfiloOrario>? Dip_ProfiloOrario { get; set; }

    }


    


}
