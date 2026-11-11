using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Reparto di una sede (schema Common, INF-COM.1), ad albero: un reparto puo' avere un reparto padre.
     Stessi nomi di classe, tabella e campi della versione delle presenze (AT-COM sposta i dati).

     Classe partial: le collection verso le tabelle delle presenze (Az_SediRepartoAttivita, Az_SediRepartoUser,
     Az_SubCommessaSediReparto) le aggiunge il ramo Presenze in un partial; CommonDbContext le ignora.
    */
    public partial class Az_SediReparto : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }

        // Relazione ricorsiva: Reparto padre
        public int? IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        // Relazione ricorsiva: Reparti figli
        public ICollection<Az_SediReparto>? Az_SediReparto_Sub { get; set; }
    }
}
