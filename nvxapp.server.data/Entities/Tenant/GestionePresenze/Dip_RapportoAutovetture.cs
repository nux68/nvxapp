using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // autovettura del dipendente usata per le spese di rimborso chilometrico (sezione "Spesa" del rapporto di lavoro)
    public class Dip_RapportoAutovetture : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        [MaxLength(15)]
        public string? Targa { get; set; }

        // rimborso per km
        [Column(TypeName = "numeric(10,4)")]
        public decimal RimborsoKM { get; set; }

        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }
}
