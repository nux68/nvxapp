using Newtonsoft.Json;
using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Regole comuni sulle richieste (Dip_GG_Richiesta, ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis), le stesse che le
     presenze applicano nel proprio motore:
        - fase corrente: la revoca se presente, altrimenti la richiesta
        - in attesa di decisione: Immessa, ApprovazioneInCorso, ParzialmenteApprovata
        - chi puo' decidere adesso: con la strategia Singola un approvatore della fase o un amministratore dell'azienda;
          con Tutti/Gerarchia un approvatore della fase che non ha ancora deciso (in gerarchia dopo il precedente)
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

        /// <summary>il richiedente puo' annullare (in attesa) o chiedere la revoca (approvata, senza revoca in corso)</summary>
        public static bool PuoAnnullare(Dip_GG_Richiesta r)
        {
            if (r.RevocaStato == null)
                return StatiInAttesa.Contains(r.RichiestaStato) || r.RichiestaStato == StatoRichiesta.Approvata || r.RichiestaStato == StatoRichiesta.Diretta;
            // revoca in attesa: si puo' ritirare; revoca rifiutata o ritirata: si puo' chiedere di nuovo
            return r.RevocaStato != StatoRichiesta.Approvata;
        }

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
