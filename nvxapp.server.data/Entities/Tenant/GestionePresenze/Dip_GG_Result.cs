using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Result : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        public TimeOnly HH_Teo { get; set; }
        public TimeOnly HH_Lav { get; set; }
        public GG_ResultStato Stato { get; set; }

        // ── eccedenze (minuti) ──────────────────────────────────────────
        // fase giornaliera
        public int Min_Lavorabili { get; set; }   // teorico +/- giustificativi
        public int Min_Assenze { get; set; }      // giustificativi che riducono il lavorabile
        public int Min_Ord { get; set; }          // lavorato entro il lavorabile
        public int Min_Ecc { get; set; }          // lavorato - lavorabile: >0 arrotondato e oltre franchigia, <0 ore mancanti
        public int Min_EccNott { get; set; }      // parte dell'eccedenza svolta in fascia notturna
        public bool Festivo { get; set; }         // festivita' del calendario (o domenica, se il profilo la considera festiva)

        // fase di periodo: qualifica delle ore in piu' del giorno
        public bool EccProvvisoria { get; set; }  // periodo di conteggio non ancora concluso
        public int Min_Suppl { get; set; }
        public int Min_SupplEcc { get; set; }
        public int Min_Strao { get; set; }
        public int Min_StraoNott { get; set; }
        public int Min_StraoFest { get; set; }
        public int Min_StraoFestNott { get; set; }
        public int Min_EccOrd { get; set; }
        public int Min_NonRiconosciute { get; set; }
        public bool BancaOre { get; set; }        // ore qualificate accreditate in banca ore

        // profili con ore in piu' solo se autorizzate: ore oltre l'autorizzazione approvata del giorno
        // (non qualificate, senza causale); di queste, quelle coperte da una richiesta di autorizzazione in attesa
        public int Min_NonAutorizzate { get; set; }
        public int Min_DaAutorizzare { get; set; }
    }


    [Flags]
 public enum GG_ResultStato: long
{
    Init    = 1 << 0,   // 1
    OK      = 1 << 1,   // 2
    Locked  = 1 << 2,   // 4
    Warning = 1 << 3,   // 8
    Err     = 1 << 4,   // 16

    // Error details (5 → 24)
    Err_TimbratureMancanti  = 1 << 5,
    Err_2  = 1 << 6,
    Err_3  = 1 << 7,
    Err_4  = 1 << 8,
    Err_5  = 1 << 9,
    Err_6  = 1 << 10,
    Err_7  = 1 << 11,
    Err_8  = 1 << 12,
    Err_9  = 1 << 13,
    Err_10 = 1 << 14,
    Err_11 = 1 << 15,
    Err_12 = 1 << 16,
    Err_13 = 1 << 17,
    Err_14 = 1 << 18,
    Err_15 = 1 << 19,
    Err_16 = 1 << 20,
    Err_17 = 1 << 21,
    Err_18 = 1 << 22,
    Err_19 = 1 << 23,
    Err_20 = 1 << 24,

    // Warning details (25 → 44)
    Warning_1  = 1 << 25,
    Warning_2  = 1 << 26,
    Warning_3  = 1 << 27,
    Warning_4  = 1 << 28,
    Warning_5  = 1 << 29,
    Warning_6  = 1 << 30,
    Warning_7  = 1 << 31,
    Warning_8  = 1L << 32,
    Warning_9  = 1L << 33,
    Warning_10 = 1L << 34,
    Warning_11 = 1L << 35,
    Warning_12 = 1L << 36,
    Warning_13 = 1L << 37,
    Warning_14 = 1L << 38,
    Warning_15 = 1L << 39,
    Warning_16 = 1L << 40,
    Warning_17 = 1L << 41,
    Warning_18 = 1L << 42,
    Warning_19 = 1L << 43,
    Warning_20 = 1L << 44,

       STATE_MASK = Init | OK | Locked | Warning | Err
}


    //public enum GG_ResultStato
    //{
    //    Init,
    //    Err,
    //    OK,
    //    Locked
    //}




}
