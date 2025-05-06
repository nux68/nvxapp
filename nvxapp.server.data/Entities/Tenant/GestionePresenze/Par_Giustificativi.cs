using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_Giustificativi : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }


        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [MaxLength(7)]
        public string? BackgroundColor { get; set; }
        [MaxLength(7)]
        public string? TextColor { get; set; }

        public JustTipoInput TipoInput { get; set; }

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }

    }

    public enum JustTipoInput
    {
        InteraGiornate,
        Intervallo,
        Tutti
    }

}
