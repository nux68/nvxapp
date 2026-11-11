using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    /*
     Accordo individuale di lavoro agile (AT-SW.1, ANALISI_ATTENDANCETRACKING_SMARTWORKING.md): periodo, giorni massimi
     a settimana e giorni consentiti. Il dipendente lo firma/prende visione con IAcknowledgementService (fonte "at.accordo-sw");
     DataFirma si aggiorna dallo stato della presa visione.
    */
    public class Dip_AccordoSW : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Column(TypeName = "date")]
        public DateTime Dal { get; set; }
        [Column(TypeName = "date")]
        public DateTime Al { get; set; }

        public int MaxGiorniSettimana { get; set; } = 2;

        // giorni consentiti lun..dom: "1111100" = dal lunedi' al venerdi'
        [MaxLength(7)]
        public string GiorniConsentiti { get; set; } = "1111100";

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime? DataRichiestaFirma { get; set; }
        public DateTime? DataFirma { get; set; }
    }
}
