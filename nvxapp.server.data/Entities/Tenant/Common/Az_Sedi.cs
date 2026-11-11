using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Sede dell'azienda (schema Common, INF-COM.1): condivisa tra gli applicativi (presenze, HR, compliance, risorse aziendali).
     Stessi nomi di classe, tabella e campi della versione delle presenze: il ramo Presenze (AT-COM) cancella la sua copia
     e sposta i dati con ALTER TABLE ... SET SCHEMA.

     Classe partial: qui solo le navigazioni tra tabelle comuni. Le collection verso le tabelle delle presenze
     (es. Az_SediAttivita) le aggiunge il ramo Presenze in un partial (come Dip_Anagrafica.Presenze.cs);
     CommonDbContext le ignora.
    */
    public partial class Az_Sedi : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }

        // reparti della sede (nome storico "Az_Reparto", usato dal codice delle presenze)
        public ICollection<Az_SediReparto>? Az_Reparto { get; set; }
    }
}
