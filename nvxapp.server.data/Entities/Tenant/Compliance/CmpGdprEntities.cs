using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.Compliance
{
    /*
     Registro GDPR (CMP-GDPR, ANALISI_COMPLIANCE_APPLICATIVO.md §4.4), schema tenant_<IdAzienda>_6.
     Lo gestiscono gli amministratori dell'azienda e, per conto delle aziende, lo studio (FinancialAdvisorPowerAdmin/Admin).
    */

    // registro dei trattamenti (art. 30). BaseGiuridica: 0 consenso, 1 contratto, 2 obbligo legale, 3 interessi vitali,
    // 4 interesse pubblico, 5 legittimo interesse
    public class Cmp_Trattamento : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Denominazione { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Finalita { get; set; }
        public int BaseGiuridica { get; set; }
        [MaxLength(1000)]
        public string? CategorieInteressati { get; set; }
        [MaxLength(1000)]
        public string? CategorieDati { get; set; }
        // categorie particolari (art. 9) o dati giudiziari (art. 10)
        public bool DatiParticolari { get; set; }
        [MaxLength(1000)]
        public string? Destinatari { get; set; }
        [MaxLength(1000)]
        public string? Trasferimenti { get; set; }
        [MaxLength(500)]
        public string? TerminiCancellazione { get; set; }
        [MaxLength(2000)]
        public string? MisureSicurezza { get; set; }
        [MaxLength(1000)]
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
    }

    // responsabili esterni e nomine (art. 28). Tipo: 0 responsabile del trattamento, 1 sub-responsabile, 2 DPO,
    // 3 contitolare, 4 altro. Scadenza del contratto nello scadenzario (fonte cmp.responsabile)
    public class Cmp_Responsabile : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Tipo { get; set; }
        [Required]
        [MaxLength(200)]
        public string Denominazione { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? CodiceFiscale { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(30)]
        public string? Telefono { get; set; }
        // trattamenti affidati / oggetto della nomina
        [MaxLength(1000)]
        public string? Trattamenti { get; set; }
        public DateTime? DataContratto { get; set; }
        public DateTime? DataScadenza { get; set; }
        [MaxLength(200)]
        public string? Documento { get; set; }
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /*
     Registro delle violazioni (art. 33-34). Rischio: 0 improbabile (nessuna notifica), 1 rischio (notifica al Garante),
     2 rischio elevato (notifica e comunicazione agli interessati). Con rischio >= 1 e senza notifica registrata la
     scadenza delle 72 ore dalla scoperta e' nello scadenzario (fonte cmp.violazione); si chiude con la data di notifica.
    */
    public class Cmp_Violazione : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public DateTime DataScoperta { get; set; }
        public DateTime? DataViolazione { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Descrizione { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? CategorieDati { get; set; }
        public int? NumeroInteressati { get; set; }
        [MaxLength(2000)]
        public string? Conseguenze { get; set; }
        [MaxLength(2000)]
        public string? Misure { get; set; }
        public int Rischio { get; set; } = 1;
        public DateTime? DataNotificaGarante { get; set; }
        // motivi del ritardo se la notifica e' oltre le 72 ore
        [MaxLength(1000)]
        public string? MotivoRitardo { get; set; }
        public DateTime? DataComunicazioneInteressati { get; set; }
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
