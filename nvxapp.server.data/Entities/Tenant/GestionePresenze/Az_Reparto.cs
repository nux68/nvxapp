using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_Reparto : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        // Relazione ricorsiva: Reparto padre
        public int? IdAz_Reparto { get; set; }
        [ForeignKey("IdAz_Reparto")]
        public virtual Az_Reparto? Az_RepartoNavigation { get; set; }



        // Relazione ricorsiva: Reparti figli
        public ICollection<Az_Reparto>? Az_Reparto_Sub { get; set; }
        
        public ICollection<Az_RepartoAttivita>? Az_RepartoAttivita { get; set; }
        public ICollection<Az_RepartoUser>? Az_RepartoUser { get; set; }

    }



}
