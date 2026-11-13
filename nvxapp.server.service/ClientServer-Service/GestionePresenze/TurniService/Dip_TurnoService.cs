using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService
{
    /*
     Pianificazione dei turni (AT-TURNI.2, ANALISI_ATTENDANCETRACKING_TURNI.md §4):
       - Pianificazione: griglia del reparto (dipendenti x giorni), coperture minime per turno e giorno, assenze
         (giustificativi con segno "-"), conflitti turno/assenza e consuntivo dei giorni calcolati (Dip_GG_Result)
       - Put: modifiche della griglia e ricalcolo dei giorni toccati (il turno prevale sul profilo, AT-TURNI.4)
       - GetAll4User: turni del dipendente, dei colleghi dei suoi reparti e scambi (AT-TURNI.3)
       - ReportPdf: report PDF della pianificazione del reparto o dei miei turni (settimana o mese), come i report delle
         pagine HR: i dati si leggono nella richiesta, il job in background scrive il PDF e il file si scarica dalla notifica
    */
    public class Dip_TurnoService : ServiceBase, IDip_TurnoService
    {
        private const int MaxGiorni = 62;

        private readonly ITurniUtility _turniUtility;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_TurnoRepository _par_TurnoRepository;
        private readonly IAz_SediRepartoTurnoRepository _az_SediRepartoTurnoRepository;
        private readonly IDip_TurnoRepository _dip_TurnoRepository;
        private readonly IDip_TurnoScambioRepository _dip_TurnoScambioRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IDip_GG_ResultRepository _dip_GG_ResultRepository;
        private readonly ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;

        public Dip_TurnoService(IMapper mapper,
                                UserManager<ApplicationUser> userManager,
                                IAspNetUsersRepository aspNetUsersRepository,
                                IOptions<JwtParameter> jwtParameter,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration,

                                ITurniUtility turniUtility,
                                IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                IPar_TurnoRepository par_TurnoRepository,
                                IAz_SediRepartoTurnoRepository az_SediRepartoTurnoRepository,
                                IDip_TurnoRepository dip_TurnoRepository,
                                IDip_TurnoScambioRepository dip_TurnoScambioRepository,
                                IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository,
                                IPar_GiustificativiRepository par_GiustificativiRepository,
                                IDip_GG_ResultRepository dip_GG_ResultRepository,
                                ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _turniUtility = turniUtility;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_TurnoRepository = par_TurnoRepository;
            _az_SediRepartoTurnoRepository = az_SediRepartoTurnoRepository;
            _dip_TurnoRepository = dip_TurnoRepository;
            _dip_TurnoScambioRepository = dip_TurnoScambioRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _dip_GG_ResultRepository = dip_GG_ResultRepository;
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        private static readonly StatoRichiesta[] StatiApprovati = { StatoRichiesta.Diretta, StatoRichiesta.Approvata };
        private static readonly StatoRichiesta[] StatiInAttesa = { StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata };

        public virtual async Task<GenericResult<Dip_Turno_PianificazioneOutModel>> Pianificazione(GenericRequest<Dip_Turno_PianificazioneInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                if (!await _turniUtility.IsAdminAsync(CurrentUserId))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonAutorizzato"));

                var dal = model.Data.Dal.Date;
                var al = model.Data.Al.Date;
                if (al < dal || (al - dal).TotalDays > MaxGiorni)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoPeriodoNonValido", MaxGiorni));

                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var reparto = _turniUtility.Reparto(model.Data.IdAz_SediReparto, idAzienda)
                              ?? throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoRepartoNonValido"));

                var retVal = new Dip_Turno_PianificazioneOutModel
                {
                    IdAz_SediReparto = reparto.Id,
                    Reparto = reparto.Descrizione,
                    Dal = dal,
                    Al = al
                };

                retVal.Dipendenti = _turniUtility.DipendentiReparto(reparto.Id, idAzienda, dal, al);
                var idRapporti = retVal.Dipendenti.Select(d => d.IdDip_RapportoLavoro).ToList();

                var turni = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda).OrderBy(t => t.Codice).ToList();
                retVal.Turni = turni.Select(_turniUtility.Modello).ToList();
                retVal.TurniReparto = _mapper.Map<List<Az_SediRepartoTurnoModel>>(
                    _az_SediRepartoTurnoRepository.FindAll(x => x.IdAz_SediReparto == reparto.Id).ToList());

                var assegnazioni = _dip_TurnoRepository.FindAll(d => idRapporti.Contains(d.IdDip_RapportoLavoro) && d.Data >= dal && d.Data <= al).ToList();
                retVal.Assegnazioni = _mapper.Map<List<Dip_TurnoModel>>(assegnazioni);

                // assenze: giustificativi con segno "-" (ferie, permessi, malattia...) approvati o in attesa
                var parGiust = _par_GiustificativiRepository.FindAll(g => g.IdAz_Anagrafica == idAzienda).ToList();
                var idAssenze = parGiust.Where(g => g.Segno == SignWithNeutral.Down && g.Tipo == TipoValore.Orario).Select(g => g.Id).ToList();
                var giust = _dip_GG_GiustificativiRepository.FindAll(g => idRapporti.Contains(g.IdDip_RapportoLavoro) && g.Data >= dal && g.Data <= al.AddDays(1) &&
                                                                          idAssenze.Contains(g.IdPar_Giustificativi))
                                                            .ToList()
                                                            .Where(g => g.Data.Date >= dal && g.Data.Date <= al &&
                                                                        (StatiApprovati.Contains(g.RichiestaStato) || StatiInAttesa.Contains(g.RichiestaStato)))
                                                            .ToList();
                foreach (var g in giust)
                {
                    var p = parGiust.First(x => x.Id == g.IdPar_Giustificativi);
                    retVal.Assenze.Add(new TurnoAssenzaModel
                    {
                        IdDip_RapportoLavoro = g.IdDip_RapportoLavoro,
                        Data = g.Data.Date,
                        Codice = p.Codice,
                        Descrizione = p.Descrizione,
                        BackgroundColor = p.BackgroundColor,
                        TextColor = p.TextColor,
                        Approvata = StatiApprovati.Contains(g.RichiestaStato),
                        GiornataIntera = g.InputType != JustificationInputType.Manual
                    });
                }

                // conflitti turno / assenza (il riposo non e' in conflitto)
                var idTurniRiposo = turni.Where(t => t.IdPar_Orario == null).Select(t => t.Id).ToHashSet();
                foreach (var a in assegnazioni.Where(a => !idTurniRiposo.Contains(a.IdPar_Turno)))
                {
                    foreach (var ass in retVal.Assenze.Where(x => x.IdDip_RapportoLavoro == a.IdDip_RapportoLavoro && x.Data == a.Data.Date))
                    {
                        var codiceTurno = turni.FirstOrDefault(t => t.Id == a.IdPar_Turno)?.Codice;
                        retVal.Conflitti.Add(new TurnoConflittoModel
                        {
                            IdDip_RapportoLavoro = a.IdDip_RapportoLavoro,
                            Data = a.Data.Date,
                            Grave = ass.Approvata && ass.GiornataIntera,
                            Messaggio = AttendanceTrackingMessages.Get(ass.Approvata ? "TurnoConflittoAssenza" : "TurnoConflittoAssenzaInAttesa", codiceTurno, ass.Codice)
                        });
                    }
                }

                // coperture: assegnati nel reparto, esclusi quelli con un'assenza approvata a giornata intera
                foreach (var tr in retVal.TurniReparto.Where(t => t.MinPersone > 0))
                {
                    for (var giorno = dal; giorno <= al; giorno = giorno.AddDays(1))
                    {
                        if (!Par_TurnoService.GiornoNellaMaschera(tr.GiorniSettimana, giorno))
                            continue;
                        int assegnati = assegnazioni.Count(a => a.Data.Date == giorno && a.IdPar_Turno == tr.IdPar_Turno &&
                                                                (a.IdAz_SediReparto == null || a.IdAz_SediReparto == reparto.Id) &&
                                                                !retVal.Assenze.Any(x => x.IdDip_RapportoLavoro == a.IdDip_RapportoLavoro && x.Data == giorno && x.Approvata && x.GiornataIntera));
                        retVal.Coperture.Add(new TurnoCoperturaGiornoModel
                        {
                            Data = giorno,
                            IdPar_Turno = tr.IdPar_Turno,
                            MinPersone = tr.MinPersone,
                            Assegnati = assegnati,
                            Insufficiente = assegnati < tr.MinPersone
                        });
                    }
                }
                retVal.CopertureInsufficienti = retVal.Coperture.Count(c => c.Insufficiente);

                // consuntivo: giorni gia' calcolati (turno previsto contro timbrature)
                var results = _dip_GG_ResultRepository.FindAll(r => idRapporti.Contains(r.IdDip_RapportoLavoro) && r.Data >= dal && r.Data <= al.AddDays(1)).ToList();
                foreach (var r in results.Where(r => r.Data.Date >= dal && r.Data.Date <= al && r.Data.Date <= DateTime.Today))
                {
                    retVal.Consuntivo.Add(new TurnoConsuntivoModel
                    {
                        IdDip_RapportoLavoro = r.IdDip_RapportoLavoro,
                        Data = r.Data.Date,
                        Min_Teo = (int)r.HH_Teo.ToTimeSpan().TotalMinutes,
                        Min_Lav = (int)r.HH_Lav.ToTimeSpan().TotalMinutes,
                        Stato = (long)r.Stato,
                        Anomalia = r.Stato.HasFlag(GG_ResultStato.Err_2)
                    });
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Turno_PutOutModel>> Put(GenericRequest<Dip_Turno_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                if (!await _turniUtility.IsAdminAsync(CurrentUserId))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonAutorizzato"));

                var retVal = new Dip_Turno_PutOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var reparto = _turniUtility.Reparto(model.Data.IdAz_SediReparto, idAzienda)
                              ?? throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoRepartoNonValido"));

                var idTurniAzienda = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda).Select(t => t.Id).ToHashSet();
                var toccati = new List<(int IdRapporto, DateTime Data)>();

                foreach (var riga in model.Data.Assegnazioni)
                {
                    var giorno = riga.Data.Date;
                    var (anagrafica, rapporto) = _turniUtility.DipendenteDelRapporto(riga.IdDip_RapportoLavoro);
                    if (anagrafica == null || rapporto == null || anagrafica.IdAz_Anagrafica != idAzienda)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoDipendenteNonValido"));
                    if (riga.IdPar_Turno > 0 && !idTurniAzienda.Contains(riga.IdPar_Turno))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonValido"));

                    var fuori = _gestionePresenzeUserUtility.VerificaGiorniNelRapporto(rapporto.Id, giorno, giorno);
                    if (fuori != null)
                    {
                        retVal.Messages.Add(new Message($"{TurniUtility.Nominativo(anagrafica)}: {fuori}", MessageType.Warning));
                        continue;
                    }

                    // turni del giorno richiesti (piu' turni compatibili, oppure uno solo con IdPar_Turno; nessuno = giorno libero)
                    var richiesti = riga.IdPar_Turni != null
                                    ? riga.IdPar_Turni.Where(x => x > 0).ToList()
                                    : (riga.IdPar_Turno > 0 ? new List<int> { riga.IdPar_Turno } : new List<int>());
                    if (richiesti.Any(x => !idTurniAzienda.Contains(x)))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonValido"));
                    var incompatibili = _turniUtility.VerificaGiorno(richiesti);
                    if (incompatibili != null)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurniGiornoNonValido", TurniUtility.Nominativo(anagrafica), giorno, incompatibili));
                    richiesti = richiesti.Distinct().ToList();

                    var esistenti = _dip_TurnoRepository.FindAll(d => d.IdDip_RapportoLavoro == rapporto.Id && d.Data == giorno).ToList();
                    bool cambiato = false;
                    // tolti: i turni del giorno non piu' richiesti (chiudono le proposte di scambio aperte)
                    foreach (var esistente in esistenti.Where(e => !richiesti.Contains(e.IdPar_Turno)))
                    {
                        await ChiudiScambiAperti(esistente.Id);
                        await _dip_TurnoRepository.DeleteAsync(esistente);
                        retVal.Tolti++;
                        cambiato = true;
                    }
                    // nuovi o confermati: reparto e note della riga
                    var note = string.IsNullOrWhiteSpace(riga.Note) ? null : riga.Note.Trim();
                    foreach (var idTurno in richiesti)
                    {
                        var turno = esistenti.FirstOrDefault(e => e.IdPar_Turno == idTurno)
                                    ?? new Dip_Turno { IdDip_RapportoLavoro = rapporto.Id, Data = giorno, IdPar_Turno = idTurno };
                        if (turno.Id != 0 && turno.IdAz_SediReparto == reparto.Id && turno.Note == note)
                            continue;
                        turno.IdAz_SediReparto = reparto.Id;
                        turno.Note = note;
                        await _dip_TurnoRepository.UpsertAsync(turno);
                        retVal.Salvati++;
                        cambiato = true;
                    }
                    if (cambiato)
                        toccati.Add((rapporto.Id, giorno));
                }

                await RicalcolaAsync(toccati);
                return retVal;
            }, isSubProcess);
        }

        // un turno cambiato o tolto chiude le proposte di scambio non ancora approvate che lo riguardano
        private async Task ChiudiScambiAperti(int idDip_Turno)
        {
            var aperti = _dip_TurnoScambioRepository.FindAll(s => (s.IdDip_Turno == idDip_Turno || s.IdDip_Turno_Collega == idDip_Turno) &&
                                                                  (s.Stato == TurnoScambioStato.InAttesaCollega))
                                                    .ToList();
            foreach (var s in aperti)
            {
                s.Stato = TurnoScambioStato.Annullato;
                await _dip_TurnoScambioRepository.UpdateAsync(s);
            }
        }

        // ricalcolo (asincrono) dei giorni toccati, per dipendente
        private async Task RicalcolaAsync(List<(int IdRapporto, DateTime Data)> toccati)
        {
            foreach (var g in toccati.GroupBy(t => t.IdRapporto))
            {
                var (anagrafica, _) = _turniUtility.DipendenteDelRapporto(g.Key);
                if (anagrafica?.IdAspNetUsers == null)
                    continue;
                var dal = g.Min(x => x.Data);
                var al = g.Max(x => x.Data);
                var req = new GenericRequest<TimeSheet_CalculateInModel>();
                req.Data = new TimeSheet_CalculateInModel
                {
                    TimeSheet_Calculate = new TimeSheet_CalculateModel
                    {
                        SelectedUserId = new List<string> { anagrafica.IdAspNetUsers },
                        Dal = dal,
                        Al = al,
                        Month = dal.Month,
                        Year = dal.Year
                    }
                };
                await _timeSheet_EngineService.Calculate(req, true);
            }
        }

        public virtual async Task<GenericResult<Dip_Turno_ReportOutModel>> ReportPdf(GenericRequest<Dip_Turno_ReportInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Turno_ReportOutModel();
                var d = model.Data;
                var dal = d.Dal.Date;
                var al = d.Al.Date;
                if (al < dal || (al - dal).TotalDays > MaxGiorni)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoPeriodoNonValido", MaxGiorni));

                // dati del report, letti come le pagine (stessi permessi)
                ReportPdf report;
                string nome;
                if (d.Tipo == TurniReportTipo.Reparto)
                {
                    var res = await Pianificazione(new GenericRequest<Dip_Turno_PianificazioneInModel>
                    {
                        Data = new Dip_Turno_PianificazioneInModel { IdAz_SediReparto = d.IdAz_SediReparto, Dal = dal, Al = al }
                    }, true);
                    if (!res.Success || res.Data == null)
                        throw new UserMessageException(string.Join(" ", res.Messages.Select(m => m.Text)));
                    report = TurniReportPdf.Reparto(res.Data, dal, al);
                    nome = $"Turni_{dal:yyyyMMdd}_{al:yyyyMMdd}";
                }
                else
                {
                    var res = await GetAll4User(new GenericRequest<Dip_Turno_GetAll4UserInModel>
                    {
                        Data = new Dip_Turno_GetAll4UserInModel { Dal = dal, Al = al }
                    }, true);
                    if (!res.Success || res.Data == null)
                        throw new UserMessageException(string.Join(" ", res.Messages.Select(m => m.Text)));
                    int idAz = await _turniUtility.IdAziendaAsync(CurrentCompany);
                    var io = _turniUtility.Dipendenti(new List<string> { CurrentUserId }, idAz, dal, al).FirstOrDefault();
                    if (io == null)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurniReport_NessunRapporto"));
                    var modelli = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAz).ToList().Select(_turniUtility.Modello).ToList();
                    report = TurniReportPdf.Miei(io.Nominativo, res.Data.MieiTurni, modelli, dal, al);
                    nome = $"MieiTurni_{dal:yyyyMMdd}_{al:yyyyMMdd}";
                }

                var jobId = Guid.NewGuid();
                retVal.JobId = jobId.ToString();
                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                {
                    Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("JobUserNotIdentified"), MessageType.Error));
                    return retVal;
                }
                int.TryParse(this.CurrentCompany, out int idCompany);
                var payload = new { d.Tipo, Dal = dal, Al = al };

                RunInBackground(async scope =>
                {
                    var longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                    Task Notifica(int progress, Message message, JobFile? file = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.Turni_Report_Pdf,
                                                                 Payload = payload,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = file?.DownloadUrl,
                                                                 FileName = file?.FileName,
                                                                 ProgressPercentage = progress,
                                                                 Message = message,
                                                                 IsFinished = isFinished
                                                             });

                    try
                    {
                        await Notifica(0, new Message { Text = AttendanceTrackingMessages.Get("Report_Avviato"), MsgType = MessageType.Information });

                        var company = await scope.ServiceProvider.GetRequiredService<ICompanyRepository>().FindByIdAsync(idCompany);
                        report.Intestazione.Insert(0, new KeyValuePair<string, string>(AttendanceTrackingMessages.Get("CartellinoPdf_Azienda"), company?.Descrizione ?? string.Empty));

                        await Notifica(80, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfWriting"), MsgType = MessageType.Information });
                        var fileName = ReportPdfWriter.Write(report, nome, jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Shift report PDF file created for job {JobId}: {FileName}", jobId, fileName);

                        await Notifica(100,
                                       new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information },
                                       NVXSystem.RegistraFileJob(fileName, userId, nome),
                                       isFinished: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                        await Notifica(100, new Message { Text = AttendanceTrackingMessages.Get("ExportFailed", ex.Message), MsgType = MessageType.Exception }, isFinished: true);
                    }
                });

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Turno_GetAll4UserOutModel>> GetAll4User(GenericRequest<Dip_Turno_GetAll4UserInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Turno_GetAll4UserOutModel();
                var dal = model.Data.Dal.Date;
                var al = model.Data.Al.Date;
                if (al < dal || (al - dal).TotalDays > MaxGiorni)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoPeriodoNonValido", MaxGiorni));

                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var io = _turniUtility.Dipendenti(new List<string> { CurrentUserId }, idAzienda, dal, al).FirstOrDefault();
                if (io == null)
                    return retVal;

                var colleghiUtenti = new List<string>();
                foreach (var idRep in _turniUtility.RepartiUtente(CurrentUserId))
                    colleghiUtenti.AddRange(_turniUtility.DipendentiReparto(idRep, idAzienda, dal, al).Select(d => d.IdAspNetUsers));
                retVal.Colleghi = _turniUtility.Dipendenti(colleghiUtenti.Distinct().Where(u => u != CurrentUserId).ToList(), idAzienda, dal, al);

                var turni = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda).ToList().Select(_turniUtility.Modello).ToList();
                Dip_TurnoUserModel Modello(Dip_Turno d, TurnoDipendenteModel dip)
                {
                    var t = turni.FirstOrDefault(x => x.Id == d.IdPar_Turno);
                    return new Dip_TurnoUserModel
                    {
                        Id = d.Id,
                        IdDip_RapportoLavoro = d.IdDip_RapportoLavoro,
                        Data = d.Data,
                        IdPar_Turno = d.IdPar_Turno,
                        IdAz_SediReparto = d.IdAz_SediReparto,
                        Note = d.Note,
                        Codice = t?.Codice,
                        Descrizione = t?.Descrizione,
                        Colore = t?.Colore,
                        OrarioTesto = t?.OrarioTesto,
                        Reparto = _turniUtility.RepartoDescrizione(d.IdAz_SediReparto),
                        IdAspNetUsers = dip.IdAspNetUsers,
                        Nominativo = dip.Nominativo
                    };
                }

                retVal.MieiTurni = _dip_TurnoRepository.FindAll(d => d.IdDip_RapportoLavoro == io.IdDip_RapportoLavoro && d.Data >= dal && d.Data <= al)
                                                       .OrderBy(d => d.Data).ToList()
                                                       .Select(d => Modello(d, io)).ToList();

                var idRapColleghi = retVal.Colleghi.Select(c => c.IdDip_RapportoLavoro).ToList();
                retVal.TurniColleghi = _dip_TurnoRepository.FindAll(d => idRapColleghi.Contains(d.IdDip_RapportoLavoro) && d.Data >= dal && d.Data <= al)
                                                           .OrderBy(d => d.Data).ToList()
                                                           .Select(d => Modello(d, retVal.Colleghi.First(c => c.IdDip_RapportoLavoro == d.IdDip_RapportoLavoro)))
                                                           .ToList();

                // scambi in cui sono richiedente o collega (ultimi 60 giorni)
                var limite = DateTime.Today.AddDays(-60);
                var scambi = _dip_TurnoScambioRepository.FindAll(s => (s.IdDip_RapportoLavoro == io.IdDip_RapportoLavoro || s.IdDip_RapportoLavoro_Collega == io.IdDip_RapportoLavoro) &&
                                                                      (s.CreationDate == null || s.CreationDate >= limite))
                                                        .OrderByDescending(s => s.Id).ToList();
                foreach (var s in scambi)
                    retVal.Scambi.Add(Dip_TurnoScambioService.Descrivi(s, _dip_TurnoRepository, _par_TurnoRepository, _turniUtility, io.IdDip_RapportoLavoro));

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDip_TurnoService : IServiceBase
    {
        Task<GenericResult<Dip_Turno_PianificazioneOutModel>> Pianificazione(GenericRequest<Dip_Turno_PianificazioneInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Turno_PutOutModel>> Put(GenericRequest<Dip_Turno_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Turno_GetAll4UserOutModel>> GetAll4User(GenericRequest<Dip_Turno_GetAll4UserInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Turno_ReportOutModel>> ReportPdf(GenericRequest<Dip_Turno_ReportInModel> model, bool isSubProcess);
    }
}
