using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_Cfg : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public TipoApprovazione ApprovazioneTipo { get; set; }
        
    }

    public enum TipoApprovazione
    {
        SigleAdmin,
        AllAdmin,
        AllAdminHierarchy,

    }


}
