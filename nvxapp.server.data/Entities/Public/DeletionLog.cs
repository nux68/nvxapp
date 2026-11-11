using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    // elemento eliminato: i valori finiscono nel database, non vanno cambiati
    public enum DeletionNodeType
    {
        Dealer = 0,
        FinancialAdvisor = 1,
        Company = 2,
        UserDealer = 3,            // utente tolto da un centro (eliminato se non ha altri collegamenti)
        UserFinancialAdvisor = 4,  // utente tolto da uno studio (eliminato se non ha altri collegamenti)
        UserCompany = 5,           // utente tolto da un'azienda (eliminato se non ha altri collegamenti)
        User = 6,                  // utente eliminato con tutti i collegamenti (solo SuperUser)
    }

    /*
     Registro delle cancellazioni fisiche (vedi nvxapp.server/docs/ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md): dopo la cancellazione
     non resta altra traccia di chi ha eliminato cosa. Nessun dato personale: tipo e id dell'elemento, il suo nome
     (centro, studio, azienda o nome utente), chi ha eliminato e il riepilogo dei conteggi.
    */
    public class DeletionLog : BaseEntity
    {
        public DeletionNodeType NodeType { get; set; }

        // id del nodo o del collegamento eliminato (per un utente: vuoto, vale IdAspNetUsersDeleted)
        public int? NodeId { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsersDeleted { get; set; }

        [Required]
        [MaxLength(256)]
        public string Descrizione { get; set; } = string.Empty;

        // chi ha eliminato: l'utente che ha fatto il login (anche se ne impersonava un altro)
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? UserName { get; set; }

        public DateTime Data { get; set; }

        // riepilogo dei conteggi (JSON)
        public string? Riepilogo { get; set; }
    }
}
