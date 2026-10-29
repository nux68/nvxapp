using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService
{
    /*
     Report PDF delle pagine HR (pulsante Report del FAB): statistiche attivita' (totali per attivita' e per
     dipendente), piano ferie, quadratura mensile (per reparto) e note spese (per tipo di spesa e per dipendente).
     Come il cartellino PDF (TimeSheet_ExportService): la richiesta avvia un job in background, il file
     si scarica dalla notifica di fine job. I dati si leggono con gli stessi servizi delle pagine, per i
     dipendenti selezionati: il CompanyPowerAdmin li vede tutti, il CompanyAdmin solo quelli dei reparti che
     amministra (gli altri id ricevuti dal client vengono scartati).
    */
    public class ReportService : ServiceBase, IReportService
    {
        // fasi (richiesta o revoca) ancora da decidere: nel piano ferie il giustificativo e' segnato "in attesa"
        private static readonly StatoRichiesta[] StatiInAttesa = { StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata };

        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IAz_SediRepartoRepository _az_SediRepartoRepository;

        public ReportService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,

                             IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                             IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                             IAz_SediRepartoRepository az_SediRepartoRepository
                             ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _az_SediRepartoRepository = az_SediRepartoRepository;
        }

        public virtual async Task<GenericResult<Report_OutModel>> ReportExportPdf(GenericRequest<Report_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Report_OutModel();
                var richiesta = model.Data.Report;

                if (richiesta.Month < 1 || richiesta.Month > 12 || richiesta.Year < 2000 || richiesta.Year > 2100)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfInvalidPeriod", richiesta.Month, richiesta.Year), MessageType.Error));
                    return retVal;
                }

                // dipendenti richiesti, limitati a quelli che l'utente puo' vedere
                var usersId = (richiesta.SelectedUserId ?? new List<string>()).Distinct().ToList();
                if (!await HaRuolo("CompanyPowerAdmin"))
                {
                    if (!await HaRuolo("CompanyAdmin"))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ExportPdfAdminsOnly"));
                    var dipendentiAmministrati = DipendentiAmministrati(this.CurrentUserId);
                    usersId = usersId.Where(u => dipendentiAmministrati.Contains(u)).ToList();
                }

                // solo i dipendenti dell'azienda, in ordine alfabetico
                var dipendenti = (await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(usersId))
                                     .GroupBy(d => d.IdAspNetUsers)
                                     .Select(g => g.First())
                                     .Select(d => new Dipendente(d.IdAspNetUsers, NomeDipendente(d.Cognome, d.Nome, d.UserName)))
                                     .OrderBy(d => d.Nome, StringComparer.CurrentCultureIgnoreCase)
                                     .ToList();
                if (dipendenti.Count == 0)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfNoEmployees"), MessageType.Warning));
                    return retVal;
                }

                // quadratura: gruppi per reparto calcolati qui (repository della richiesta)
                var reparti = richiesta.Tipo == ReportTipo.QuadraturaMensile ? GruppiReparto(richiesta.RepartiId, dipendenti) : new List<GruppoReparto>();

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
                var payload = new { richiesta.Tipo, richiesta.Year, richiesta.Month };

                RunInBackground(async scope =>
                {
                    var longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                    Task Notifica(int progress, Message message, JobFile? file = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.Report_Pdf,
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
                        await Notifica(0, new Message { Text = T("Report_Avviato"), MsgType = MessageType.Information });

                        var company = await scope.ServiceProvider.GetRequiredService<ICompanyRepository>().FindByIdAsync(idCompany);
                        var ctx = new Contesto(scope.ServiceProvider, richiesta, dipendenti, reparti, company?.Descrizione ?? string.Empty);

                        ReportPdf report = richiesta.Tipo switch
                        {
                            ReportTipo.AttivitaTotali => await ReportAttivita(ctx, perDipendente: false),
                            ReportTipo.AttivitaPerDipendente => await ReportAttivita(ctx, perDipendente: true),
                            ReportTipo.PianoFerie => await ReportPianoFerie(ctx),
                            ReportTipo.QuadraturaMensile => await ReportQuadratura(ctx),
                            ReportTipo.NotaSpesePerTipo => await ReportNotaSpese(ctx, perDipendente: false),
                            ReportTipo.NotaSpesePerDipendente => await ReportNotaSpese(ctx, perDipendente: true),
                            _ => throw new UserMessageException(T("Report_TipoNonValido"))
                        };

                        await Notifica(80, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfWriting"), MsgType = MessageType.Information });

                        var nome = $"Report_{richiesta.Tipo}_{richiesta.Year}-{richiesta.Month:00}";
                        var fileName = ReportPdfWriter.Write(report, nome, jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Report PDF file created for job {JobId}: {FileName}", jobId, fileName);

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

        #region Report

        // ---------------------------------------------------------------- statistiche attivita'

        private static async Task<ReportPdf> ReportAttivita(Contesto ctx, bool perDipendente)
        {
            var req = new GenericRequest<ActivityStatistics_GetInModel>();
            req.Data.ActivityStatistics = new ActivityStatisticsModel { Year = ctx.Richiesta.Year, Month = ctx.Richiesta.Month, SelectedUserId = ctx.UsersId };
            var res = await ctx.Get<IActivityStatisticsService>().ActivityStatisticsGet(req, true);
            var dati = Dati(res);

            var report = ctx.NuovoReport(T(perDipendente ? "Report_AttivitaPerDipendente_Titolo" : "Report_AttivitaTotali_Titolo"), orizzontale: false);
            report.Intestazione.Add(Voce(T("Report_Dipendenti"), ctx.Dipendenti.Count.ToString(CultureInfo.CurrentCulture)));

            var tabella = new ReportPdf_Tabella
            {
                Colonne =
                {
                    new ReportPdf_Colonna(T("Report_ColAttivita"), 5.0),
                    new ReportPdf_Colonna(T("Report_ColCommessa"), 5.2),
                    new ReportPdf_Colonna(T("Report_ColCliente"), 5.4),
                    new ReportPdf_Colonna(T("Report_ColOre"), 3.0, ReportPdf_Allineamento.Destra)
                }
            };

            if (perDipendente)
            {
                foreach (var gruppo in dati.RighePerDipendente.GroupBy(r => r.UserId)
                                                              .Select(g => new { Nome = ctx.Nome(g.Key, g.First().NomeDipendente), Righe = g.ToList() })
                                                              .OrderBy(g => g.Nome, StringComparer.CurrentCultureIgnoreCase))
                {
                    tabella.Righe.Add(new ReportPdf_Riga(ReportPdf_TipoRiga.Gruppo,
                                                         new ReportPdf_Cella(gruppo.Nome, 2),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(gruppo.Righe.Sum(r => r.TotaleMinuti)))));
                    foreach (var r in gruppo.Righe.OrderBy(r => r.NomeCliente).ThenBy(r => r.NomeCommessa).ThenBy(r => r.NomeAttivita))
                        tabella.Righe.Add(new ReportPdf_Riga(r.NomeAttivita, Commessa(r.NomeCommessa, r.NomeSubCommessa), r.NomeCliente, TimeSheetPdfWriter.HHMM(r.TotaleMinuti)));
                }
                if (tabella.Righe.Count > 0)
                    tabella.Righe.Add(RigaTotale(T("Report_TotaleComplessivo"), 3, TimeSheetPdfWriter.HHMM(dati.RighePerDipendente.Sum(r => r.TotaleMinuti))));
            }
            else
            {
                foreach (var r in dati.TotaliPerAttivita.OrderBy(r => r.NomeCliente).ThenBy(r => r.NomeCommessa).ThenBy(r => r.NomeAttivita))
                    tabella.Righe.Add(new ReportPdf_Riga(r.NomeAttivita, Commessa(r.NomeCommessa, r.NomeSubCommessa), r.NomeCliente, TimeSheetPdfWriter.HHMM(r.TotaleMinuti)));
                if (tabella.Righe.Count > 0)
                    tabella.Righe.Add(RigaTotale(T("Report_TotaleComplessivo"), 3, TimeSheetPdfWriter.HHMM(dati.TotaliPerAttivita.Sum(r => r.TotaleMinuti))));
            }
            report.Tabelle.Add(tabella);

            // giornate non conteggiate (non elaborate o con errori), come la scheda "Giornate escluse" della pagina
            if (dati.GiornateEscluse.Count > 0)
                report.Note.Add(T("Report_GiornateEscluse", dati.GiornateEscluse.Count));

            return report;
        }

        private static string Commessa(string commessa, string subCommessa)
            => string.IsNullOrWhiteSpace(subCommessa) ? commessa : $"{commessa}\n{subCommessa}";

        // ---------------------------------------------------------------- piano ferie

        private static async Task<ReportPdf> ReportPianoFerie(Contesto ctx)
        {
            int anno = ctx.Richiesta.Year, mese = ctx.Richiesta.Month;

            var req = new GenericRequest<VacationPlan_GetInModel>();
            req.Data.VacationPlan = new VacationPlanModel { Year = anno, Month = mese, SelectedUserId = ctx.UsersId };
            var dati = Dati(await ctx.Get<IVacationPlanService>().VacationPlanGet(req, true));

            var resGiust = await ctx.Get<IPar_GiustificativiService>().GetAll(new GenericRequest<Par_GiustificativiInModel>(), true);
            var giustificativi = (resGiust.Success && resGiust.Data != null ? resGiust.Data.Par_Giustificativi : new List<Par_GiustificativiModel>())
                                     .ToDictionary(g => g.Id);

            var report = ctx.NuovoReport(T("Report_PianoFerie_Titolo"), orizzontale: true);
            report.Intestazione.Add(Voce(T("Report_Dipendenti"), ctx.Dipendenti.Count.ToString(CultureInfo.CurrentCulture)));

            int giorni = DateTime.DaysInMonth(anno, mese);
            const double colonnaNome = 4.3;
            double colonnaGiorno = Math.Round((ReportPdfWriter.LarghezzaUtileCm(true) - colonnaNome) / giorni, 3);
            var culture = CultureInfo.CurrentCulture;

            var tabella = new ReportPdf_Tabella { FontSize = 6.5 };
            tabella.Colonne.Add(new ReportPdf_Colonna(T("Report_ColDipendente"), colonnaNome));
            for (int g = 1; g <= giorni; g++)
            {
                var giorno = new DateTime(anno, mese, g);
                tabella.Colonne.Add(new ReportPdf_Colonna($"{g}\n{giorno.ToString("ddd", culture)[..Math.Min(2, giorno.ToString("ddd", culture).Length)]}", colonnaGiorno, ReportPdf_Allineamento.Centro));
            }

            var codiciUsati = new SortedDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            bool inAttesa = false;

            foreach (var slot in dati.DaySlots.OrderBy(s => ctx.Nome(s.IdAspNetUsers, null), StringComparer.CurrentCultureIgnoreCase))
            {
                var riga = new ReportPdf_Riga();
                riga.Celle.Add(new ReportPdf_Cella(ctx.Nome(slot.IdAspNetUsers, null)));

                for (int g = 1; g <= giorni; g++)
                {
                    var giorno = new DateTime(anno, mese, g);
                    var cella = new ReportPdf_Cella();
                    var delGiorno = slot.Dip_GG_Giustificativi
                                        .Where(x => x.Data.Date == giorno && x.RichiestaStato != StatoRichiesta.Rifiutata && x.RichiestaStato != StatoRichiesta.Cancellata)
                                        .ToList();
                    var testi = new List<string>();
                    foreach (var x in delGiorno)
                    {
                        giustificativi.TryGetValue(x.IdPar_Giustificativi, out var par);
                        var codice = par?.Codice ?? x.IdPar_Giustificativi.ToString(CultureInfo.InvariantCulture);
                        codiciUsati[codice] = par?.Descrizione ?? codice;

                        bool attesa = StatiInAttesa.Contains(x.RichiestaStato);
                        inAttesa |= attesa;
                        testi.Add(codice + (attesa ? TimeSheetPdfWriter.SegnoInAttesa : ""));
                        if (x.Hours is TimeSpan ore && ore > TimeSpan.Zero)
                            testi.Add(TimeSheetPdfWriter.HHMM((int)ore.TotalMinutes));

                        // colori del primo giustificativo del giorno, come nella pagina
                        if (cella.Sfondo == null && par != null)
                        {
                            cella.Sfondo = par.BackgroundColor;
                            cella.Colore = par.TextColor;
                        }
                    }
                    cella.Testo = string.Join("\n", testi);
                    if (cella.Sfondo == null && (giorno.DayOfWeek == DayOfWeek.Saturday || giorno.DayOfWeek == DayOfWeek.Sunday))
                        cella.Sfondo = ColoreFineSettimana;
                    riga.Celle.Add(cella);
                }
                tabella.Righe.Add(riga);
            }
            report.Tabelle.Add(tabella);

            if (codiciUsati.Count > 0)
                report.Note.Add(T("Report_Legenda") + ": " + string.Join("  ·  ", codiciUsati.Select(c => $"{c.Key} = {c.Value}")));
            if (inAttesa)
                report.Note.Add(AttendanceTrackingMessages.Get("CartellinoPdf_LegendaInAttesa", TimeSheetPdfWriter.SegnoInAttesa));

            return report;
        }

        private const string ColoreFineSettimana = "#F2F2F2";

        // ---------------------------------------------------------------- quadratura mensile

        private static async Task<ReportPdf> ReportQuadratura(Contesto ctx)
        {
            var report = ctx.NuovoReport(T("Report_Quadratura_Titolo"), orizzontale: true);
            var titoliReparti = ctx.Reparti.Where(r => r.Titolo != null).Select(r => r.Titolo!).ToList();
            if (titoliReparti.Count > 0)
                report.Intestazione.Add(Voce(T("Report_Reparti"), string.Join(", ", titoliReparti)));

            var colonne = new List<ReportPdf_Colonna>
            {
                new ReportPdf_Colonna(T("Report_ColDipendente"), 6.9),
                new ReportPdf_Colonna(T("Report_ColContratto"), 3.6),
                new ReportPdf_Colonna(T("Report_ColStato"), 2.6),
                new ReportPdf_Colonna(T("Report_ColLavorate"), 2.6, ReportPdf_Allineamento.Destra),
                new ReportPdf_Colonna(T("Report_ColInPiu"), 2.3, ReportPdf_Allineamento.Destra),
                new ReportPdf_Colonna(T("Report_ColSuppl"), 2.3, ReportPdf_Allineamento.Destra),
                new ReportPdf_Colonna(T("Report_ColStrao"), 2.3, ReportPdf_Allineamento.Destra),
                new ReportPdf_Colonna(T("Report_ColEccedenti"), 2.3, ReportPdf_Allineamento.Destra),
                new ReportPdf_Colonna(T("Report_ColNonRic"), 2.4, ReportPdf_Allineamento.Destra)
            };

            bool provvisorie = false;
            var dip_PeriodoService = ctx.Get<IDip_PeriodoService>();

            foreach (var reparto in ctx.Reparti)
            {
                var req = new GenericRequest<Dip_Periodo_GetAll_InModel>();
                req.Data.Anno = ctx.Richiesta.Year;
                req.Data.Mese = ctx.Richiesta.Month;
                req.Data.UsersId = reparto.UsersId;
                var dati = Dati(await dip_PeriodoService.Dip_Periodo_GetAll(req, true));

                var tabella = new ReportPdf_Tabella { Titolo = reparto.Titolo, Colonne = colonne };
                var periodi = new List<Dip_PeriodoModel>();

                foreach (var r in dati.Righe.OrderBy(r => ctx.Nome(r.IdAspNetUsers, NomeDipendente(r.Cognome, r.Nome, r.UserName)), StringComparer.CurrentCultureIgnoreCase))
                {
                    var riga = new ReportPdf_Riga();
                    riga.Celle.Add(new ReportPdf_Cella(ctx.Nome(r.IdAspNetUsers, NomeDipendente(r.Cognome, r.Nome, r.UserName))));
                    riga.Celle.Add(new ReportPdf_Cella(Contratto(r)) { Tenue = true });
                    riga.Celle.Add(new ReportPdf_Cella(StatoPeriodo(r.Dip_Periodo)));

                    if (r.Dip_Periodo is Dip_PeriodoModel p)
                    {
                        periodi.Add(p);
                        provvisorie |= p.Min_Provvisorie > 0;
                        riga.Celle.Add(new ReportPdf_Cella($"{TimeSheetPdfWriter.HHMM(p.Min_Lav)}\n/ {TimeSheetPdfWriter.HHMM(p.Min_Teo)}"));
                        riga.Celle.Add(new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(p.Min_Ecc) + (p.Min_Provvisorie > 0 ? TimeSheetPdfWriter.SegnoProvvisorie : "")));
                        riga.Celle.Add(new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(Supplementare(p))));
                        riga.Celle.Add(new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(Straordinario(p))));
                        riga.Celle.Add(new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(p.Min_EccOrd)));
                        riga.Celle.Add(new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(p.Min_NonRiconosciute)));
                    }
                    else
                        riga.Celle.Add(new ReportPdf_Cella(T("Report_DaRicalcolare"), 5) { Tenue = true });

                    tabella.Righe.Add(riga);
                }

                if (periodi.Count > 0)
                    tabella.Righe.Add(new ReportPdf_Riga(ReportPdf_TipoRiga.Totale,
                                                         new ReportPdf_Cella(T("Report_Totale"), 2),
                                                         new ReportPdf_Cella($"{TimeSheetPdfWriter.HHMM(periodi.Sum(p => p.Min_Lav))}\n/ {TimeSheetPdfWriter.HHMM(periodi.Sum(p => p.Min_Teo))}"),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(periodi.Sum(p => p.Min_Ecc))),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(periodi.Sum(Supplementare))),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(periodi.Sum(Straordinario))),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(periodi.Sum(p => p.Min_EccOrd))),
                                                         new ReportPdf_Cella(TimeSheetPdfWriter.HHMM(periodi.Sum(p => p.Min_NonRiconosciute)))));

                report.Tabelle.Add(tabella);
            }

            if (provvisorie)
                report.Note.Add(AttendanceTrackingMessages.Get("CartellinoPdf_LegendaProvvisorie", TimeSheetPdfWriter.SegnoProvvisorie));

            return report;
        }

        private static int Supplementare(Dip_PeriodoModel p) => p.Min_Suppl + p.Min_SupplEcc;
        private static int Straordinario(Dip_PeriodoModel p) => p.Min_Strao + p.Min_StraoNott + p.Min_StraoFest + p.Min_StraoFestNott;

        private static string Contratto(Dip_Periodo_Riga r)
            => T(r.TipoContratto == TipoContratto.PartTime ? "Report_PartTime" : "Report_FullTime")
               + (r.DestinazioneEccedenze == DestinazioneEccedenze.BancaOre ? "\n" + T("Report_BancaOre") : "");

        private static string StatoPeriodo(Dip_PeriodoModel? p) => p == null
            ? T("Report_PeriodoNonCalcolato")
            : p.Stato switch
            {
                PeriodoStato.Approvato => T("Report_PeriodoApprovato"),
                PeriodoStato.DaApprovare => T("Report_PeriodoDaApprovare"),
                _ => T("Report_PeriodoProvvisorio")
            };

        // ---------------------------------------------------------------- note spese

        private static async Task<ReportPdf> ReportNotaSpese(Contesto ctx, bool perDipendente)
        {
            int anno = ctx.Richiesta.Year, mese = ctx.Richiesta.Month;

            var req = new GenericRequest<Dip_GG_NotaSpesa_Search_InModel>();
            req.Data.Dal = new DateTime(anno, mese, 1);
            req.Data.Al = new DateTime(anno, mese, DateTime.DaysInMonth(anno, mese));
            req.Data.UsersId = ctx.UsersId;
            req.Data.IdPar_TipoSpesa = ctx.Richiesta.IdPar_TipoSpesa;
            req.Data.Stati = ctx.Richiesta.Stati;
            var spese = Dati(await ctx.Get<IDip_GG_NotaSpesaService>().Search(req, true)).Dip_GG_NotaSpesa;

            var resTipi = await ctx.Get<IPar_TipoSpesaService>().GetAll(new GenericRequest<Par_TipoSpesaInModel>(), true);
            var tipi = (resTipi.Success && resTipi.Data != null ? resTipi.Data.Par_TipoSpesa : new List<Par_TipoSpesaModel>())
                           .ToDictionary(t => t.Id, t => t.Descrizione ?? t.Codice ?? string.Empty);
            string Tipo(int id) => tipi.TryGetValue(id, out var d) ? d : string.Empty;

            var report = ctx.NuovoReport(T(perDipendente ? "Report_NotaSpesePerDipendente_Titolo" : "Report_NotaSpesePerTipo_Titolo"), orizzontale: false);
            report.Intestazione.Add(Voce(T("Report_TipoSpesa"), ctx.Richiesta.IdPar_TipoSpesa is int idTipo && idTipo > 0 ? Tipo(idTipo) : T("Report_Tutti")));
            report.Intestazione.Add(Voce(T("Report_Stato"), ctx.Richiesta.Stati == null || ctx.Richiesta.Stati.Count == 0
                                                                ? T("Report_Tutti")
                                                                : string.Join(", ", ctx.Richiesta.Stati.Select(StatoRichiestaTesto))));

            var tabella = new ReportPdf_Tabella
            {
                Colonne =
                {
                    new ReportPdf_Colonna(T("Report_ColData"), 2.2),
                    new ReportPdf_Colonna(T(perDipendente ? "Report_ColTipoSpesa" : "Report_ColDipendente"), 4.4),
                    new ReportPdf_Colonna(T("Report_ColDescrizione"), 6.5),
                    new ReportPdf_Colonna(T("Report_ColStato"), 2.8),
                    new ReportPdf_Colonna(T("Report_ColImporto"), 2.7, ReportPdf_Allineamento.Destra)
                }
            };

            var gruppi = spese.GroupBy(s => perDipendente ? s.IdAspNetUsers : s.IdPar_TipoSpesa.ToString(CultureInfo.InvariantCulture))
                              .Select(g => new
                              {
                                  Titolo = perDipendente ? ctx.Nome(g.Key, null) : Tipo(g.First().IdPar_TipoSpesa),
                                  Spese = g.OrderBy(s => s.Data).ThenBy(s => s.Id).ToList()
                              })
                              .OrderBy(g => g.Titolo, StringComparer.CurrentCultureIgnoreCase);

            foreach (var g in gruppi)
            {
                tabella.Righe.Add(new ReportPdf_Riga(ReportPdf_TipoRiga.Gruppo,
                                                     new ReportPdf_Cella($"{g.Titolo}  ({T(g.Spese.Count == 1 ? "Report_Spesa" : "Report_Spese", g.Spese.Count)})", 3),
                                                     new ReportPdf_Cella(Euro(g.Spese.Sum(s => s.Importo)), 1)));
                foreach (var s in g.Spese)
                    tabella.Righe.Add(new ReportPdf_Riga(s.Data.ToString("d", CultureInfo.CurrentCulture),
                                                         perDipendente ? Tipo(s.IdPar_TipoSpesa) : ctx.Nome(s.IdAspNetUsers, null),
                                                         DescrizioneSpesa(s),
                                                         StatoRichiestaTesto(s.RichiestaStato),
                                                         Euro(s.Importo)));
            }
            if (tabella.Righe.Count > 0)
                tabella.Righe.Add(RigaTotale($"{T("Report_TotaleComplessivo")}  ({T(spese.Count == 1 ? "Report_Spesa" : "Report_Spese", spese.Count)})", 4, Euro(spese.Sum(s => s.Importo))));

            report.Tabelle.Add(tabella);
            return report;
        }

        private static string DescrizioneSpesa(Dip_GG_NotaSpesaModel s)
        {
            var righe = new List<string>();
            if (!string.IsNullOrWhiteSpace(s.Descrizione))
                righe.Add(s.Descrizione.Trim());
            if (!string.IsNullOrWhiteSpace(s.RichiestaDescrizione))
                righe.Add(s.RichiestaDescrizione.Trim());
            if (s.Km is decimal km && km > 0)
            {
                var auto = string.Join(" ", new[] { s.AutovetturaDescrizione, string.IsNullOrWhiteSpace(s.AutovetturaTarga) ? null : $"({s.AutovetturaTarga})" }
                                                .Where(x => !string.IsNullOrWhiteSpace(x)));
                righe.Add(T("Report_RimborsoKm", km.ToString("0.##", CultureInfo.CurrentCulture), auto).TrimEnd(' ', '·'));
            }
            return string.Join("\n", righe);
        }

        private static string Euro(decimal valore) => "€ " + valore.ToString("N2", CultureInfo.CurrentCulture);

        private static string StatoRichiestaTesto(StatoRichiesta stato) => T("Report_StatoRichiesta_" + stato);

        #endregion

        #region Utilita'

        private static ReportPdf_Riga RigaTotale(string etichetta, int colonneEtichetta, string valore)
            => new ReportPdf_Riga(ReportPdf_TipoRiga.Totale, new ReportPdf_Cella(etichetta, colonneEtichetta - 1), new ReportPdf_Cella(valore));

        private static KeyValuePair<string, string> Voce(string etichetta, string valore) => new KeyValuePair<string, string>(etichetta, valore);

        // dati di un servizio di lettura: un errore ferma il report (il messaggio arriva nella notifica del job)
        private static TData Dati<TData>(GenericResult<TData> res) where TData : class
        {
            if (!res.Success || res.Data == null)
                throw new InvalidOperationException(res.Messages?.FirstOrDefault()?.Text ?? T("Report_DatiNonDisponibili"));
            return res.Data;
        }

        private static string NomeDipendente(string? cognome, string? nome, string? userName)
        {
            var testo = $"{cognome} {nome}".Trim();
            return testo.Length > 0 ? testo : userName ?? string.Empty;
        }

        // gruppi della quadratura: un gruppo per reparto selezionato (i suoi dipendenti tra quelli richiesti),
        // poi i dipendenti richiesti che non stanno in nessuno dei reparti. Senza reparti un solo gruppo senza titolo
        private List<GruppoReparto> GruppiReparto(List<int>? repartiId, List<Dipendente> dipendenti)
        {
            var ids = (repartiId ?? new List<int>()).Distinct().ToList();
            var richiesti = dipendenti.Select(d => d.UserId).ToList();
            if (ids.Count == 0)
                return new List<GruppoReparto> { new GruppoReparto(null, richiesti) };

            var gruppi = new List<GruppoReparto>();
            var inReparto = new HashSet<string>();
            foreach (var reparto in _az_SediRepartoRepository.FindAll(x => ids.Contains(x.Id)).ToList().OrderBy(x => x.Descrizione))
            {
                var membri = _az_SediRepartoUserRepository.FindAll(x => x.IdAz_SediReparto == reparto.Id && x.UserInDepartment)
                                                          .Select(x => x.IdAspNetUsers).ToHashSet();
                var users = richiesti.Where(u => membri.Contains(u)).ToList();
                if (users.Count == 0)
                    continue;
                inReparto.UnionWith(users);
                gruppi.Add(new GruppoReparto(reparto.Descrizione ?? string.Empty, users));
            }

            var altri = richiesti.Where(u => !inReparto.Contains(u)).ToList();
            if (altri.Count > 0)
                gruppi.Add(new GruppoReparto(gruppi.Count > 0 ? T("Report_AltriDipendenti") : null, altri));
            return gruppi;
        }

        // dipendenti dei reparti in cui l'utente e' amministratore (come TimeSheet_ExportService)
        private HashSet<string> DipendentiAmministrati(string idAspNetUsers)
        {
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.EnabledToAdmin)
                                                       .Select(x => x.IdAz_SediReparto).ToList();
            return _az_SediRepartoUserRepository.FindAll(x => reparti.Contains(x.IdAz_SediReparto) && x.UserInDepartment)
                                                .Select(x => x.IdAspNetUsers).ToHashSet();
        }

        private async Task<bool> HaRuolo(string ruolo)
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            return user != null && await _userManager.IsInRoleAsync(user, ruolo);
        }

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);

        private record Dipendente(string UserId, string Nome);

        private record GruppoReparto(string? Titolo, List<string> UsersId);

        // dati comuni ai report, nello scope del job
        private class Contesto
        {
            private readonly IServiceProvider _services;
            private readonly Dictionary<string, string> _nomi;

            public ReportModel Richiesta { get; }
            public List<Dipendente> Dipendenti { get; }
            public List<GruppoReparto> Reparti { get; }
            public string Azienda { get; }
            public List<string> UsersId => Dipendenti.Select(d => d.UserId).ToList();

            public Contesto(IServiceProvider services, ReportModel richiesta, List<Dipendente> dipendenti, List<GruppoReparto> reparti, string azienda)
            {
                _services = services;
                Richiesta = richiesta;
                Dipendenti = dipendenti;
                Reparti = reparti;
                Azienda = azienda;
                _nomi = dipendenti.ToDictionary(d => d.UserId, d => d.Nome);
            }

            public TService Get<TService>() where TService : notnull => _services.GetRequiredService<TService>();

            // nome del dipendente (cognome e nome), altrimenti quello fornito dal servizio
            public string Nome(string userId, string? alternativo)
                => _nomi.TryGetValue(userId, out var nome) && nome.Length > 0 ? nome : alternativo ?? userId;

            public ReportPdf NuovoReport(string titolo, bool orizzontale)
            {
                var report = new ReportPdf { Titolo = titolo, Orizzontale = orizzontale };
                report.Intestazione.Add(Voce(AttendanceTrackingMessages.Get("CartellinoPdf_Azienda"), Azienda));
                report.Intestazione.Add(Voce(AttendanceTrackingMessages.Get("CartellinoPdf_Mese"), NomeMese(Richiesta.Year, Richiesta.Month)));
                return report;
            }

            private static string NomeMese(int anno, int mese)
            {
                var nome = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mese);
                return (nome.Length > 0 ? char.ToUpper(nome[0]) + nome[1..] : nome) + " " + anno;
            }
        }

        #endregion
    }

    public interface IReportService : IServiceBase
    {
        Task<GenericResult<Report_OutModel>> ReportExportPdf(GenericRequest<Report_InModel> model, bool isSubProcess);
    }
}
