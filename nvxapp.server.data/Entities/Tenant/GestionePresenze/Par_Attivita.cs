using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_Attivita : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
        [MaxLength(7)]
        public string? BackgroundColor { get; set; }
        [MaxLength(7)]
        public string? TextColor { get; set; }
        public Boolean Default { get; set; }

        public ICollection<Par_AttivitaCompetenza>? Par_AttivitaCompetenza { get; set; }
    }
}
