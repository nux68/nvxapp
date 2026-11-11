using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Competenza definita dall'azienda (schema Common, INF-COM.2): condivisa tra presenze, HR e compliance.
     Stessi nomi di classe, tabella e campi della versione delle presenze (AT-COM sposta i dati).
     Classe partial: le navigazioni verso le tabelle delle presenze (es. Par_AttivitaCompetenza) restano nel ramo Presenze.
    */
    public partial class Par_Competenza : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;

        public Boolean Default { get; set; }
    }
}
