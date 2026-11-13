using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models
{
    // Turni e pianificazione (AT-TURNI, ANALISI_ATTENDANCETRACKING_TURNI.md)

    public class Par_TurnoModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
        public int? IdPar_Orario { get; set; }
        public string? Colore { get; set; }

        // solo in lettura: orario del turno ("06:00-14:00", vuoto = riposo) e ore teoriche in minuti
        public string? OrarioTesto { get; set; }
        public int MinutiTeorici { get; set; }
    }

    public class Az_SediRepartoTurnoModel
    {
        public int Id { get; set; }
        public int IdAz_SediReparto { get; set; }
        public int IdPar_Turno { get; set; }
        public int MinPersone { get; set; }
        public string GiorniSettimana { get; set; } = "1111111";
    }

    public class Dip_TurnoModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public int IdPar_Turno { get; set; }
        public int? IdAz_SediReparto { get; set; }
        public string? Note { get; set; }
    }

    public class Par_Turno_GetAllInModel { }
    public class Par_Turno_GetAllOutModel : ModelResult
    {
        public List<Par_TurnoModel> Par_Turno { get; set; } = new();
    }

    public class Par_Turno_GetInModel { public int Id { get; set; } }
    public class Par_Turno_GetOutModel : ModelResult
    {
        public Par_TurnoModel Par_Turno { get; set; } = new();
    }

    public class Par_Turno_PutInModel { public Par_TurnoModel Par_Turno { get; set; } = new(); }
    public class Par_Turno_PutOutModel : ModelResult
    {
        public Par_TurnoModel Par_Turno { get; set; } = new();
    }

    public class Par_Turno_DeleteInModel { public int Id { get; set; } }
    public class Par_Turno_DeleteOutModel : ModelResult { }

    // turni e coperture minime di un reparto
    public class Az_SediRepartoTurno_GetInModel { public int IdAz_SediReparto { get; set; } }
    public class Az_SediRepartoTurno_GetOutModel : ModelResult
    {
        public int IdAz_SediReparto { get; set; }
        public List<Az_SediRepartoTurnoModel> Az_SediRepartoTurno { get; set; } = new();
    }

    public class Az_SediRepartoTurno_PutInModel
    {
        public int IdAz_SediReparto { get; set; }
        // elenco completo del reparto: le righe che mancano si cancellano
        public List<Az_SediRepartoTurnoModel> Az_SediRepartoTurno { get; set; } = new();
    }
    public class Az_SediRepartoTurno_PutOutModel : ModelResult
    {
        public List<Az_SediRepartoTurnoModel> Az_SediRepartoTurno { get; set; } = new();
    }

    // ── Pianificazione ─────────────────────────────────────────────────

    public class TurnoDipendenteModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public int IdDip_Anagrafica { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public string Nominativo { get; set; } = string.Empty;
    }

    public class TurnoAssenzaModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        // approvata (o inserita dall'HR) oppure ancora in approvazione
        public bool Approvata { get; set; }
        public bool GiornataIntera { get; set; }
    }

    public class TurnoCoperturaGiornoModel
    {
        public DateTime Data { get; set; }
        public int IdPar_Turno { get; set; }
        public int MinPersone { get; set; }
        public int Assegnati { get; set; }
        public bool Insufficiente { get; set; }
    }

    public class TurnoConflittoModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public string Messaggio { get; set; } = string.Empty;
        // true = turno su un'assenza approvata, false = avviso (assenza in attesa)
        public bool Grave { get; set; }
    }

    public class TurnoConsuntivoModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public int Min_Teo { get; set; }
        public int Min_Lav { get; set; }
        public long Stato { get; set; }
        // ore mancanti o errori nel giorno calcolato
        public bool Anomalia { get; set; }
    }

    public class Dip_Turno_PianificazioneInModel
    {
        public int IdAz_SediReparto { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_Turno_PianificazioneOutModel : ModelResult
    {
        public int IdAz_SediReparto { get; set; }
        public string? Reparto { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public List<TurnoDipendenteModel> Dipendenti { get; set; } = new();
        public List<Par_TurnoModel> Turni { get; set; } = new();
        public List<Az_SediRepartoTurnoModel> TurniReparto { get; set; } = new();
        public List<Dip_TurnoModel> Assegnazioni { get; set; } = new();
        public List<TurnoAssenzaModel> Assenze { get; set; } = new();
        public List<TurnoCoperturaGiornoModel> Coperture { get; set; } = new();
        public List<TurnoConflittoModel> Conflitti { get; set; } = new();
        public List<TurnoConsuntivoModel> Consuntivo { get; set; } = new();
        public int CopertureInsufficienti { get; set; }
    }

    public class Dip_Turno_PutRiga
    {
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        // 0 = togli il turno del giorno (usato se IdPar_Turni e' null: un solo turno nel giorno)
        public int IdPar_Turno { get; set; }
        // turni del giorno (piu' turni compatibili, TurniUtility.VerificaGiorno): sostituiscono quelli presenti; vuoto = nessun turno
        public List<int>? IdPar_Turni { get; set; }
        public string? Note { get; set; }
    }

    public class Dip_Turno_PutInModel
    {
        public int IdAz_SediReparto { get; set; }
        public List<Dip_Turno_PutRiga> Assegnazioni { get; set; } = new();
    }
    public class Dip_Turno_PutOutModel : ModelResult
    {
        public int Salvati { get; set; }
        public int Tolti { get; set; }
    }

    // ── Turni del dipendente e scambi ──────────────────────────────────

    public class Dip_TurnoUserModel : Dip_TurnoModel
    {
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
        public string? Colore { get; set; }
        public string? OrarioTesto { get; set; }
        public string? Reparto { get; set; }
        public string? IdAspNetUsers { get; set; }
        public string? Nominativo { get; set; }
    }

    public class Dip_TurnoScambioModel
    {
        public int Id { get; set; }
        public TurnoScambioStato Stato { get; set; }
        public int IdDip_Turno { get; set; }
        public int? IdDip_Turno_Collega { get; set; }
        public string? Richiedente { get; set; }
        public string? Collega { get; set; }
        public string? IdAspNetUsers_Richiedente { get; set; }
        public string? IdAspNetUsers_Collega { get; set; }
        public string? Testo { get; set; }
        public string? Note { get; set; }
        public int? IdDip_GG_Richiesta { get; set; }
        // l'utente corrente e' il collega e deve rispondere
        public bool DaRispondere { get; set; }
        // l'utente corrente e' il richiedente e puo' ritirarla
        public bool Annullabile { get; set; }
    }

    public class Dip_Turno_GetAll4UserInModel
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }
    public class Dip_Turno_GetAll4UserOutModel : ModelResult
    {
        public List<Dip_TurnoUserModel> MieiTurni { get; set; } = new();
        // turni dei colleghi dei miei reparti nello stesso periodo (per proporre uno scambio)
        public List<Dip_TurnoUserModel> TurniColleghi { get; set; } = new();
        // colleghi dei miei reparti (anche senza turni: per la cessione)
        public List<TurnoDipendenteModel> Colleghi { get; set; } = new();
        public List<Dip_TurnoScambioModel> Scambi { get; set; } = new();
    }

    public class Dip_TurnoScambio_ProponiInModel
    {
        public int IdDip_Turno { get; set; }
        // turno del collega (scambio) oppure, per la cessione, il collega
        public int? IdDip_Turno_Collega { get; set; }
        public string? IdAspNetUsers_Collega { get; set; }
        public string? Note { get; set; }
    }
    public class Dip_TurnoScambio_ProponiOutModel : ModelResult
    {
        public Dip_TurnoScambioModel? Dip_TurnoScambio { get; set; }
    }

    public class Dip_TurnoScambio_RispondiInModel
    {
        public int Id { get; set; }
        public bool Accetta { get; set; }
    }
    public class Dip_TurnoScambio_RispondiOutModel : ModelResult
    {
        public Dip_TurnoScambioModel? Dip_TurnoScambio { get; set; }
    }

    public class Dip_TurnoScambio_AnnullaInModel { public int Id { get; set; } }
    public class Dip_TurnoScambio_AnnullaOutModel : ModelResult { }

    // corpo della richiesta di approvazione (Dip_GG_Richiesta.Dati, TipoRichiesta.ScambioTurno)
    public class Dip_GG_Richiesta_Body_ScambioTurno
    {
        public int IdDip_TurnoScambio { get; set; }
        public string Testo { get; set; } = string.Empty;
    }
}
