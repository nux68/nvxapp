using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Richiesta da approvare (schema Common, ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis): una sola tabella per tutti gli
     applicativi. Nata nelle presenze (timbrature, giustificativi, note spese, straordinari, scambio turni), ora la usano
     anche le risorse aziendali (prenotazioni da approvare).
        - IdAz_Anagrafica : azienda della richiesta
        - IdAspNetUsers   : utente che ha fatto la richiesta ("Le mie richieste")
        - IdDip_Anagrafica: dipendente comune, se censito (le presenze lo valorizzano sempre)
        - IdDip_RapportoLavoro: rapporto di lavoro delle presenze, tabella che resta nelle presenze e verso cui Common
                            non puo' puntare: qui e' solo un numero (la FK la conosce il contesto delle presenze)
        - Dati            : corpo JSON del tipo (es. Dip_GG_Richiesta_Body_Giustificativo delle presenze, prenotazione delle risorse aziendali)
        - RichiestaApprovazioneData / RevocaApprovazioneData: cronologia JSON (Dip_GG_Richiesta_Stato_Cronology) degli
                            approvatori della richiesta e dell'eventuale revoca
     Classe partial: le navigazioni verso le tabelle delle presenze (rapporto di lavoro, giustificativi, timbrature,
     note spese) le aggiunge il ramo Presenze (Dip_GG_Richiesta.Presenze.cs); CommonDbContext le ignora.
     Scritture solo dal repository comune (Dip_GG_RichiestaCommonRepository).
    */
    public partial class Dip_GG_Richiesta : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsers { get; set; }

        public int? IdDip_Anagrafica { get; set; }
        [ForeignKey("IdDip_Anagrafica")]
        public virtual Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        // rapporto di lavoro delle presenze (senza FK in Common)
        public int? IdDip_RapportoLavoro { get; set; }

        public DateTime Data { get; set; }
        public DateTime DataA { get; set; }

        public TipoRichiesta RichiestaTipo { get; set; }
        // Campo per oggetto JSON
        public required string Dati { get; set; } = string.Empty;

        public StatoRichiesta RichiestaStato { get; set; }
        //TODO gestire json nativo
        public required string RichiestaApprovazioneData { get; set; } = string.Empty;

        // servono per gestire l'annullamento di una richiesta approvata
        public StatoRichiesta? RevocaStato { get; set; }
        //TODO gestire json nativo
        public required string RevocaApprovazioneData { get; set; } = string.Empty;
    }

    public class Dip_GG_Richiesta_Stato_Cronology
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public StatoRichiesta RichiestaStato { get; set; }
        public DateTime Data { get; set; }

        // motivo del rifiuto (richieste approvate dagli amministratori, es. prenotazioni)
        public string? Motivo { get; set; }

        public string? UsrName_DEB { get; set; } = string.Empty;
    }


    public enum TipoRichiesta
    {
        Timbratura,
        Giustificativo,
        NotaSpesa,
        ApprovazioneStraordinario,
        // scambio di turni tra colleghi (AT-TURNI.3): Dati = { idDip_TurnoScambio, testo }
        ScambioTurno,
        // prenotazione di una risorsa che richiede approvazione (risorse aziendali): Dati = { idAst_Prenotazione }
        PrenotazioneRisorsa
    }

    public enum StatoRichiesta
    {
        Diretta,

        Immessa,
        Cancellata,
        Rifiutata,
        ApprovazioneInCorso,
        ParzialmenteApprovata,

        Approvata,
    }
}
