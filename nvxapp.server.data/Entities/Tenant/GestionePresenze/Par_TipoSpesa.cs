using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Tipo di spesa delle note spese (es. vitto, alloggio, trasporti), parametro dell'azienda
    public class Par_TipoSpesa : BaseEntity
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

        // comportamento della spesa: generica (importo libero) o rimborso chilometrico (importo = km x rimborso dell'autovettura)
        public TipoSpesaTipo Tipo { get; set; } = TipoSpesaTipo.Generica;

        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }

    public enum TipoSpesaTipo
    {
        Generica,
        RimborsoKm
    }
}
