using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /// <summary>
    /// Definisce quante ore matura mensilmente un dipendente per un determinato giustificativo.
    /// Il dato è legato al rapporto di lavoro ed è fisso per tutto l'anno.
    /// </summary>
    public class Dip_Rapporto_Giustificativi_Maturazione : BaseEntity
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
        /// Ore che maturano ogni mese per questo giustificativo.
        /// </summary>
        [Required]
        public TimeSpan OreMaturazione { get; set; }
    }
}
