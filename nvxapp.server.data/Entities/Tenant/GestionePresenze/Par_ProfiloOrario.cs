using nvxapp.server.data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{



    public class Par_ProfiloOrario : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public int NumGiorniCiclo { get; set; } = 0; // stabilisce quante righe di Par_ProfiloOrarioGG ci devono essere

        public TipoProfilo TipoProfilo { get; set; } = TipoProfilo.Settimanale;


        // ── Eccedenze: straordinario / lavoro supplementare ─────────────────
        // se false le ore oltre il teorico restano solo segnalate (nessuna qualifica, nessuna causale)
        public bool EccedenzeAttive { get; set; } = true;

        // periodo su cui si qualificano le ore in piu' (giorno, settimana lun-dom, mese)
        public StraoTipoConteggio TipoConteggio { get; set; } = StraoTipoConteggio.Giornaliero;

        // soglia giornaliera full-time di riferimento per i part-time (conteggio giornaliero)
        public TimeOnly? StraoSogliaHHFullTime { get; set; }

        // soglia settimanale full-time (es. 40 ore): conteggio settimanale e mensile
        [Column(TypeName = "decimal(5,2)")]
        public decimal SogliaSettimanaleFT { get; set; } = 40;

        // limite del supplementare "standard" in % delle ore settimanali contrattuali (D.Lgs 81/2015: 25%)
        [Column(TypeName = "decimal(5,2)")]
        public decimal SupplLimitePerc { get; set; } = 25;

        // arrotondamento per difetto dell'eccedenza giornaliera (blocchi) e franchigia in minuti
        public TimeRoundInterval EccArrotondamento { get; set; } = TimeRoundInterval.Min15;
        public int EccFranchigiaMin { get; set; } = 0;

        // le ore di assenza giustificata contano per raggiungere la soglia dello straordinario
        public bool AssenzeContanoSoglia { get; set; } = false;

        // le ore in piu' si riconoscono solo fino alle ore autorizzate (giustificativo "Autorizza ore in piu'" approvato)
        public bool StraoSoloAutorizzato { get; set; } = false;

        // la domenica e' considerata festiva per lo straordinario
        public bool DomenicaFestiva { get; set; } = true;

        // fascia notturna
        public TimeOnly NotturnoDalle { get; set; } = new TimeOnly(22, 0);
        public TimeOnly NotturnoAlle { get; set; } = new TimeOnly(6, 0);


        [Required]
        public required int IdPar_Orario_Festivo { get; set; }
        [ForeignKey("IdPar_Orario_Festivo")]
        public virtual Par_Orario? Par_Orario_FestivoNavigation { get; set; }


        // causali delle ore qualificate: quelle facoltative, se vuote, ricadono sulla causale base
        [Required]
        public required int IdCausale_Lavoro_Strao { get; set; }
        [ForeignKey("IdCausale_Lavoro_Strao")]
        public virtual Par_Causali? Causale_Lavoro_StraoNavigation { get; set; }

        public int? IdCausale_Lavoro_StraoNott { get; set; }
        [ForeignKey("IdCausale_Lavoro_StraoNott")]
        public virtual Par_Causali? Causale_Lavoro_StraoNottNavigation { get; set; }

        public int? IdCausale_Lavoro_StraoFest { get; set; }
        [ForeignKey("IdCausale_Lavoro_StraoFest")]
        public virtual Par_Causali? Causale_Lavoro_StraoFestNavigation { get; set; }

        public int? IdCausale_Lavoro_StraoFestNott { get; set; }
        [ForeignKey("IdCausale_Lavoro_StraoFestNott")]
        public virtual Par_Causali? Causale_Lavoro_StraoFestNottNavigation { get; set; }

        [Required]
        public required int IdCausale_Lavoro_Suppl { get; set; }
        [ForeignKey("IdCausale_Lavoro_Suppl")]
        public virtual Par_Causali? Causale_Lavoro_SupplNavigation { get; set; }

        public int? IdCausale_Lavoro_SupplEcc { get; set; }
        [ForeignKey("IdCausale_Lavoro_SupplEcc")]
        public virtual Par_Causali? Causale_Lavoro_SupplEccNavigation { get; set; }

        // ore oltre il teorico di un full-time che non superano la soglia dello straordinario
        public int? IdCausale_Lavoro_EccOrd { get; set; }
        [ForeignKey("IdCausale_Lavoro_EccOrd")]
        public virtual Par_Causali? Causale_Lavoro_EccOrdNavigation { get; set; }

        // accredito in banca ore (dipendenti con destinazione "Banca ore")
        public int? IdCausale_BancaOre { get; set; }
        [ForeignKey("IdCausale_BancaOre")]
        public virtual Par_Causali? Causale_BancaOreNavigation { get; set; }


        [Required]
        public required int IdGiustificativo_Assenza_Ingiust { get; set; }
        [ForeignKey("IdGiustificativo_Assenza_Ingiust")]
        public virtual Par_Giustificativi? Giustificativo_Assenza_IngiustNavigation { get; set; }




        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }
        public ICollection<Dip_ProfiloOrario>? Dip_ProfiloOrario { get; set; }

    }

    public enum TipoProfilo
    {
        Settimanale,
        Ciclico
    }


    public enum StraoTipoConteggio
    {
        Giornaliero,
        Settimanale,
        Mensile
    }


}
