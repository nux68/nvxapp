using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Persona di riferimento di un cliente (schema Common): usata dal CRM (contatto di
     opportunita' e attivita') e dal documentale (destinatario esterno).
     Principale: contatto da proporre per primo (al massimo uno per cliente, controllo nel servizio).
    */
    public class Az_ClienteContatto : BaseEntity
    {
        [Required]
        public required int IdAz_Cliente { get; set; }
        [ForeignKey("IdAz_Cliente")]
        public virtual Az_Cliente? Az_ClienteNavigation { get; set; }

        [MaxLength(50)]
        public string? Cognome { get; set; }
        [MaxLength(50)]
        public string? Nome { get; set; }

        // ruolo nell'azienda del cliente (es. "Responsabile amministrativo")
        [MaxLength(100)]
        public string? Ruolo { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }
        [MaxLength(30)]
        public string? Cellulare { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }

        public Boolean Principale { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
