using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    /// <summary>
    /// Pausa di un orario a coppie (ANALISI_ATTENDANCETRACKING_PAUSE.md):
    /// - Pranzo: lo stacco tra due coppie (o, su orario a coppia unica, un'uscita intermedia nella finestra);
    ///   Rilevata = la pausa va timbrata, il motore controlla e avvisa; Automatica = timbratura facoltativa,
    ///   il motore detrae la parte mancante fino a DurataMin.
    /// - Breve: uscite intermedie dentro le coppie (caffe'); vanno sempre timbrate, RetribuitaMin resta pagata.
    /// Un orario senza righe di pausa si calcola come prima.
    /// </summary>
    public class Par_OrarioPausa : BaseEntity
    {
        [Required]
        public required int IdPar_Orario { get; set; }
        [ForeignKey("IdPar_Orario")]
        public virtual Par_Orario? Par_OrarioNavigation { get; set; }

        public int NumPausa { get; set; } = 1;
        public PausaTipo Tipo { get; set; } = PausaTipo.Pranzo;

        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public PausaModalita Modalita { get; set; } = PausaModalita.Rilevata;

        // minuti: durata minima (Pranzo), oltre DurataMaxMin avviso; pausa dovuta solo se il lavorato supera SogliaMin
        public int DurataMin { get; set; }
        public int? DurataMaxMin { get; set; }
        public int SogliaMin { get; set; } = 360;

        // finestra in cui la pausa si riconosce e si colloca; vuota = dagli intervalli di riconoscimento delle coppie
        public TimeOnly? FinestraDalle { get; set; }
        public TimeOnly? FinestraAlle { get; set; }

        // minuti della pausa che restano pagati
        public int RetribuitaMin { get; set; }

        // causale (informativa) che riceve i minuti detratti
        public int? IdCausale_Pausa { get; set; }
        [ForeignKey("IdCausale_Pausa")]
        public virtual Par_Causali? Causale_PausaNavigation { get; set; }
    }

    public enum PausaTipo
    {
        Pranzo = 0,
        Breve = 1
    }

    public enum PausaModalita
    {
        Rilevata = 0,
        Automatica = 1
    }
}
