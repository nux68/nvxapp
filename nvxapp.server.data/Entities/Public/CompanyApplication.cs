using nvxapp.server.data.Infrastructure.Tenancy;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Applicativo attivato per un'azienda.
        - attivo: i dati dell'applicativo sono accessibili (in multi-tenant nello schema
          tenant_<IdCompany>_<ApplicationType>)
        - disattivato: accesso bloccato, lo schema e i dati restano
     Una sola riga per coppia azienda/applicativo.
    */
    public class CompanyApplication : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        [Required]
        public ApplicationType ApplicationType { get; set; }

        public bool Active { get; set; }

        public DateTime? ActivationDate { get; set; }

        public DateTime? DeactivationDate { get; set; }
    }
}
