using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Licenza sottoscritta da un soggetto (ANALISI_LICENZE.md §3.2): IdCompany oppure IdFinancialAdvisor.
     Conserva la fotografia del piano al momento della sottoscrizione o dell'ultimo rinnovo (ambito, livello, prezzo,
     limite utenti, DisponibileSuCompany, descrizione), cosi' resta valida anche se il listino cambia o viene eliminato
     (IdListino e IdPiano diventano null).
     Al piu' un abbonamento valido (InAttesa/Attivo) per soggetto e ambito: lo garantisce il servizio.
    */
    public class Lic_Abbonamento : BaseEntity
    {
        public int? IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        public int? IdFinancialAdvisor { get; set; }
        public FinancialAdvisor? FinancialAdvisorNavigation { get; set; }

        public int? IdListino { get; set; }
        public Lic_Listino? ListinoNavigation { get; set; }

        public int? IdPiano { get; set; }
        public Lic_Piano? PianoNavigation { get; set; }

        // fotografia del piano
        public AmbitoLicenza Ambito { get; set; }
        public LivelloLicenza Livello { get; set; }

        [MaxLength(100)]
        public string? DescrizionePiano { get; set; }

        public PeriodicitaLicenza Periodicita { get; set; }
        public decimal PrezzoBloccato { get; set; }

        [Required]
        [MaxLength(3)]
        public string Valuta { get; set; } = "EUR";

        public int? MaxUtentiBloccato { get; set; }
        public int? MaxClientiBloccato { get; set; }
        public int? MaxSpazioMBBloccato { get; set; }
        public bool DisponibileSuCompany { get; set; }

        // centro del listino d'origine (null = listino globale), per i resoconti degli incassi
        public int? IdDealerListino { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime DataScadenza { get; set; }

        public StatoAbbonamento Stato { get; set; }

        public bool RinnovoAutomatico { get; set; }

        public MetodoPagamento Metodo { get; set; }

        // riferimento della sottoscrizione presso il provider (rinnovi gestiti dal provider, fase dei pagamenti reali)
        [MaxLength(200)]
        public string? RiferimentoGateway { get; set; }

        // ultimo avviso di scadenza inviato (per non ripeterlo)
        public DateTime? DataUltimoAvviso { get; set; }
    }
}
