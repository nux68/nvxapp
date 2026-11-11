using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Formula personalizzata del calcolo giornata (script JavaScript eseguito sul server con Jint, vedi ANALISI_FORMULE.md)
    public class Par_Formule : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(20)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(60)]
        public string? Titolo { get; set; }

        public string? Descrizione { get; set; }

        // testo della formula (JavaScript)
        public string? Testo { get; set; }

        // impronta SHA256 del testo: chiave della cache degli script preparati
        [MaxLength(64)]
        public string? Hash { get; set; }

        public ICollection<Par_ProfiloFormuleDett>? Par_ProfiloFormuleDett { get; set; }
    }

    // fase del calcolo in cui gira la formula
    public enum FormulaFase
    {
        Giorno = 1,   // dopo la generazione delle causali del giorno, prima della qualifica di periodo
        Finale = 2    // dopo la qualifica di straordinario/supplementare, prima della scrittura delle causali
    }
}
