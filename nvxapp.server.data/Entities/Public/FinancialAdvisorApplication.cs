using nvxapp.server.data.Infrastructure.Tenancy;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Applicativo abilitato per uno studio (FinancialAdvisor): abilita le funzioni "lato studio"
     dell'applicativo (es. documentale: area studio, distribuzione dei cedolini alle aziende).
     Non crea schemi e non fa da tetto alle aziende dello studio: un'azienda puo' avere un applicativo
     attivo anche se il suo studio non lo ha (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §4.1).
     Attivano e disattivano solo il SuperUser e il DealerPowerAdmin del centro dello studio.
     Una sola riga per studio/applicativo.
    */
    public class FinancialAdvisorApplication : BaseEntity
    {
        [Required]
        public int IdFinancialAdvisor { get; set; }
        public FinancialAdvisor? FinancialAdvisorNavigation { get; set; }

        [Required]
        public ApplicationType ApplicationType { get; set; }

        public bool Active { get; set; }

        public DateTime? ActivationDate { get; set; }

        public DateTime? DeactivationDate { get; set; }
    }
}
