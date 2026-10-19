using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Voce di menu nascosta a un ruolo dell'azienda (configurata dal CompanyPowerAdmin).
     Si salvano solo le esclusioni: una voce senza righe resta visibile come previsto dal codice,
     quindi le voci aggiunte in futuro dagli applicativi sono visibili finche' non vengono escluse.
        - Url : url della voce di menu (es. "/vacationplan")
        - Role: ruolo a cui la voce e' nascosta ("CompanyAdmin" o "User")
     Una sola riga per azienda/voce/ruolo.
    */
    public class CompanyMenuRestriction : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Url { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
