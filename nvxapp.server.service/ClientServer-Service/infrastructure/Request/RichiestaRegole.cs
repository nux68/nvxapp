using Newtonsoft.Json;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Regole comuni sulle richieste (Dip_GG_Richiesta, ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis), le stesse che le
     presenze applicano nel proprio motore:
        - fase corrente: la revoca se presente, altrimenti la richiesta
        - in attesa di decisione: Immessa, ApprovazioneInCorso, ParzialmenteApprovata
        - chi puo' decidere adesso: con la strategia Singola un approvatore della fase o un amministratore dell'azienda;
          con Tutti/Gerarchia un approvatore della fase che non ha ancora deciso (in gerarchia dopo il precedente)

     Ciclo unico per tutti i tipi di richiesta (presenze, note spese, prenotazioni, ...):
        1. richiesta -> il richiedente la ritira                       -> Cancellata                     (fine)
        2. richiesta -> l'approvatore la rifiuta                         -> Rifiutata                      (fine)
        3. richiesta approvata -> annullamento chiesto -> approvato     -> annullata (revoca Approvata)   (fine)
        4. richiesta approvata -> annullamento chiesto -> respinto      -> resta Approvata (revoca Rifiutata) (fine)
        5. richiesta approvata -> annullamento chiesto -> ritirato      -> resta Approvata (revoca Cancellata) (fine)
     L'annullamento si chiede una volta sola: deciso o ritirato, la pratica e' conclusa.
    */
    public static class RichiestaRegole
    {
        public static readonly StatoRichiesta[] StatiInAttesa =
        {
            StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata
        };

        public static StatoRichiesta StatoCorrente(Dip_GG_Richiesta r) => r.RevocaStato ?? r.RichiestaStato;

        public static bool InAttesa(Dip_GG_Richiesta r) => StatiInAttesa.Contains(StatoCorrente(r));

        public static List<Dip_GG_Richiesta_Stato_Cronology> Cronologia(string? json)
            => !string.IsNullOrEmpty(json)
                ? JsonConvert.DeserializeObject<List<Dip_GG_Richiesta_Stato_Cronology>>(json) ?? new List<Dip_GG_Richiesta_Stato_Cronology>()
                : new List<Dip_GG_Richiesta_Stato_Cronology>();

        public static string Serializza(List<Dip_GG_Richiesta_Stato_Cronology> cronologia) => JsonConvert.SerializeObject(cronologia);

        /// <summary>cronologia della fase corrente (revoca se presente)</summary>
        public static List<Dip_GG_Richiesta_Stato_Cronology> CronologiaCorrente(Dip_GG_Richiesta r)
            => Cronologia(r.RevocaStato != null ? r.RevocaApprovazioneData : r.RichiestaApprovazioneData);

        /// <summary>l'utente puo' approvare o rifiutare adesso la fase corrente</summary>
        public static bool PuoDecidere(Dip_GG_Richiesta r, string idAspNetUsers, bool amministratore, StrategiaApprovazione strategia)
        {
            if (!InAttesa(r))
                return false;

            var lista = CronologiaCorrente(r);
            int idx = lista.FindIndex(x => x.IdAspNetUsers == idAspNetUsers);

            if (strategia == StrategiaApprovazione.Singola)
                return StatoCorrente(r) == StatoRichiesta.Immessa && (amministratore || idx >= 0);

            if (idx < 0 || lista[idx].RichiestaStato != StatoRichiesta.Immessa)
                return false;
            return strategia != StrategiaApprovazione.Gerarchia || idx == 0 || lista[idx - 1].RichiestaStato == StatoRichiesta.Approvata;
        }

        /// <summary>
        /// il richiedente puo' ritirare la richiesta in attesa, chiedere l'annullamento (revoca) di quella approvata o ritirare
        /// l'annullamento ancora in attesa. Come nelle presenze: una volta che l'annullamento e' deciso (rifiutato) o ritirato
        /// la pratica e' conclusa e la richiesta resta approvata
        /// </summary>
        public static bool PuoAnnullare(Dip_GG_Richiesta r) => PuoAnnullare(r.RichiestaStato, r.RevocaStato);

        public static bool PuoAnnullare(StatoRichiesta richiesta, StatoRichiesta? revoca)
            => (revoca == null && StatiInAttesa.Contains(richiesta)) || PuoChiedereAnnullamento(richiesta, revoca) || AnnullamentoInAttesa(revoca);

        /// <summary>il richiedente puo' chiedere l'annullamento: richiesta approvata e annullamento mai chiesto (una volta sola)</summary>
        public static bool PuoChiedereAnnullamento(Dip_GG_Richiesta r) => PuoChiedereAnnullamento(r.RichiestaStato, r.RevocaStato);

        public static bool PuoChiedereAnnullamento(StatoRichiesta richiesta, StatoRichiesta? revoca)
            => richiesta == StatoRichiesta.Approvata && revoca == null;

        /// <summary>la richiesta di annullamento e' ancora da decidere (il richiedente la puo' ritirare: la pratica si chiude)</summary>
        public static bool AnnullamentoInAttesa(StatoRichiesta? revoca) => revoca != null && StatiInAttesa.Contains(revoca.Value);

        /// <summary>effetto dell'annullamento del richiedente</summary>
        public static RequestCancelKind TipoAnnullamento(Dip_GG_Richiesta r)
            => AnnullamentoInAttesa(r.RevocaStato) ? RequestCancelKind.RitiraAnnullamento
             : PuoChiedereAnnullamento(r) ? RequestCancelKind.ChiediAnnullamento
             : RequestCancelKind.Ritira;

        /// <summary>
        /// testo dello stato, uguale per tutti i tipi di richiesta: la fase dell'annullamento prevale
        /// (es. "Approvata (annullamento respinto)")
        /// </summary>
        public static string TestoStato(Dip_GG_Richiesta r) => TestoStato(r.RichiestaStato, r.RevocaStato);

        public static string TestoStato(StatoRichiesta richiesta, StatoRichiesta? revoca)
            => InfrastructureMessages.Get(revoca switch
            {
                StatoRichiesta.Approvata => "RequestStateRevoked",
                StatoRichiesta.Rifiutata => "RequestStateRevokeRejected",
                StatoRichiesta.Cancellata => "RequestStateRevokeWithdrawn",
                not null => "RequestStateRevokePending",
                null => richiesta switch
                {
                    StatoRichiesta.Diretta => "RequestStateDirect",
                    StatoRichiesta.Immessa => "RequestStatePending",
                    StatoRichiesta.ApprovazioneInCorso or StatoRichiesta.ParzialmenteApprovata => "RequestStateInProgress",
                    StatoRichiesta.Rifiutata => "RequestStateRejected",
                    StatoRichiesta.Cancellata => "RequestStateCancelled",
                    _ => "RequestStateApproved",
                },
            });

        /// <summary>stato comune per le pagine</summary>
        public static RequestState StatoComune(Dip_GG_Richiesta r) => StatoCorrente(r) switch
        {
            StatoRichiesta.Approvata when r.RevocaStato != null => RequestState.Annullata,                       // revoca approvata
            StatoRichiesta.Immessa => RequestState.InAttesa,
            StatoRichiesta.ApprovazioneInCorso or StatoRichiesta.ParzialmenteApprovata => RequestState.InApprovazione,
            StatoRichiesta.Rifiutata => r.RevocaStato != null ? RequestState.Approvata : RequestState.Rifiutata,  // revoca rifiutata: la richiesta resta approvata
            StatoRichiesta.Cancellata => r.RevocaStato != null ? RequestState.Approvata : RequestState.Annullata, // revoca annullata: idem
            _ => RequestState.Approvata,
        };
    }
}
