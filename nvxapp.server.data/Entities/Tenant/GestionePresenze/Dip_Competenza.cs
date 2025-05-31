using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_Competenza : BaseEntity
    {
        [Required]
        public required int IdDip_Anagrafica { get; set; }
        [ForeignKey("IdDip_Anagrafica")]
        public virtual Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        public required int IdAz_Competenza { get; set; }
        [ForeignKey("IdAz_Competenza")]
        public virtual Az_Competenza? Az_CompetenzaNavigation { get; set; }
    }
}
