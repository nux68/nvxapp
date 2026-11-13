using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService.Models
{
    // Timbratura evoluta (AT-TIMB, ANALISI_ATTENDANCETRACKING_TIMBRATURA.md)

    public class Az_SediTimbraturaModel
    {
        public int IdAz_Sedi { get; set; }
        public string? Sede { get; set; }
        public double? Latitudine { get; set; }
        public double? Longitudine { get; set; }
        public int RaggioMetri { get; set; } = 200;
        public RegolaGeofence RegolaGeofence { get; set; }
        public bool QrAttivo { get; set; }
        public bool QrObbligatorio { get; set; }
        public int QrDurataSecondi { get; set; } = 30;
        public bool KioskAttivo { get; set; }
        public int DifferitaMaxOre { get; set; } = 24;

        // solo lettura: c'e' un dispositivo kiosk registrato
        public bool KioskRegistrato { get; set; }
        // solo scrittura: genera un nuovo segreto del QR (i codici esposti smettono di valere)
        public bool RigeneraSegretoQr { get; set; }
    }

    public class TimbraturaEvoluta_SediInModel { }
    public class TimbraturaEvoluta_SediOutModel : ModelResult
    {
        public List<Az_SediTimbraturaModel> Sedi { get; set; } = new();
        // sede proposta all'utente (primo reparto in cui lavora)
        public int? IdAz_Sedi_Proposta { get; set; }
    }

    public class TimbraturaEvoluta_ConfigGetInModel { public int IdAz_Sedi { get; set; } }
    public class TimbraturaEvoluta_ConfigGetOutModel : ModelResult
    {
        public Az_SediTimbraturaModel Config { get; set; } = new();
    }

    public class TimbraturaEvoluta_ConfigPutInModel { public Az_SediTimbraturaModel Config { get; set; } = new(); }
    public class TimbraturaEvoluta_ConfigPutOutModel : ModelResult
    {
        public Az_SediTimbraturaModel Config { get; set; } = new();
    }

    public class TimbraturaEvoluta_QrCodeInModel
    {
        public int IdAz_Sedi { get; set; }
        // indirizzo del client (es. https://app.example.com): il QR apre <Origine>/timbraturaqr?c=<codice>
        public string? Origine { get; set; }
    }
    public class TimbraturaEvoluta_QrCodeOutModel : ModelResult
    {
        public string Codice { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        // immagine PNG del QR in base64
        public string ImmaginePng { get; set; } = string.Empty;
        public int ScadeTraSecondi { get; set; }
        public int DurataSecondi { get; set; }
    }

    public class TimbraturaEvoluta_StampInModel
    {
        // reparto-attivita' scelti tra le opzioni della sede (0 = l'opzione di default)
        public int IdAz_SubCommessaAttivita { get; set; }
        public int? IdAz_SediReparto { get; set; }
        // codice del QR letto (facoltativo)
        public string? Codice { get; set; }
        // ticket restituito da Opzioni dopo la verifica del codice: sostituisce il codice (la scelta non fa scadere il QR)
        public string? Ticket { get; set; }
        public int? IdAz_Sedi { get; set; }
        public double? Latitudine { get; set; }
        public double? Longitudine { get; set; }
        public double? PrecisioneMetri { get; set; }
        // timbratura fatta offline: vale l'ora del dispositivo (ISO locale, es. 2026-11-12T08:01:00)
        public bool Offline { get; set; }
        public string? DataDispositivo { get; set; }
    }

    public class Dip_GG_TimbraturaInfoModel
    {
        public int IdDip_GG_Timbratura { get; set; }
        public DateTime Timbratura { get; set; }
        public int? IdAz_Sedi { get; set; }
        public string? Sede { get; set; }
        public int? IdAz_SediReparto { get; set; }
        public string? Reparto { get; set; }
        public OrigineTimbratura Origine { get; set; }
        public bool Differita { get; set; }
        public DateTime? DataDispositivo { get; set; }
        public DateTime DataRicezione { get; set; }
        public double? DistanzaMetri { get; set; }
        public EsitoGeofence EsitoGeofence { get; set; }
        public string? Operatore { get; set; }
    }

    public class TimbraturaEvoluta_StampOutModel : ModelResult
    {
        public Dip_GG_TimbraturaInfoModel? Info { get; set; }
    }

    // reparto - attivita' proposti al dipendente che timbra in una sede (AT-TIMB.7)
    public class TimbraturaOpzioneModel
    {
        public int? IdAz_SediReparto { get; set; }
        public string? Reparto { get; set; }
        public int IdAz_SubCommessaAttivita { get; set; }
        public string Attivita { get; set; } = string.Empty;
        // cliente (se non quello di default) · commessa · sub commessa
        public string? Commessa { get; set; }
    }

    // opzioni per la timbratura dell'utente collegato: dal codice del QR (verificato: torna il ticket) o dalla sede scelta
    public class TimbraturaEvoluta_OpzioniInModel
    {
        public string? Codice { get; set; }
        public int? IdAz_Sedi { get; set; }
    }
    public class TimbraturaEvoluta_OpzioniOutModel : ModelResult
    {
        public int? IdAz_Sedi { get; set; }
        public string? Ticket { get; set; }
        public List<TimbraturaOpzioneModel> Opzioni { get; set; } = new();
        public int IndiceDefault { get; set; }
    }

    public class TimbraturaEvoluta_KioskRegistraInModel { public int IdAz_Sedi { get; set; } }
    public class TimbraturaEvoluta_KioskRegistraOutModel : ModelResult
    {
        public string KioskToken { get; set; } = string.Empty;
    }

    // uscita dalla modalita' terminale con la password di un CompanyPowerAdmin dell'azienda
    public class TimbraturaEvoluta_TerminaleSbloccaInModel { public string? Password { get; set; } }
    public class TimbraturaEvoluta_TerminaleSbloccaOutModel : ModelResult
    {
        public bool Sbloccato { get; set; }
    }

    // kiosk: PIN o badge -> dipendente e opzioni reparto-attivita' della sede del kiosk
    public class TimbraturaEvoluta_KioskIdentificaInModel
    {
        public int IdAz_Sedi { get; set; }
        public string? KioskToken { get; set; }
        public string? Pin { get; set; }
        public string? Badge { get; set; }
    }
    public class TimbraturaEvoluta_KioskIdentificaOutModel : ModelResult
    {
        public string? Nominativo { get; set; }
        public List<TimbraturaOpzioneModel> Opzioni { get; set; } = new();
        public int IndiceDefault { get; set; }
    }

    public class TimbraturaEvoluta_KioskStampInModel : TimbraturaEvoluta_KioskIdentificaInModel
    {
        // reparto-attivita' scelti (0 = l'opzione di default)
        public int IdAz_SubCommessaAttivita { get; set; }
        public int? IdAz_SediReparto { get; set; }
    }
    public class TimbraturaEvoluta_KioskStampOutModel : ModelResult
    {
        public string? Nominativo { get; set; }
        public DateTime Ora { get; set; }
        public Dip_GG_TimbraturaInfoModel? Info { get; set; }
    }

    public class Dip_TimbraturaPinModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string? Nominativo { get; set; }
        public bool PinImpostato { get; set; }
        public string? Badge { get; set; }
    }

    public class TimbraturaEvoluta_PinGetAllInModel { }
    public class TimbraturaEvoluta_PinGetAllOutModel : ModelResult
    {
        public List<Dip_TimbraturaPinModel> Dipendenti { get; set; } = new();
    }

    public class TimbraturaEvoluta_PinPutInModel
    {
        // dipendente a cui l HR assegna PIN e badge (vuoto = l utente corrente)
        public string? IdAspNetUsers { get; set; }
        // null = non cambia, "" = toglie
        public string? Pin { get; set; }
        public string? Badge { get; set; }
    }
    public class TimbraturaEvoluta_PinPutOutModel : ModelResult { }

    public class TimbraturaEvoluta_Info4GiornoInModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
    }
    public class TimbraturaEvoluta_Info4GiornoOutModel : ModelResult
    {
        public List<Dip_GG_TimbraturaInfoModel> Info { get; set; } = new();
    }
}
