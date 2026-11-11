using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    /*
     Turni e pianificazione (AT-TURNI, ANALISI_ATTENDANCETRACKING_TURNI.md).
       Par_Turno            modello di turno (mattina, pomeriggio, notte, riposo) collegato a un orario
       Az_SediRepartoTurno  turno usato in un reparto (comune) con la copertura minima
       Dip_Turno            turno pianificato di un dipendente in un giorno (uno per giorno)
       Dip_TurnoScambio     proposta di scambio (o cessione) di un turno tra colleghi
    */
    public class Par_Turno : BaseEntity
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

        // orario del turno; null = riposo (nel calcolo vale l'orario festivo del profilo: 0 ore)
        public int? IdPar_Orario { get; set; }
        [ForeignKey("IdPar_Orario")]
        public virtual Par_Orario? Par_OrarioNavigation { get; set; }

        // colore della cella nella pianificazione (#rrggbb)
        [MaxLength(7)]
        public string? Colore { get; set; }
    }

    public class Az_SediRepartoTurno : BaseEntity
    {
        [Required]
        public required int IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        [Required]
        public required int IdPar_Turno { get; set; }
        [ForeignKey("IdPar_Turno")]
        public virtual Par_Turno? Par_TurnoNavigation { get; set; }

        // persone minime con questo turno nel reparto (0 = nessun controllo)
        public int MinPersone { get; set; }

        // giorni (lun..dom) in cui vale la copertura: "1111100" = solo i feriali
        [MaxLength(7)]
        public string GiorniSettimana { get; set; } = "1111111";
    }

    public class Dip_Turno : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [Required]
        public required int IdPar_Turno { get; set; }
        [ForeignKey("IdPar_Turno")]
        public virtual Par_Turno? Par_TurnoNavigation { get; set; }

        // reparto in cui si svolge il turno (per le coperture)
        public int? IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }
    }

    public class Dip_TurnoScambio : BaseEntity
    {
        // turno del richiedente
        [Required]
        public required int IdDip_Turno { get; set; }
        [ForeignKey("IdDip_Turno")]
        public virtual Dip_Turno? Dip_TurnoNavigation { get; set; }

        // turno del collega; null = cessione (il collega prende il turno del richiedente)
        public int? IdDip_Turno_Collega { get; set; }
        [ForeignKey("IdDip_Turno_Collega")]
        public virtual Dip_Turno? Dip_Turno_CollegaNavigation { get; set; }

        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Required]
        public required int IdDip_RapportoLavoro_Collega { get; set; }
        [ForeignKey("IdDip_RapportoLavoro_Collega")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoro_CollegaNavigation { get; set; }

        public TurnoScambioStato Stato { get; set; } = TurnoScambioStato.InAttesaCollega;

        // richiesta di approvazione (TipoRichiesta.ScambioTurno) creata quando il collega accetta
        public int? IdDip_GG_Richiesta { get; set; }
        [ForeignKey("IdDip_GG_Richiesta")]
        public virtual Dip_GG_Richiesta? Dip_GG_RichiestaNavigation { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public DateTime? DataRisposta { get; set; }
    }

    public enum TurnoScambioStato
    {
        InAttesaCollega = 0,
        RifiutatoCollega = 1,
        InApprovazione = 2,
        Approvato = 3,
        Rifiutato = 4,
        Annullato = 5
    }
}
