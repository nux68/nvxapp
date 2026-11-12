using nvxapp.server.data.Repositories.Tenant.Common;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService
{
    /*
     Scambio di turni tra colleghi (AT-TURNI.3, ANALISI_ATTENDANCETRACKING_TURNI.md §5):
       1. Proponi: il dipendente propone lo scambio di un suo turno con quello di un collega (o la cessione)
       2. Rispondi: il collega accetta (nasce la richiesta TipoRichiesta.ScambioTurno del richiedente, con gli
          approvatori del suo reparto) o rifiuta
       3. l'approvazione e la revoca passano da Dip_GG_RichiestaService.SetState, che chiama IDip_TurnoScambioHandler
    */
    public class Dip_TurnoScambioService : ServiceBase, IDip_TurnoScambioService
    {
        private readonly ITurniUtility _turniUtility;
        private readonly IDip_TurnoRepository _dip_TurnoRepository;
        private readonly IDip_TurnoScambioRepository _dip_TurnoScambioRepository;
        private readonly IPar_TurnoRepository _par_TurnoRepository;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IDip_GG_RichiestaCommonRepository _dip_GG_RichiestaRepository;
        private readonly IDip_TurnoScambioHandler _handler;
        private readonly INotificationSender _notificationSender;

        public Dip_TurnoScambioService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration,

                                       ITurniUtility turniUtility,
                                       IDip_TurnoRepository dip_TurnoRepository,
                                       IDip_TurnoScambioRepository dip_TurnoScambioRepository,
                                       IPar_TurnoRepository par_TurnoRepository,
                                       IDip_GG_RichiestaService dip_GG_RichiestaService,
                                       IDip_GG_RichiestaCommonRepository dip_GG_RichiestaRepository,
                                       IDip_TurnoScambioHandler handler,
                                       INotificationSender notificationSender) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _turniUtility = turniUtility;
            _dip_TurnoRepository = dip_TurnoRepository;
            _dip_TurnoScambioRepository = dip_TurnoScambioRepository;
            _par_TurnoRepository = par_TurnoRepository;
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _handler = handler;
            _notificationSender = notificationSender;
        }

        public virtual async Task<GenericResult<Dip_TurnoScambio_ProponiOutModel>> Proponi(GenericRequest<Dip_TurnoScambio_ProponiInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_TurnoScambio_ProponiOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var oggi = DateTime.Today;

                var mio = _dip_TurnoRepository.FindAll(d => d.Id == model.Data.IdDip_Turno).FirstOrDefault();
                var (ioAna, _) = mio != null ? _turniUtility.DipendenteDelRapporto(mio.IdDip_RapportoLavoro) : (null, null);
                if (mio == null || ioAna == null || ioAna.IdAspNetUsers != CurrentUserId || ioAna.IdAz_Anagrafica != idAzienda)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoNonTuo"));
                if (mio.Data.Date < oggi)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoPassato"));

                Dip_Turno? suo = null;
                int idRapportoCollega;
                if (model.Data.IdDip_Turno_Collega is int idSuo && idSuo > 0)
                {
                    suo = _dip_TurnoRepository.FindAll(d => d.Id == idSuo).FirstOrDefault();
                    if (suo == null || suo.IdDip_RapportoLavoro == mio.IdDip_RapportoLavoro)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoCollegaNonValido"));
                    if (suo.Data.Date < oggi)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoPassato"));
                    idRapportoCollega = suo.IdDip_RapportoLavoro;
                }
                else
                {
                    // cessione: il collega prende il mio turno
                    var collega = string.IsNullOrEmpty(model.Data.IdAspNetUsers_Collega) ? null
                                  : _turniUtility.Dipendenti(new List<string> { model.Data.IdAspNetUsers_Collega }, idAzienda, mio.Data, mio.Data).FirstOrDefault();
                    if (collega == null || collega.IdAspNetUsers == CurrentUserId)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoCollegaNonValido"));
                    idRapportoCollega = collega.IdDip_RapportoLavoro;
                }

                var (collegaAna, _) = _turniUtility.DipendenteDelRapporto(idRapportoCollega);
                if (collegaAna == null || collegaAna.IdAz_Anagrafica != idAzienda)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoCollegaNonValido"));

                // stesso reparto: il collega deve lavorare in uno dei reparti del richiedente
                var mieiReparti = _turniUtility.RepartiUtente(CurrentUserId);
                if (!_turniUtility.RepartiUtente(collegaAna.IdAspNetUsers!).Any(mieiReparti.Contains))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoAltroReparto"));

                if (_dip_TurnoScambioRepository.FindAll(s => (s.IdDip_Turno == mio.Id || s.IdDip_Turno_Collega == mio.Id) &&
                                                             (s.Stato == TurnoScambioStato.InAttesaCollega || s.Stato == TurnoScambioStato.InApprovazione)).Any())
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoGiaProposto"));

                var scambio = new Dip_TurnoScambio
                {
                    IdDip_Turno = mio.Id,
                    IdDip_Turno_Collega = suo?.Id,
                    IdDip_RapportoLavoro = mio.IdDip_RapportoLavoro,
                    IdDip_RapportoLavoro_Collega = idRapportoCollega,
                    Stato = TurnoScambioStato.InAttesaCollega,
                    Note = string.IsNullOrWhiteSpace(model.Data.Note) ? null : model.Data.Note.Trim()
                };

                var errore = _handler.Verifica(scambio, avanti: true);
                if (errore != null)
                    throw new UserMessageException(errore);

                scambio = await _dip_TurnoScambioRepository.CreateAsync(scambio);
                retVal.Dip_TurnoScambio = Descrivi(scambio, _dip_TurnoRepository, _par_TurnoRepository, _turniUtility, mio.IdDip_RapportoLavoro);

                await Notifica(collegaAna.IdAspNetUsers!, int.TryParse(CurrentCompany, out var idc) ? idc : null,
                               AttendanceTrackingMessages.Get("ScambioTurnoNotificaProposta"), retVal.Dip_TurnoScambio.Testo);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_TurnoScambio_RispondiOutModel>> Rispondi(GenericRequest<Dip_TurnoScambio_RispondiInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_TurnoScambio_RispondiOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);

                var scambio = _dip_TurnoScambioRepository.FindAll(s => s.Id == model.Data.Id).FirstOrDefault();
                var (collegaAna, _) = scambio != null ? _turniUtility.DipendenteDelRapporto(scambio.IdDip_RapportoLavoro_Collega) : (null, null);
                if (scambio == null || collegaAna == null || collegaAna.IdAspNetUsers != CurrentUserId || collegaAna.IdAz_Anagrafica != idAzienda)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoNonTrovato"));
                if (scambio.Stato != TurnoScambioStato.InAttesaCollega)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoGiaRisposto"));

                var (richAna, _) = _turniUtility.DipendenteDelRapporto(scambio.IdDip_RapportoLavoro);
                int? idCompany = int.TryParse(CurrentCompany, out var idc) ? idc : null;
                var testo = Descrivi(scambio, _dip_TurnoRepository, _par_TurnoRepository, _turniUtility, scambio.IdDip_RapportoLavoro).Testo ?? string.Empty;

                scambio.DataRisposta = DateTime.Now;
                if (!model.Data.Accetta)
                {
                    scambio.Stato = TurnoScambioStato.RifiutatoCollega;
                    await _dip_TurnoScambioRepository.UpdateAsync(scambio);
                    if (richAna?.IdAspNetUsers != null)
                        await Notifica(richAna.IdAspNetUsers, idCompany, AttendanceTrackingMessages.Get("ScambioTurnoNotificaRifiutata"), testo);
                }
                else
                {
                    var errore = _handler.Verifica(scambio, avanti: true);
                    if (errore != null)
                        throw new UserMessageException(errore);

                    // richiesta di approvazione del richiedente: stessi approvatori e stati delle altre richieste
                    var date = _handler.DateScambio(scambio);
                    var req = new GenericRequest<Dip_GG_Richiesta_Send_InModel>();
                    req.Data.IdAspNetUsers = richAna!.IdAspNetUsers;
                    req.Data.ExcludeRicalc = true;
                    req.Data.Dip_GG_Richiesta = new Dip_GG_RichiestaModel
                    {
                        Id = 0,
                        IdDip_RapportoLavoro = scambio.IdDip_RapportoLavoro,
                        Data = date.Min().ToString("dd/MM/yyyy"),
                        DataA = date.Max().ToString("dd/MM/yyyy"),
                        RichiestaTipo = TipoRichiesta.ScambioTurno,
                        RichiestaStato = StatoRichiesta.Immessa,
                        Dati = JsonConvert.SerializeObject(new Dip_GG_Richiesta_Body_ScambioTurno { IdDip_TurnoScambio = scambio.Id, Testo = testo })
                    };
                    var res = await _dip_GG_RichiestaService.Send(req, false);
                    var richiesta = res.Data?.Dip_GG_Richiesta.FirstOrDefault();
                    if (!res.Success || richiesta == null || richiesta.Id <= 0)
                    {
                        var msg = res.Data?.Messages.FirstOrDefault()?.Text ?? res.Messages?.FirstOrDefault()?.Text ?? AttendanceTrackingMessages.Get("NoApproverEnabled");
                        throw new UserMessageException(msg);
                    }

                    scambio.Stato = TurnoScambioStato.InApprovazione;
                    scambio.IdDip_GG_Richiesta = richiesta.Id;
                    await _dip_TurnoScambioRepository.UpdateAsync(scambio);
                    if (richAna.IdAspNetUsers != null)
                        await Notifica(richAna.IdAspNetUsers, idCompany, AttendanceTrackingMessages.Get("ScambioTurnoNotificaAccettata"), testo);
                }

                retVal.Dip_TurnoScambio = Descrivi(scambio, _dip_TurnoRepository, _par_TurnoRepository, _turniUtility, scambio.IdDip_RapportoLavoro_Collega);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_TurnoScambio_AnnullaOutModel>> Annulla(GenericRequest<Dip_TurnoScambio_AnnullaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var scambio = _dip_TurnoScambioRepository.FindAll(s => s.Id == model.Data.Id).FirstOrDefault();
                var (richAna, _) = scambio != null ? _turniUtility.DipendenteDelRapporto(scambio.IdDip_RapportoLavoro) : (null, null);
                if (scambio == null || richAna?.IdAspNetUsers != CurrentUserId)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoNonTrovato"));
                // ritirabile solo prima della risposta del collega; dopo si annulla la richiesta (pagina Richieste)
                if (scambio.Stato != TurnoScambioStato.InAttesaCollega)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ScambioTurnoGiaRisposto"));
                scambio.Stato = TurnoScambioStato.Annullato;
                await _dip_TurnoScambioRepository.UpdateAsync(scambio);
                return new Dip_TurnoScambio_AnnullaOutModel();
            }, isSubProcess);
        }

        private async Task Notifica(string idAspNetUsers, int? idCompany, string titolo, string? testo)
        {
            try
            {
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = new[] { idAspNetUsers },
                    IdCompany = idCompany,
                    Titolo = titolo,
                    Messaggio = testo,
                    Url = "/mieiturni"
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Scambio turni: notifica non inviata");
            }
        }

        // descrizione dello scambio vista dal rapporto indicato
        public static Dip_TurnoScambioModel Descrivi(Dip_TurnoScambio s, IDip_TurnoRepository turnoRepo, IPar_TurnoRepository parTurnoRepo, ITurniUtility util, int idRapportoVista)
        {
            var mio = turnoRepo.FindAll(d => d.Id == s.IdDip_Turno).FirstOrDefault();
            var suo = s.IdDip_Turno_Collega != null ? turnoRepo.FindAll(d => d.Id == s.IdDip_Turno_Collega).FirstOrDefault() : null;
            var (rich, _) = util.DipendenteDelRapporto(s.IdDip_RapportoLavoro);
            var (coll, _) = util.DipendenteDelRapporto(s.IdDip_RapportoLavoro_Collega);
            string Turno(Dip_Turno? d) => d == null ? string.Empty
                : $"{d.Data:dd/MM} {parTurnoRepo.FindAll(t => t.Id == d.IdPar_Turno).Select(t => t.Codice).FirstOrDefault()}";

            var richiedente = rich != null ? TurniUtility.Nominativo(rich) : string.Empty;
            var collega = coll != null ? TurniUtility.Nominativo(coll) : string.Empty;
            var testo = suo != null
                ? AttendanceTrackingMessages.Get("ScambioTurnoTesto", richiedente, Turno(mio), collega, Turno(suo))
                : AttendanceTrackingMessages.Get("ScambioTurnoTestoCessione", richiedente, Turno(mio), collega);

            return new Dip_TurnoScambioModel
            {
                Id = s.Id,
                Stato = s.Stato,
                IdDip_Turno = s.IdDip_Turno,
                IdDip_Turno_Collega = s.IdDip_Turno_Collega,
                Richiedente = richiedente,
                Collega = collega,
                IdAspNetUsers_Richiedente = rich?.IdAspNetUsers,
                IdAspNetUsers_Collega = coll?.IdAspNetUsers,
                Testo = testo,
                Note = s.Note,
                IdDip_GG_Richiesta = s.IdDip_GG_Richiesta,
                DaRispondere = s.Stato == TurnoScambioStato.InAttesaCollega && s.IdDip_RapportoLavoro_Collega == idRapportoVista,
                Annullabile = s.Stato == TurnoScambioStato.InAttesaCollega && s.IdDip_RapportoLavoro == idRapportoVista
            };
        }
    }

    public interface IDip_TurnoScambioService : IServiceBase
    {
        Task<GenericResult<Dip_TurnoScambio_ProponiOutModel>> Proponi(GenericRequest<Dip_TurnoScambio_ProponiInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_TurnoScambio_RispondiOutModel>> Rispondi(GenericRequest<Dip_TurnoScambio_RispondiInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_TurnoScambio_AnnullaOutModel>> Annulla(GenericRequest<Dip_TurnoScambio_AnnullaInModel> model, bool isSubProcess);
    }

    /*
     Esito della richiesta ScambioTurno (chiamato da Dip_GG_RichiestaService.SetState):
       - ApplicaAsync: richiesta approvata -> scambio dei turni e ricalcolo dei giorni
       - EsitoNegativoAsync: rifiutata/annullata -> stato dello scambio; revoca approvata -> scambio annullato (turni come prima)
     Lo scambio e' un'involuzione: rifarlo riporta i turni come prima.
       stesso giorno:   si scambiano turno, reparto e note tra le due righe
       giorni diversi:  si scambiano i proprietari delle due righe
       cessione:        la riga passa al collega (avanti) o torna al richiedente (indietro)
    */
    public class Dip_TurnoScambioHandler : IDip_TurnoScambioHandler
    {
        private readonly IDip_TurnoRepository _dip_TurnoRepository;
        private readonly IDip_TurnoScambioRepository _dip_TurnoScambioRepository;
        private readonly ITurniUtility _turniUtility;
        private readonly ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;

        public Dip_TurnoScambioHandler(IDip_TurnoRepository dip_TurnoRepository,
                                       IDip_TurnoScambioRepository dip_TurnoScambioRepository,
                                       ITurniUtility turniUtility,
                                       ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService)
        {
            _dip_TurnoRepository = dip_TurnoRepository;
            _dip_TurnoScambioRepository = dip_TurnoScambioRepository;
            _turniUtility = turniUtility;
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        public List<DateTime> DateScambio(Dip_TurnoScambio s)
        {
            var date = new List<DateTime>();
            var a = _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno).FirstOrDefault();
            if (a != null) date.Add(a.Data.Date);
            if (s.IdDip_Turno_Collega != null)
            {
                var b = _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno_Collega).FirstOrDefault();
                if (b != null) date.Add(b.Data.Date);
            }
            return date.Count > 0 ? date : new List<DateTime> { DateTime.Today };
        }

        // null = lo scambio si puo' fare (avanti) o disfare (indietro), altrimenti il motivo
        public string? Verifica(Dip_TurnoScambio s, bool avanti)
        {
            var a = _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno).FirstOrDefault();
            var b = s.IdDip_Turno_Collega != null ? _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno_Collega).FirstOrDefault() : null;
            if (a == null || (s.IdDip_Turno_Collega != null && b == null))
                return AttendanceTrackingMessages.Get("ScambioTurnoTurniCambiati");

            // proprietari attesi prima dell'operazione
            int ownerA = avanti ? s.IdDip_RapportoLavoro : s.IdDip_RapportoLavoro_Collega;
            int ownerB = avanti ? s.IdDip_RapportoLavoro_Collega : s.IdDip_RapportoLavoro;
            if (b != null && a.Data.Date == b.Data.Date)
            {
                // stesso giorno: le righe restano ai loro proprietari
                if (a.IdDip_RapportoLavoro != s.IdDip_RapportoLavoro || b.IdDip_RapportoLavoro != s.IdDip_RapportoLavoro_Collega)
                    return AttendanceTrackingMessages.Get("ScambioTurnoTurniCambiati");
                return null;
            }

            if (a.IdDip_RapportoLavoro != ownerA || (b != null && b.IdDip_RapportoLavoro != ownerB))
                return AttendanceTrackingMessages.Get("ScambioTurnoTurniCambiati");

            // chi riceve una riga non deve avere gia' un turno in quel giorno
            if (_dip_TurnoRepository.FindAll(d => d.IdDip_RapportoLavoro == ownerB && d.Data == a.Data && d.Id != a.Id && (b == null || d.Id != b.Id)).Any())
                return AttendanceTrackingMessages.Get("ScambioTurnoGiornoOccupato", a.Data);
            if (b != null && _dip_TurnoRepository.FindAll(d => d.IdDip_RapportoLavoro == ownerA && d.Data == b.Data && d.Id != a.Id && d.Id != b.Id).Any())
                return AttendanceTrackingMessages.Get("ScambioTurnoGiornoOccupato", b.Data);
            return null;
        }

        private async Task<string?> Esegui(Dip_TurnoScambio s, bool avanti)
        {
            var errore = Verifica(s, avanti);
            if (errore != null)
                return errore;

            var a = _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno).First();
            var b = s.IdDip_Turno_Collega != null ? _dip_TurnoRepository.FindAll(d => d.Id == s.IdDip_Turno_Collega).First() : null;

            if (b != null && a.Data.Date == b.Data.Date)
            {
                (a.IdPar_Turno, b.IdPar_Turno) = (b.IdPar_Turno, a.IdPar_Turno);
                (a.IdAz_SediReparto, b.IdAz_SediReparto) = (b.IdAz_SediReparto, a.IdAz_SediReparto);
                (a.Note, b.Note) = (b.Note, a.Note);
                await _dip_TurnoRepository.UpdateAsync(a);
                await _dip_TurnoRepository.UpdateAsync(b);
            }
            else
            {
                a.IdDip_RapportoLavoro = avanti ? s.IdDip_RapportoLavoro_Collega : s.IdDip_RapportoLavoro;
                await _dip_TurnoRepository.UpdateAsync(a);
                if (b != null)
                {
                    b.IdDip_RapportoLavoro = avanti ? s.IdDip_RapportoLavoro : s.IdDip_RapportoLavoro_Collega;
                    await _dip_TurnoRepository.UpdateAsync(b);
                }
            }

            // ricalcolo dei giorni dei due dipendenti
            var date = new List<DateTime> { a.Data.Date };
            if (b != null) date.Add(b.Data.Date);
            foreach (var idRapporto in new[] { s.IdDip_RapportoLavoro, s.IdDip_RapportoLavoro_Collega })
            {
                var (ana, _) = _turniUtility.DipendenteDelRapporto(idRapporto);
                if (ana?.IdAspNetUsers == null)
                    continue;
                var req = new GenericRequest<TimeSheet_CalculateInModel>();
                req.Data = new TimeSheet_CalculateInModel
                {
                    TimeSheet_Calculate = new TimeSheet_CalculateModel
                    {
                        SelectedUserId = new List<string> { ana.IdAspNetUsers },
                        Dal = date.Min(),
                        Al = date.Max(),
                        Month = date.Min().Month,
                        Year = date.Min().Year
                    }
                };
                try { await _timeSheet_EngineService.Calculate(req, true); }
                catch (Exception ex) { Log.Warning(ex, "Scambio turni {Id}: ricalcolo non avviato", s.Id); }
            }
            return null;
        }

        private Dip_TurnoScambio? Scambio(Dip_GG_Richiesta richiesta)
        {
            Dip_GG_Richiesta_Body_ScambioTurno? body = null;
            try { body = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_ScambioTurno>(richiesta.Dati ?? string.Empty); }
            catch { body = null; }
            if (body == null)
                return null;
            return _dip_TurnoScambioRepository.FindAll(s => s.Id == body.IdDip_TurnoScambio && s.IdDip_GG_Richiesta == richiesta.Id).FirstOrDefault();
        }

        public async Task<string?> ApplicaAsync(Dip_GG_Richiesta richiesta)
        {
            if (richiesta.RichiestaStato != StatoRichiesta.Approvata)
                return null;
            var s = Scambio(richiesta);
            if (s == null || s.Stato != TurnoScambioStato.InApprovazione)
                return null;

            var errore = await Esegui(s, avanti: true);
            // turni cambiati nel frattempo: lo scambio non si applica e si annulla (la richiesta resta approvata)
            s.Stato = errore == null ? TurnoScambioStato.Approvato : TurnoScambioStato.Annullato;
            await _dip_TurnoScambioRepository.UpdateAsync(s);
            return errore == null ? null : AttendanceTrackingMessages.Get("ScambioTurnoNonApplicato", errore);
        }

        public async Task<string?> EsitoNegativoAsync(Dip_GG_Richiesta richiesta)
        {
            var s = Scambio(richiesta);
            if (s == null)
                return null;

            string? errore = null;
            if (s.Stato == TurnoScambioStato.Approvato)
            {
                // revoca approvata: si rimettono i turni come prima
                errore = await Esegui(s, avanti: false);
                s.Stato = TurnoScambioStato.Annullato;
            }
            else if (s.Stato == TurnoScambioStato.InApprovazione)
            {
                s.Stato = richiesta.RichiestaStato == StatoRichiesta.Rifiutata ? TurnoScambioStato.Rifiutato : TurnoScambioStato.Annullato;
            }
            else
                return null;

            await _dip_TurnoScambioRepository.UpdateAsync(s);
            return errore == null ? null : AttendanceTrackingMessages.Get("ScambioTurnoNonRipristinato", errore);
        }
    }

    public interface IDip_TurnoScambioHandler
    {
        List<DateTime> DateScambio(Dip_TurnoScambio s);
        string? Verifica(Dip_TurnoScambio s, bool avanti);
        Task<string?> ApplicaAsync(Dip_GG_Richiesta richiesta);
        Task<string?> EsitoNegativoAsync(Dip_GG_Richiesta richiesta);
    }
}
