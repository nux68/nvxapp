using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Richiesta di registrazione di una nuova azienda dallo showcase, in attesa della conferma
     dell'email: l'azienda (con utenti e applicativi) si crea solo quando si apre il link ricevuto.
        - Descrizione : nome dell'azienda (gia' ripulito: solo lettere e cifre)
        - Mail        : indirizzo a cui e' stato inviato il link
        - PasswordHash: password scelta, gia' cifrata come quelle degli utenti (si svuota alla conferma)
        - PowerAdminUserName / AdminUserName: nomi scelti per gli amministratori dell'azienda
          (vuoti = quelli predefiniti, Descrizione + "_PowerAdmin" / "_Admin")
        - TokenHash   : SHA-256 del token del link (il token in chiaro sta solo nell'email)
        - ExpiresAt   : scadenza del link
        - ConfirmedAt / IdCompany: conferma avvenuta e azienda creata
     Nessuna relazione con Company: i contesti degli applicativi mappano Company e
     scoprirebbero la tabella seguendo la relazione.
    */
    public class CompanyRegistrationRequest : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Mail { get; set; } = string.Empty;

        [MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(256)]
        public string PowerAdminUserName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string AdminUserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public int? IdCompany { get; set; }
    }
}
