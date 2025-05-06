using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_SediReparto : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        // Relazione ricorsiva: Reparto padre
        public int? IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }



        // Relazione ricorsiva: Reparti figli
        public ICollection<Az_SediReparto>? Az_SediReparto_Sub { get; set; }
        
        public ICollection<Az_SediRepartoAttivita>? Az_SediRepartoAttivita { get; set; }
        public ICollection<Az_SediRepartoUser>? Az_SediRepartoUser { get; set; }

    }



}
