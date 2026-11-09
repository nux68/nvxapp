using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Pagamento di una licenza (ANALISI_LICENZE.md §3.2): uno per sottoscrizione o rinnovo.
     Resta anche dopo la cancellazione del soggetto o dell'abbonamento (IdAbbonamento diventa null; IdCompany e
     IdFinancialAdvisor senza chiave esterna), come lo storico dello spazio, per i resoconti degli incassi.
     Debug = true: giro completo senza transazione reale (§7.2).
    */
    public class Lic_Pagamento : BaseEntity
    {
        public int? IdAbbonamento { get; set; }
        public Lic_Abbonamento? AbbonamentoNavigation { get; set; }

        public int? IdCompany { get; set; }
        public int? IdFinancialAdvisor { get; set; }

        [MaxLength(100)]
        public string? Soggetto { get; set; }

        public AmbitoLicenza Ambito { get; set; }
        public LivelloLicenza Livello { get; set; }

        public decimal Importo { get; set; }

        [Required]
        [MaxLength(3)]
        public string Valuta { get; set; } = "EUR";

        public PeriodicitaLicenza Periodicita { get; set; }
        public DateTime DataInizioCoperta { get; set; }
        public DateTime DataFineCoperta { get; set; }

        public MetodoPagamento Metodo { get; set; }
        public StatoPagamento Stato { get; set; }

        [MaxLength(200)]
        public string? RiferimentoGateway { get; set; }

        public bool Debug { get; set; }

        public int? IdDealerListino { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
