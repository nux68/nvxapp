using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_Causali : BaseEntity
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

        // ore (Dip_GG_Causali.Valore) oppure importo in euro (Dip_GG_Causali.Importo); non si cambia se la causale e' gia' usata
        public TipoValore Tipo { get; set; } = TipoValore.Orario;

        public ICollection<Dip_GG_Causali>? Dip_GG_Causali { get; set; }
        public ICollection<Par_ExportCau_Causali>? Par_ExportCau_Causali { get; set; }
        
    }




    /// <summary>valore di causali e giustificativi: ore oppure un importo in euro (2 decimali)</summary>
    public enum TipoValore
    {
        Orario = 0,
        Importo = 1
    }
}
