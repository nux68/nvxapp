using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Competenza del dipendente (schema Common, INF-COM.2): una riga per dipendente e competenza (indice univoco).
     Stessi nomi di classe, tabella e campi della versione delle presenze (AT-COM sposta i dati).
    */
    public partial class Dip_Competenza : BaseEntity
    {
        [Required]
        public required int IdDip_Anagrafica { get; set; }
        [ForeignKey("IdDip_Anagrafica")]
        public virtual Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        public required int IdPar_Competenza { get; set; }
        [ForeignKey("IdPar_Competenza")]
        public virtual Par_Competenza? Par_CompetenzaNavigation { get; set; }
    }
}
