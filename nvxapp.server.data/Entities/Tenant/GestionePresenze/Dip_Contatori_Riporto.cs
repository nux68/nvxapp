using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /// <summary>
    /// Riporto di apertura anno (Mese 0) per dipendente e giustificativo.
    /// Viene calcolato automaticamente a fine anno e può essere modificato manualmente.
    /// </summary>
    public class Dip_Contatori_Riporto : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Required]
        public required int IdPar_Giustificativi { get; set; }
        [ForeignKey("IdPar_Giustificativi")]
        public virtual Par_Giustificativi? Par_GiustificativiNavigation { get; set; }

        /// <summary>
        /// Anno a cui si riferisce il riporto (es. 2025 = saldo di apertura anno 2025).
        /// </summary>
        [Required]
        public required int Anno { get; set; }

        /// <summary>
        /// Saldo portato dall'anno precedente.
        /// Calcolato automaticamente a fine anno, modificabile manualmente.
        /// </summary>
        [Required]
        public TimeSpan SaldoRiporto { get; set; }

        /// <summary>
        /// True se il valore è stato modificato manualmente rispetto al calcolo automatico.
        /// </summary>
        public bool IsManuale { get; set; }
    }
}
