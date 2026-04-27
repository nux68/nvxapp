using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_ProfiloOrarioGG : BaseEntity
    {

        [Required]
        public required int IdPar_ProfiloOrario { get; set; }
        [ForeignKey("IdPar_ProfiloOrario")]
        public virtual Par_ProfiloOrario? Par_ProfiloOrarioNavigation { get; set; }

        public int NumGiorno { get; set; } = 0; // relazione con Par_ProfiloOrario->NumGiorniCiclo

        public int ZOrder { get; set; }  //1 = riga defaul, che deve essere sempre presente
        

        [Required]
        public required int IdPar_Orario { get; set; }
        [ForeignKey("IdPar_Orario")]
        public virtual Par_Orario? Par_OrarioNavigation { get; set; }
        
        [Required]
        public required int IdAz_SubCommessaAttivita { get; set; }
        [ForeignKey("IdAz_SubCommessaAttivita")]
        public virtual Az_SubCommessaAttivita? Az_SubCommessaAttivitaNavigation { get; set; }
        

        public ICollection<Par_Orario>? Par_Orario { get; set; }




    }





}
