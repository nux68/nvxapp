using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    /*
     Timbratura evoluta (AT-TIMB, ANALISI_ATTENDANCETRACKING_TIMBRATURA.md).
       Az_SediTimbratura       configurazione della timbratura per sede (QR, geofence, kiosk, differite): tabella delle
                               Presenze collegata alla sede comune, lo schema Common non cambia
       Dip_GG_TimbraturaInfo   come e' nata una timbratura (origine, sede, posizione, esito del geofence, differita, QR):
                               tabella 1:1, cosi' le modifiche dell'HR alla timbratura non la azzerano
       Dip_TimbraturaPin       PIN e badge del dipendente per il kiosk
    */
    public class Az_SediTimbratura : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        // geofence: centro e raggio della sede, regola per chi timbra fuori (o senza posizione)
        public double? Latitudine { get; set; }
        public double? Longitudine { get; set; }
        public int RaggioMetri { get; set; } = 200;
        public RegolaGeofence RegolaGeofence { get; set; } = RegolaGeofence.Nessuna;

        // QR rotante della postazione: firma HMAC con il segreto della sede, nuovo codice ogni QrDurataSecondi
        public bool QrAttivo { get; set; }
        public bool QrObbligatorio { get; set; }
        [MaxLength(64)]
        public string? QrSegreto { get; set; }
        public int QrDurataSecondi { get; set; } = 30;

        // kiosk: dispositivo condiviso registrato con un token (registrarne un altro invalida il precedente)
        public bool KioskAttivo { get; set; }
        [MaxLength(64)]
        public string? KioskToken { get; set; }

        // timbrature differite (offline): ore massime tra l'ora del dispositivo e la ricezione (0 = non accettate)
        public int DifferitaMaxOre { get; set; } = 24;
    }

    public class Dip_GG_TimbraturaInfo : BaseEntity
    {
        [Required]
        public required int IdDip_GG_Timbratura { get; set; }
        [ForeignKey("IdDip_GG_Timbratura")]
        public virtual Dip_GG_Timbratura? Dip_GG_TimbraturaNavigation { get; set; }

        [Required]
        public required int IdDip_RapportoLavoro { get; set; }

        public int? IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        public OrigineTimbratura Origine { get; set; } = OrigineTimbratura.App;

        // timbratura fatta senza rete e inviata dopo: vale l'ora del dispositivo
        public bool Differita { get; set; }
        public DateTime? DataDispositivo { get; set; }
        public DateTime DataRicezione { get; set; }

        public double? Latitudine { get; set; }
        public double? Longitudine { get; set; }
        public double? PrecisioneMetri { get; set; }
        public double? DistanzaMetri { get; set; }
        public EsitoGeofence EsitoGeofence { get; set; } = EsitoGeofence.NonRichiesto;

        // slot del QR usato (anti-replay: indice univoco sede + slot + rapporto)
        public long? QrSlot { get; set; }

        // kiosk: utente del dispositivo che ha registrato la timbratura per il dipendente
        [MaxLength(450)]
        public string? IdAspNetUsers_Operatore { get; set; }
    }

    public class Dip_TimbraturaPin : BaseEntity
    {
        [Required]
        public required int IdDip_Anagrafica { get; set; }
        [ForeignKey("IdDip_Anagrafica")]
        public virtual Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        public required int IdAz_Anagrafica { get; set; }

        // HMAC-SHA256 del PIN (chiave del server + azienda): si cerca per valore, il PIN non si ricava
        [MaxLength(100)]
        public string? PinHash { get; set; }

        [MaxLength(50)]
        public string? Badge { get; set; }
    }

    public enum RegolaGeofence
    {
        Nessuna = 0,
        Segnala = 1,
        Blocca = 2
    }

    public enum OrigineTimbratura
    {
        App = 0,
        Qr = 1,
        Kiosk = 2
    }

    public enum EsitoGeofence
    {
        NonRichiesto = 0,
        Dentro = 1,
        Fuori = 2,
        PosizioneMancante = 3,
        LavoroAgile = 4
    }
}
