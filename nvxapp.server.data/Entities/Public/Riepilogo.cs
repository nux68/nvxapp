using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Riepilogo periodico (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, area INF-AI): testo scritto dall'AI (o, se l'AI non e'
     configurata o non risponde, generato dai fatti senza AI) a partire dai fatti strutturati che ogni modulo fornisce
     con un IDigestProvider. Si conserva per la consultazione nella pagina "Riepiloghi".
        - Azienda: uno per ogni amministratore dell'azienda (IdCompany + IdAspNetUsers), con i fatti visti da lui
        - Studio : uno per lo studio (IdFinancialAdvisor, IdAspNetUsers null), con le aziende dello studio che hanno
                   criticita'; lo vedono gli utenti dello studio
     Tabella in public (come Scadenza e Notifica). Chiavi esterne senza navigation: i contesti degli applicativi mappano
     Company, FinancialAdvisor e AspNetUsers e non devono scoprire questa tabella.
     Gli enum si salvano come interi: i valori non vanno rinumerati.
    */
    public enum TipoRiepilogo
    {
        Azienda = 0,
        Studio = 1,
    }

    public class Riepilogo : BaseEntity
    {
        public TipoRiepilogo Tipo { get; set; }

        /// <summary>azienda (riepilogo di tipo Azienda)</summary>
        public int? IdCompany { get; set; }

        /// <summary>studio (riepilogo di tipo Studio)</summary>
        public int? IdFinancialAdvisor { get; set; }

        /// <summary>destinatario (riepilogo di tipo Azienda); null = tutti gli utenti dello studio</summary>
        [MaxLength(450)]
        public string? IdAspNetUsers { get; set; }

        /// <summary>periodo riepilogato (giorni compresi)</summary>
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        /// <summary>testo del riepilogo</summary>
        [Required]
        public string Testo { get; set; } = string.Empty;

        /// <summary>fatti strutturati da cui e' stato scritto il testo (JSON delle sezioni dei moduli)</summary>
        [Required]
        public string Fatti { get; set; } = "[]";

        /// <summary>testo scritto dall'AI (false = testo semplice generato dai fatti)</summary>
        public bool GeneratoConAI { get; set; }

        /// <summary>provider AI usato (es. "AI_deepseek"), null senza AI</summary>
        [MaxLength(50)]
        public string? ProviderAI { get; set; }

        /// <summary>generato con "Genera ora" (false = generazione settimanale automatica)</summary>
        public bool Manuale { get; set; }
    }
}
