using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Cartellino di quadratura mensile di un rapporto di lavoro.
     I totali (in minuti) sono scritti dal motore di calcolo a ogni ricalcolo del mese, finche' il mese
     non e' approvato: da quel momento i giorni sono bloccati e i totali restano quelli approvati.

     Stato:
       Provvisorio  - mese (o ultima settimana del mese) ancora in corso
       DaApprovare  - periodo concluso, in attesa della validazione HR
       Approvato    - consolidato: giorni bloccati, pronto per le paghe, accredito banca ore effettivo
    */
    public class Dip_Periodo : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public int Anno { get; set; }
        public int Mese { get; set; }

        public PeriodoStato Stato { get; set; } = PeriodoStato.Provvisorio;

        // totali del mese in minuti
        public int Min_Teo { get; set; }
        public int Min_Lav { get; set; }
        public int Min_Ord { get; set; }
        public int Min_Assenze { get; set; }
        public int Min_Ecc { get; set; }            // ore oltre il lavorabile (dopo arrotondamenti e franchigie)
        public int Min_Compensate { get; set; }     // ore in piu' compensate da ore in meno nello stesso periodo
        public int Min_Provvisorie { get; set; }    // ore in piu' in attesa di quadratura (periodo in corso)
        public int Min_Suppl { get; set; }
        public int Min_SupplEcc { get; set; }
        public int Min_Strao { get; set; }
        public int Min_StraoNott { get; set; }
        public int Min_StraoFest { get; set; }
        public int Min_StraoFestNott { get; set; }
        public int Min_EccOrd { get; set; }
        public int Min_NonRiconosciute { get; set; }
        public int Min_BancaOre { get; set; }       // parte delle ore qualificate accreditata in banca ore
        public int Min_NonAutorizzate { get; set; } // ore in piu' oltre le autorizzazioni approvate
        public int Min_DaAutorizzare { get; set; }  // di cui con una richiesta di autorizzazione in attesa

        // tetto delle ore in piu' riconosciute nel mese deciso dall'HR (null = tutte, 0 = rifiutate)
        public int? Min_Riconosciute { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime? DataApprovazione { get; set; }
        [MaxLength(450)]
        public string? IdAspNetUsers_Approvazione { get; set; }
    }

    public enum PeriodoStato
    {
        Provvisorio,
        DaApprovare,
        Approvato
    }
}
