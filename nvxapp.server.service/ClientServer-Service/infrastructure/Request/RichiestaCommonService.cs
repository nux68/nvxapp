using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Motore delle richieste approvate dagli amministratori dell'azienda (ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis),
     sulla tabella comune Dip_GG_Richiesta. Lo usano i moduli che non hanno un proprio ciclo di approvazione
     (oggi le prenotazioni delle risorse aziendali); le presenze hanno il proprio (Dip_GG_RichiestaService).
        - Crea: richiesta dell'utente collegato nell'azienda corrente, stato Immessa, cronologia = amministratori
          dell'azienda (escluso il richiedente); strategia Singola: basta un amministratore
        - Approva / Rifiuta: solo un amministratore dell'azienda, con la richiesta ancora da decidere
        - Annulla: solo il richiedente. Richiesta da decidere: si ritira. Richiesta approvata: si apre la richiesta di annullamento
          (RevocaStato/RevocaApprovazioneData, come la revoca delle presenze) che gli amministratori approvano o rifiutano;
          un annullamento ancora da decidere si puo' ritirare. Rifiutata: fine del ciclo
        - Scadi: annullamento automatico (es. prenotazione non decisa all'ora di inizio), senza controllo dell'utente
     Avvisi: notifica (categoria "richieste") agli amministratori per una nuova richiesta e al richiedente per l'esito,
     se il modulo non le ha disattivate (RichiestaAvviso.Notifica); SignalR "RequestNotifier" sempre.
     Gli effetti sui dati del modulo (es. stato della prenotazione) li applica il modulo, prima o dopo la chiamata.
    */
    public class RichiestaCommonService : ServiceBase, IRichiestaCommonService
    {
        private readonly IDip_GG_RichiestaCommonRepository _dip_GG_RichiestaRepository;
        private readonly IAz_AnagraficaRepository _az_AnagraficaRepository;
        private readonly ICompanyRecipients _companyRecipients;
        private readonly INotificationSender _notificationSender;
        private readonly IRequestNotifier _requestNotifier;

        public RichiestaCommonService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      IDip_GG_RichiestaCommonRepository dip_GG_RichiestaRepository,
                                      IAz_AnagraficaRepository az_AnagraficaRepository,
                                      ICompanyRecipients companyRecipients,
                                      INotificationSender notificationSender,
                                      IRequestNotifier requestNotifier) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _az_AnagraficaRepository = az_AnagraficaRepository;
            _companyRecipients = companyRecipients;
            _notificationSender = notificationSender;
            _requestNotifier = requestNotifier;
        }

        public async Task<Dip_GG_Richiesta> CreaAsync(RichiestaNuova nuova)
        {
            var (idCompany, idAz) = await AziendaAsync();
            var admins = (await _companyRecipients.AdminsAsync(idCompany)).Where(x => x != this.CurrentUserId).ToList();
            if (admins.Count == 0)
                throw new UserMessageException(InfrastructureMessages.Get("RequestNoApprover"));

            var r = await _dip_GG_RichiestaRepository.CreateAsync(new Dip_GG_Richiesta
            {
                IdAz_Anagrafica = idAz,
                IdAspNetUsers = this.CurrentUserId,
                IdDip_Anagrafica = nuova.IdDip_Anagrafica,
                Data = nuova.Dal,
                DataA = nuova.Al,
                RichiestaTipo = nuova.Tipo,
                Dati = nuova.Dati,
                RichiestaStato = StatoRichiesta.Immessa,
                RichiestaApprovazioneData = RichiestaRegole.Serializza(admins.Select(x => new Dip_GG_Richiesta_Stato_Cronology
                {
                    IdAspNetUsers = x,
                    RichiestaStato = StatoRichiesta.Immessa,
                    Data = DateTime.Now,
                }).ToList()),
                RevocaApprovazioneData = string.Empty,
            });

            await NotificaAsync(nuova.Avviso, admins, idCompany, InfrastructureMessages.Get("RequestNotifyNew"), nuova.Avviso.Descrizione, "/requestsadmin");
            await _requestNotifier.NotifyAsync(admins.Append(this.CurrentUserId), r.Id.ToString());
            return r;
        }

        public async Task<FaseRichiesta> ApprovaAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso)
            => await DecidiAsync(richiesta, StatoRichiesta.Approvata, null, avviso);

        public async Task<FaseRichiesta> RifiutaAsync(Dip_GG_Richiesta richiesta, string? motivo, RichiestaAvviso avviso)
            => await DecidiAsync(richiesta, StatoRichiesta.Rifiutata, motivo, avviso);

        public async Task<EsitoAnnullamento> AnnullaAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso, bool sostituita = false)
        {
            var (idCompany, _) = await AziendaAsync();
            var r = await CaricaAsync(richiesta.Id);
            if (r.IdAspNetUsers != this.CurrentUserId)
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));

            EsitoAnnullamento esito;
            if (r.RevocaStato != null && RichiestaRegole.StatiInAttesa.Contains(r.RevocaStato.Value))
            {
                // richiesta di annullamento ancora da decidere: si ritira, la richiesta resta approvata
                r.RevocaStato = StatoRichiesta.Cancellata;
                esito = EsitoAnnullamento.RevocaRitirata;
            }
            else if (r.RevocaStato == null && RichiestaRegole.StatiInAttesa.Contains(r.RichiestaStato))
            {
                // richiesta ancora da decidere: si ritira
                r.RichiestaStato = StatoRichiesta.Cancellata;
                esito = EsitoAnnullamento.Ritirata;
            }
            else if (r.RichiestaStato == StatoRichiesta.Approvata && r.RevocaStato != StatoRichiesta.Approvata && sostituita)
            {
                // la richiesta approvata e' sostituita da una nuova (es. prenotazione modificata): si chiude senza approvazione
                r.RichiestaStato = StatoRichiesta.Cancellata;
                esito = EsitoAnnullamento.Ritirata;
            }
            else if (r.RichiestaStato == StatoRichiesta.Approvata && r.RevocaStato != StatoRichiesta.Approvata)
            {
                // richiesta approvata: l'annullamento va approvato dagli amministratori (come la revoca delle presenze)
                var admins = (await _companyRecipients.AdminsAsync(idCompany)).Where(x => x != this.CurrentUserId).ToList();
                if (admins.Count == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNoApprover"));
                r.RevocaStato = StatoRichiesta.Immessa;
                r.RevocaApprovazioneData = RichiestaRegole.Serializza(admins.Select(x => new Dip_GG_Richiesta_Stato_Cronology
                {
                    IdAspNetUsers = x,
                    RichiestaStato = StatoRichiesta.Immessa,
                    Data = DateTime.Now,
                }).ToList());
                await _dip_GG_RichiestaRepository.UpdateAsync(r);
                await NotificaAsync(avviso, admins, idCompany, InfrastructureMessages.Get("RequestNotifyNewRevoke"), avviso.Descrizione, "/requestsadmin");
                await _requestNotifier.NotifyAsync(await DestinatariAsync(idCompany, r), r.Id.ToString());
                return EsitoAnnullamento.RevocaRichiesta;
            }
            else
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

            await _dip_GG_RichiestaRepository.UpdateAsync(r);
            await _requestNotifier.NotifyAsync(await DestinatariAsync(idCompany, r), r.Id.ToString());
            return esito;
        }

        public async Task ScadiAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso, int idCompany)
        {
            var r = await _dip_GG_RichiestaRepository.FindAll(x => x.Id == richiesta.Id).FirstOrDefaultAsync();
            if (r == null || !RichiestaRegole.InAttesa(r))
                return;

            r.RichiestaStato = StatoRichiesta.Cancellata;
            await _dip_GG_RichiestaRepository.UpdateAsync(r);
            if (r.IdAspNetUsers != null)
                await NotificaAsync(avviso, new[] { r.IdAspNetUsers }, idCompany, InfrastructureMessages.Get("RequestNotifyExpired"), avviso.Descrizione, "/requests");
            await _requestNotifier.NotifyAsync(await DestinatariAsync(idCompany, r), r.Id.ToString());
        }

        // ---------------------------------------------------------------------------

        private async Task<FaseRichiesta> DecidiAsync(Dip_GG_Richiesta richiesta, StatoRichiesta esito, string? motivo, RichiestaAvviso avviso)
        {
            var (idCompany, _) = await AziendaAsync();
            if (!(await _companyRecipients.AdminsAsync(idCompany)).Contains(this.CurrentUserId))
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));

            var r = await CaricaAsync(richiesta.Id);
            if (!RichiestaRegole.PuoDecidere(r, this.CurrentUserId, true, StrategiaApprovazione.Singola))
                throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

            // fase corrente: la richiesta di annullamento se c'e' (PuoDecidere ha gia' verificato che sia da decidere), altrimenti la richiesta
            var fase = r.RevocaStato != null ? FaseRichiesta.Revoca : FaseRichiesta.Richiesta;
            var lista = RichiestaRegole.Cronologia(fase == FaseRichiesta.Revoca ? r.RevocaApprovazioneData : r.RichiestaApprovazioneData);
            var mia = lista.FirstOrDefault(x => x.IdAspNetUsers == this.CurrentUserId);
            if (mia == null)
            {
                mia = new Dip_GG_Richiesta_Stato_Cronology { IdAspNetUsers = this.CurrentUserId };
                lista.Add(mia);
            }
            mia.RichiestaStato = esito;
            mia.Data = DateTime.Now;
            mia.Motivo = motivo;
            if (fase == FaseRichiesta.Revoca)
            {
                r.RevocaApprovazioneData = RichiestaRegole.Serializza(lista);
                r.RevocaStato = esito;
            }
            else
            {
                r.RichiestaApprovazioneData = RichiestaRegole.Serializza(lista);
                r.RichiestaStato = esito;
            }
            await _dip_GG_RichiestaRepository.UpdateAsync(r);

            if (r.IdAspNetUsers != null)
            {
                var titolo = InfrastructureMessages.Get(fase == FaseRichiesta.Revoca
                    ? (esito == StatoRichiesta.Approvata ? "RequestNotifyRevokeApproved" : "RequestNotifyRevokeRejected")
                    : (esito == StatoRichiesta.Approvata ? "RequestNotifyApproved" : "RequestNotifyRejected"));
                var testo = esito == StatoRichiesta.Rifiutata && !string.IsNullOrWhiteSpace(motivo)
                    ? avviso.Descrizione + "\n" + InfrastructureMessages.Get("RequestNotifyReason", motivo)
                    : avviso.Descrizione;
                await NotificaAsync(avviso, new[] { r.IdAspNetUsers }, idCompany, titolo, testo, "/requests");
            }
            await _requestNotifier.NotifyAsync(await DestinatariAsync(idCompany, r), r.Id.ToString());
            return fase;
        }

        private async Task<Dip_GG_Richiesta> CaricaAsync(int id)
        {
            var (_, idAz) = await AziendaAsync();
            return await _dip_GG_RichiestaRepository.FindAll(x => x.Id == id && x.IdAz_Anagrafica == idAz).FirstOrDefaultAsync()
                   ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));
        }

        private async Task<List<string?>> DestinatariAsync(int idCompany, Dip_GG_Richiesta r)
            => (await _companyRecipients.AdminsAsync(idCompany)).Cast<string?>().Append(r.IdAspNetUsers).ToList();

        private async Task NotificaAsync(RichiestaAvviso avviso, IEnumerable<string> utenti, int idCompany, string titolo, string? testo, string url)
        {
            if (!avviso.Notifica)
                return;
            try
            {
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = utenti.ToList(),
                    IdCompany = idCompany,
                    Categoria = NotificationCategory.Richieste.Code,
                    Titolo = titolo,
                    Messaggio = testo,
                    Url = avviso.Url ?? url,
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Richieste: notifica non inviata ({Titolo})", titolo);
            }
        }

        private int? _idCompany;
        private int _idAz;

        // azienda corrente: id della Company e della sua Az_Anagrafica (Common)
        private async Task<(int IdCompany, int IdAz)> AziendaAsync()
        {
            if (_idCompany == null)
            {
                if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));
                _idAz = await _az_AnagraficaRepository.FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync()
                        ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));
                _idCompany = idCompany;
            }
            return (_idCompany.Value, _idAz);
        }
    }

    /// <summary>fase decisa da un'approvazione o da un rifiuto: la richiesta o la sua richiesta di annullamento (revoca)</summary>
    public enum FaseRichiesta { Richiesta = 0, Revoca = 1 }

    /// <summary>
    /// effetto di AnnullaAsync: Ritirata = richiesta da decidere ritirata (o approvata sostituita da una nuova);
    /// RevocaRichiesta = richiesta approvata, aperta la richiesta di annullamento da approvare; RevocaRitirata = ritirata la richiesta di annullamento
    /// </summary>
    public enum EsitoAnnullamento { Ritirata = 0, RevocaRichiesta = 1, RevocaRitirata = 2 }

    /// <summary>nuova richiesta da far approvare agli amministratori dell'azienda</summary>
    public class RichiestaNuova
    {
        public TipoRichiesta Tipo { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        /// <summary>corpo JSON del tipo</summary>
        public string Dati { get; set; } = string.Empty;
        public RichiestaAvviso Avviso { get; set; } = new RichiestaAvviso();
    }

    /// <summary>testo degli avvisi della richiesta (tradotto dal modulo) e invio delle notifiche</summary>
    public class RichiestaAvviso
    {
        /// <summary>es. "Sala riunioni, 13/11/2026 10:00-11:00"</summary>
        public string Descrizione { get; set; } = string.Empty;
        /// <summary>false = il modulo ha disattivato le notifiche (SignalR resta)</summary>
        public bool Notifica { get; set; } = true;
        /// <summary>pagina da aprire dalla notifica (default: /requests o /requestsadmin)</summary>
        public string? Url { get; set; }
    }

    public interface IRichiestaCommonService : IServiceBase
    {
        Task<Dip_GG_Richiesta> CreaAsync(RichiestaNuova nuova);
        /// <summary>decide la fase corrente (richiesta o richiesta di annullamento) e restituisce quale</summary>
        Task<FaseRichiesta> ApprovaAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso);
        Task<FaseRichiesta> RifiutaAsync(Dip_GG_Richiesta richiesta, string? motivo, RichiestaAvviso avviso);
        /// <summary>
        /// annullamento da parte del richiedente: ritira una richiesta (o un annullamento) da decidere; su una richiesta approvata
        /// apre la richiesta di annullamento da approvare, salvo sostituita = true (sostituita da una nuova richiesta: si chiude)
        /// </summary>
        Task<EsitoAnnullamento> AnnullaAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso, bool sostituita = false);
        /// <summary>annullamento automatico (senza controllo dell'utente): idCompany esplicito, vale anche da un job</summary>
        Task ScadiAsync(Dip_GG_Richiesta richiesta, RichiestaAvviso avviso, int idCompany);
    }
}
