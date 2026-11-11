using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance._utility;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Compliance
{
    /*
     Registro GDPR (CMP-GDPR, ANALISI_COMPLIANCE_APPLICATIVO.md §4.4):
        - registro dei trattamenti (CMP-GDPR.1), responsabili esterni e nomine con la scadenza del contratto (CMP-GDPR.2)
        - registro delle violazioni con il termine delle 72 ore per la notifica al Garante nello scadenzario (CMP-GDPR.3):
          aperto finche' la notifica non e' registrata, tolto se il rischio e' improbabile (nessuna notifica dovuta);
          a una nuova violazione gli amministratori dell'azienda ricevono una notifica
        - gestione da parte dello studio per conto delle aziende (CMP-GDPR.4): stesse funzioni con l'azienda scelta
          (IdCompany), elenco delle aziende dello studio con Compliance attivo e scadenze del modulo dell'azienda
        - export del registro in Excel con un job (file e notifica con il link come gli altri export)
     Permessi: CompanyPowerAdmin, CompanyAdmin e studio (FinancialAdvisorPowerAdmin/Admin), controllati in CmpUtility.
    */
    public class Cmp_GdprService : ServiceBase, ICmp_GdprService
    {
        public const string JobTypeExport = "Compliance.GdprExcel";

        private readonly ICmpUtility _utility;
        private readonly ICmpScadenze _scadenze;
        private readonly INotificationSender _notificationSender;
        private readonly ICompanyRecipients _companyRecipients;
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICmp_TrattamentoRepository _Cmp_Trattamento;
        private readonly ICmp_ResponsabileRepository _Cmp_Responsabile;
        private readonly ICmp_ViolazioneRepository _Cmp_Violazione;

        public Cmp_GdprService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,
                               ICmpUtility utility,
                               ICmpScadenze scadenze,
                               INotificationSender notificationSender,
                               ICompanyRecipients companyRecipients,
                               IScadenzaRepository scadenzaRepository,
                               ICmp_TrattamentoRepository cmp_Trattamento,
                               ICmp_ResponsabileRepository cmp_Responsabile,
                               ICmp_ViolazioneRepository cmp_Violazione) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _scadenze = scadenze;
            _notificationSender = notificationSender;
            _companyRecipients = companyRecipients;
            _scadenzaRepository = scadenzaRepository;
            _Cmp_Trattamento = cmp_Trattamento;
            _Cmp_Responsabile = cmp_Responsabile;
            _Cmp_Violazione = cmp_Violazione;
        }

        private async Task<CmpContesto> GestoreAsync(int? idCompany)
        {
            var ctx = await _utility.ContestoAsync(idCompany);
            if (!ctx.GestoreGdpr)
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            return ctx;
        }

        // =========================================================================================== trattamenti (CMP-GDPR.1)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data?.IdCompany);
                using var scope = ctx.Scope();
                var items = await _Cmp_Trattamento.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Denominazione).ToListAsync();
                return new Cmp_ListOutModel<Cmp_TrattamentoModel> { Items = items.Select(TrattamentoModel).ToList() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoPut(GenericRequest<Cmp_PutInModel<Cmp_TrattamentoModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var d = model.Data.Item;
                var riga = d.Id == 0 ? new Cmp_Trattamento { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Trattamento.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                riga.Denominazione = Cmp_SicurezzaService.Obbligatorio(d.Denominazione, 200);
                riga.Finalita = Cmp_SicurezzaService.Testo(d.Finalita, 2000);
                riga.BaseGiuridica = Math.Clamp(d.BaseGiuridica, 0, 5);
                riga.CategorieInteressati = Cmp_SicurezzaService.Testo(d.CategorieInteressati, 1000);
                riga.CategorieDati = Cmp_SicurezzaService.Testo(d.CategorieDati, 1000);
                riga.DatiParticolari = d.DatiParticolari;
                riga.Destinatari = Cmp_SicurezzaService.Testo(d.Destinatari, 1000);
                riga.Trasferimenti = Cmp_SicurezzaService.Testo(d.Trasferimenti, 1000);
                riga.TerminiCancellazione = Cmp_SicurezzaService.Testo(d.TerminiCancellazione, 500);
                riga.MisureSicurezza = Cmp_SicurezzaService.Testo(d.MisureSicurezza, 2000);
                riga.Note = Cmp_SicurezzaService.Testo(d.Note, 1000);
                riga.Attivo = d.Attivo;
                riga = d.Id == 0 ? await _Cmp_Trattamento.CreateAsync(riga) : await _Cmp_Trattamento.UpdateAsync(riga);
                return new Cmp_PutOutModel<Cmp_TrattamentoModel> { Item = TrattamentoModel(riga) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_TrattamentoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var riga = await _Cmp_Trattamento.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                await _Cmp_Trattamento.DeleteAsync(riga);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        private static Cmp_TrattamentoModel TrattamentoModel(Cmp_Trattamento x) => new()
        {
            Id = x.Id, Denominazione = x.Denominazione, Finalita = x.Finalita, BaseGiuridica = x.BaseGiuridica, CategorieInteressati = x.CategorieInteressati,
            CategorieDati = x.CategorieDati, DatiParticolari = x.DatiParticolari, Destinatari = x.Destinatari, Trasferimenti = x.Trasferimenti,
            TerminiCancellazione = x.TerminiCancellazione, MisureSicurezza = x.MisureSicurezza, Note = x.Note, Attivo = x.Attivo
        };

        // =========================================================================================== responsabili (CMP-GDPR.2)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabileList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data?.IdCompany);
                using var scope = ctx.Scope();
                var items = await _Cmp_Responsabile.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Denominazione).ToListAsync();
                return new Cmp_ListOutModel<Cmp_ResponsabileModel> { Items = items.Select(x => ResponsabileModel(ctx, x)).ToList() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabilePut(GenericRequest<Cmp_PutInModel<Cmp_ResponsabileModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var d = model.Data.Item;
                if (d.DataContratto != null && d.DataScadenza != null && d.DataScadenza < d.DataContratto)
                    throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
                var riga = d.Id == 0 ? new Cmp_Responsabile { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Responsabile.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                riga.Tipo = Math.Clamp(d.Tipo, 0, 4);
                riga.Denominazione = Cmp_SicurezzaService.Obbligatorio(d.Denominazione, 200);
                riga.CodiceFiscale = Cmp_SicurezzaService.Testo(d.CodiceFiscale, 20);
                riga.Email = Cmp_SicurezzaService.Testo(d.Email, 100);
                riga.Telefono = Cmp_SicurezzaService.Testo(d.Telefono, 30);
                riga.Trattamenti = Cmp_SicurezzaService.Testo(d.Trattamenti, 1000);
                riga.DataContratto = d.DataContratto?.Date;
                riga.DataScadenza = d.DataScadenza?.Date;
                riga.Documento = Cmp_SicurezzaService.Testo(d.Documento, 200);
                riga.Note = Cmp_SicurezzaService.Testo(d.Note, 1000);
                riga = d.Id == 0 ? await _Cmp_Responsabile.CreateAsync(riga) : await _Cmp_Responsabile.UpdateAsync(riga);
                await _scadenze.RegistraAsync(ctx, CmpFonti.Responsabile, riga.Id, ComplianceMessages.Get("DeadlineProcessor", riga.Denominazione), riga.DataScadenza, false,
                                              CmpUrl.Gdpr, "responsabili");
                return new Cmp_PutOutModel<Cmp_ResponsabileModel> { Item = ResponsabileModel(ctx, riga) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ResponsabileDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var riga = await _Cmp_Responsabile.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                await _Cmp_Responsabile.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Responsabile, riga.Id);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        private static Cmp_ResponsabileModel ResponsabileModel(CmpContesto ctx, Cmp_Responsabile x)
        {
            var oggi = DateTime.Today;
            return new Cmp_ResponsabileModel
            {
                Id = x.Id, Tipo = x.Tipo, Denominazione = x.Denominazione, CodiceFiscale = x.CodiceFiscale, Email = x.Email, Telefono = x.Telefono,
                Trattamenti = x.Trattamenti, DataContratto = x.DataContratto, DataScadenza = x.DataScadenza, Documento = x.Documento, Note = x.Note,
                Scaduta = x.DataScadenza != null && x.DataScadenza < oggi,
                InScadenza = x.DataScadenza != null && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(ctx.GiorniPreavviso),
            };
        }

        // =========================================================================================== violazioni (CMP-GDPR.3)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data?.IdCompany);
                using var scope = ctx.Scope();
                var items = await _Cmp_Violazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderByDescending(x => x.DataScoperta).ToListAsync();
                return new Cmp_ListOutModel<Cmp_ViolazioneModel> { Items = items.Select(ViolazioneModel).ToList() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazionePut(GenericRequest<Cmp_PutInModel<Cmp_ViolazioneModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var d = model.Data.Item;
                if (d.DataScoperta == default)
                    throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
                if ((d.DataViolazione != null && d.DataViolazione > d.DataScoperta)
                    || (d.DataNotificaGarante != null && d.DataNotificaGarante < d.DataScoperta.Date))
                    throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
                bool nuova = d.Id == 0;
                var riga = nuova ? new Cmp_Violazione { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Violazione.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                riga.DataScoperta = d.DataScoperta;
                riga.DataViolazione = d.DataViolazione;
                riga.Descrizione = Cmp_SicurezzaService.Obbligatorio(d.Descrizione, 2000);
                riga.CategorieDati = Cmp_SicurezzaService.Testo(d.CategorieDati, 1000);
                riga.NumeroInteressati = d.NumeroInteressati is >= 0 ? d.NumeroInteressati : null;
                riga.Conseguenze = Cmp_SicurezzaService.Testo(d.Conseguenze, 2000);
                riga.Misure = Cmp_SicurezzaService.Testo(d.Misure, 2000);
                riga.Rischio = Math.Clamp(d.Rischio, 0, 2);
                riga.DataNotificaGarante = d.DataNotificaGarante;
                riga.MotivoRitardo = Cmp_SicurezzaService.Testo(d.MotivoRitardo, 1000);
                riga.DataComunicazioneInteressati = d.DataComunicazioneInteressati;
                riga.Note = Cmp_SicurezzaService.Testo(d.Note, 1000);
                riga = nuova ? await _Cmp_Violazione.CreateAsync(riga) : await _Cmp_Violazione.UpdateAsync(riga);

                // 72 ore dalla scoperta: aperta finche' non c'e' la notifica, nessuna scadenza se il rischio e' improbabile
                var termine = Termine(riga);
                await _scadenze.RegistraAsync(ctx, CmpFonti.Violazione, riga.Id, ComplianceMessages.Get("DeadlineBreach", riga.DataScoperta.ToString("dd/MM/yyyy HH:mm")),
                                              riga.Rischio == 0 ? null : termine, riga.DataNotificaGarante != null, CmpUrl.Gdpr, "violazioni",
                                              ComplianceMessages.Get("DeadlineBreachNote", termine.ToString("dd/MM/yyyy HH:mm")), giorniPreavviso: 3);

                if (nuova && ctx.NotificheAttive)
                {
                    var amministratori = await _companyRecipients.AdminsAsync(ctx.IdCompany);
                    await _notificationSender.SendAsync(new NotificationMessage
                    {
                        UserIds = amministratori.Where(x => x != ctx.IdUtente),
                        IdCompany = ctx.IdCompany,
                        Categoria = NotificationCategory.Scadenze.Code,
                        Titolo = ComplianceMessages.Get("BreachNotifyTitle"),
                        Messaggio = ComplianceMessages.Get("BreachNotifyText", riga.DataScoperta.ToString("dd/MM/yyyy HH:mm"), termine.ToString("dd/MM/yyyy HH:mm")),
                        Url = CmpUrl.Gdpr,
                        UrlState = new { sezione = "violazioni", id = riga.Id },
                    });
                }
                return new Cmp_PutOutModel<Cmp_ViolazioneModel> { Item = ViolazioneModel(riga) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ViolazioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data.IdCompany);
                using var scope = ctx.Scope();
                var riga = await _Cmp_Violazione.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw Cmp_SicurezzaService.NonTrovato();
                await _Cmp_Violazione.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Violazione, riga.Id);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        private static DateTime Termine(Cmp_Violazione x) => x.DataScoperta.AddHours(72);

        private static Cmp_ViolazioneModel ViolazioneModel(Cmp_Violazione x)
        {
            var termine = Termine(x);
            return new Cmp_ViolazioneModel
            {
                Id = x.Id, DataScoperta = x.DataScoperta, DataViolazione = x.DataViolazione, Descrizione = x.Descrizione, CategorieDati = x.CategorieDati,
                NumeroInteressati = x.NumeroInteressati, Conseguenze = x.Conseguenze, Misure = x.Misure, Rischio = x.Rischio, DataNotificaGarante = x.DataNotificaGarante,
                MotivoRitardo = x.MotivoRitardo, DataComunicazioneInteressati = x.DataComunicazioneInteressati, Note = x.Note,
                TermineNotifica = termine,
                NotificaDovuta = x.Rischio > 0 && x.DataNotificaGarante == null,
                NotificaInRitardo = x.Rischio > 0 && (x.DataNotificaGarante ?? DateTime.Now) > termine,
            };
        }

        // =========================================================================================== export (job)

        /*
         Registro GDPR in Excel (trattamenti, responsabili, violazioni) con un job in background, come gli altri export: la richiesta
         controlla i permessi, raccoglie i dati e prepara i testi nella lingua dell'utente; il job scrive il file e lo notifica.
        */
        public virtual async Task<GenericResult<Cmp_GdprExportOutModel>> Cmp_GdprExport(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data?.IdCompany);
                using var scope = ctx.Scope();
                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                    throw new UserMessageException(ComplianceMessages.Get("JobUserNotIdentified"));

                var trattamenti = await _Cmp_Trattamento.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Denominazione).ToListAsync();
                var responsabili = await _Cmp_Responsabile.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Denominazione).ToListAsync();
                var violazioni = await _Cmp_Violazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderByDescending(x => x.DataScoperta).ToListAsync();
                var testi = CmpGdprExcel.Testi.Correnti();
                Func<byte[]> scrivi = () => CmpGdprExcel.Crea(trattamenti, responsabili, violazioni, testi);

                var jobId = Guid.NewGuid().ToString();
                var nomeVisibile = ComplianceMessages.Get("GdprExportFileName", DateTime.Today.ToString("yyyy-MM-dd"));
                var payload = new { ctx.IdCompany };
                var messaggi = new
                {
                    Avviato = ComplianceMessages.Get("GdprExportStarted"),
                    Pronto = ComplianceMessages.Get("GdprExportReady"),
                    Errore = ComplianceMessages.Get("GdprExportFailed"),
                };

                RunInBackground(async scopeJob =>
                {
                    var notifier = scopeJob.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    Task Notifica(int progress, string testo, MessageType tipo, JobFile? file = null, bool fine = false) =>
                        notifier.LongJobProgressAsync(userId, new LongJobProgressUpdate
                        {
                            JobId = jobId, JobType = JobTypeExport, Payload = payload, Category = LongJobCategory.FileGeneration,
                            DownloadUrl = file?.DownloadUrl, FileName = file?.FileName, ProgressPercentage = progress,
                            Message = new Message { Text = testo, MsgType = tipo }, IsFinished = fine
                        });
                    try
                    {
                        await Notifica(10, messaggi.Avviato, MessageType.Information);
                        var file = NVXSystem.SalvaFileJob(jobId, userId, nomeVisibile, "xlsx", scrivi());
                        await Notifica(100, messaggi.Pronto, MessageType.Information, file, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Export del registro GDPR: job {JobId} non riuscito", jobId);
                        await Notifica(100, messaggi.Errore, MessageType.Exception, null, true);
                    }
                });

                await Task.Delay(DelayAsyncMethod);
                return new Cmp_GdprExportOutModel { JobId = jobId };
            }, isSubProcess);
        }

        // =========================================================================================== studio (CMP-GDPR.4)

        // aziende dello studio con Compliance attivo, con le scadenze del modulo aperte e gia' scadute
        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_StudioAziendaModel>>> Cmp_StudioAziende(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var aziende = await _utility.AziendeStudioAsync();
                var ids = aziende.Select(x => x.Id).ToList();
                var oggi = DateTime.Today;
                var conteggi = await _scadenzaRepository.FindAll(x => ids.Contains(x.IdCompany) && x.Application == (int)ApplicationType.Compliance && x.Stato == StatoScadenza.Aperta)
                                                        .GroupBy(x => x.IdCompany)
                                                        .Select(g => new { g.Key, Aperte = g.Count(), Scadute = g.Count(x => x.DataScadenza < oggi) })
                                                        .ToDictionaryAsync(x => x.Key);
                return new Cmp_ListOutModel<Cmp_StudioAziendaModel>
                {
                    Items = aziende.Select(a => new Cmp_StudioAziendaModel
                    {
                        IdCompany = a.Id, Descrizione = a.Descrizione ?? a.Id.ToString(),
                        ScadenzeAperte = conteggi.TryGetValue(a.Id, out var c) ? c.Aperte : 0,
                        ScadenzeScadute = conteggi.TryGetValue(a.Id, out var s) ? s.Scadute : 0,
                    }).ToList()
                };
            }, isSubProcess);
        }

        // scadenze di Compliance (sicurezza e GDPR) dell'azienda scelta, in sola lettura
        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_StudioScadenzaModel>>> Cmp_StudioScadenze(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await GestoreAsync(model.Data?.IdCompany);
                var oggi = DateTime.Today;
                var righe = await _scadenzaRepository.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.Compliance)
                                                     .OrderBy(x => x.Stato).ThenBy(x => x.DataScadenza).Take(1000).ToListAsync();
                return new Cmp_ListOutModel<Cmp_StudioScadenzaModel>
                {
                    Items = righe.Select(x => new Cmp_StudioScadenzaModel
                    {
                        Id = x.Id, Fonte = x.Fonte, Descrizione = x.Descrizione, DataScadenza = x.DataScadenza, Chiusa = x.Stato == StatoScadenza.Chiusa,
                        Scaduta = x.Stato == StatoScadenza.Aperta && x.DataScadenza < oggi,
                        InScadenza = x.Stato == StatoScadenza.Aperta && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(x.GiorniPreavviso),
                    }).ToList()
                };
            }, isSubProcess);
        }
    }

    public interface ICmp_GdprService : IServiceBase
    {
        Task<GenericResult<Cmp_ListOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_TrattamentoModel>>> Cmp_TrattamentoPut(GenericRequest<Cmp_PutInModel<Cmp_TrattamentoModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_TrattamentoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabileList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_ResponsabileModel>>> Cmp_ResponsabilePut(GenericRequest<Cmp_PutInModel<Cmp_ResponsabileModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ResponsabileDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_ViolazioneModel>>> Cmp_ViolazionePut(GenericRequest<Cmp_PutInModel<Cmp_ViolazioneModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_ViolazioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_GdprExportOutModel>> Cmp_GdprExport(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_StudioAziendaModel>>> Cmp_StudioAziende(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_StudioScadenzaModel>>> Cmp_StudioScadenze(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
    }

    // file Excel del registro GDPR (scritto nel job): un foglio per sezione
    public static class CmpGdprExcel
    {
        public sealed class Testi
        {
            public string[] Fogli { get; init; } = Array.Empty<string>();
            public string[] ColTrattamenti { get; init; } = Array.Empty<string>();
            public string[] ColResponsabili { get; init; } = Array.Empty<string>();
            public string[] ColViolazioni { get; init; } = Array.Empty<string>();
            public Dictionary<int, string> BasiGiuridiche { get; init; } = new();
            public Dictionary<int, string> TipiResponsabile { get; init; } = new();
            public Dictionary<int, string> Rischi { get; init; } = new();
            public string Si { get; init; } = string.Empty;
            public string No { get; init; } = string.Empty;

            private static string[] Colonne(string prefisso, int n) => Enumerable.Range(1, n).Select(i => ComplianceMessages.Get(prefisso + i)).ToArray();

            public static Testi Correnti() => new()
            {
                Fogli = new[] { ComplianceMessages.Get("GdprSheetTreatments"), ComplianceMessages.Get("GdprSheetProcessors"), ComplianceMessages.Get("GdprSheetBreaches") },
                ColTrattamenti = Colonne("GdprColT", 12),
                ColResponsabili = Colonne("GdprColR", 9),
                ColViolazioni = Colonne("GdprColV", 11),
                BasiGiuridiche = Enumerable.Range(0, 6).ToDictionary(i => i, CmpTesti.BaseGiuridica),
                TipiResponsabile = Enumerable.Range(0, 5).ToDictionary(i => i, CmpTesti.TipoResponsabile),
                Rischi = Enumerable.Range(0, 3).ToDictionary(i => i, CmpTesti.Rischio),
                Si = ComplianceMessages.Get("Yes"),
                No = ComplianceMessages.Get("No"),
            };
        }

        public static byte[] Crea(List<Cmp_Trattamento> trattamenti, List<Cmp_Responsabile> responsabili, List<Cmp_Violazione> violazioni, Testi t)
        {
            using var wb = new XLWorkbook();

            var ws = Foglio(wb, t.Fogli[0], t.ColTrattamenti);
            int r = 2;
            foreach (var x in trattamenti)
            {
                Riga(ws, r++, x.Denominazione, x.Finalita, t.BasiGiuridiche.GetValueOrDefault(x.BaseGiuridica), x.CategorieInteressati, x.CategorieDati,
                     x.DatiParticolari ? t.Si : t.No, x.Destinatari, x.Trasferimenti, x.TerminiCancellazione, x.MisureSicurezza, x.Attivo ? t.Si : t.No, x.Note);
            }

            ws = Foglio(wb, t.Fogli[1], t.ColResponsabili);
            r = 2;
            foreach (var x in responsabili)
                Riga(ws, r++, t.TipiResponsabile.GetValueOrDefault(x.Tipo), x.Denominazione, x.CodiceFiscale, x.Email, x.Telefono, x.Trattamenti,
                     x.DataContratto?.ToString("dd/MM/yyyy"), x.DataScadenza?.ToString("dd/MM/yyyy"), x.Documento);

            ws = Foglio(wb, t.Fogli[2], t.ColViolazioni);
            r = 2;
            foreach (var x in violazioni)
                Riga(ws, r++, x.DataScoperta.ToString("dd/MM/yyyy HH:mm"), x.DataViolazione?.ToString("dd/MM/yyyy HH:mm"), x.Descrizione, x.CategorieDati,
                     x.NumeroInteressati?.ToString(), x.Conseguenze, x.Misure, t.Rischi.GetValueOrDefault(x.Rischio),
                     x.DataNotificaGarante?.ToString("dd/MM/yyyy HH:mm"), x.MotivoRitardo, x.DataComunicazioneInteressati?.ToString("dd/MM/yyyy HH:mm"));

            foreach (var foglio in wb.Worksheets)
            {
                foglio.Columns().AdjustToContents();
                foreach (var col in foglio.ColumnsUsed().Where(c => c.Width > 60))
                    col.Width = 60;
            }
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static IXLWorksheet Foglio(XLWorkbook wb, string nome, string[] colonne)
        {
            var pulito = new string(nome.Where(c => !":\\/?*[]".Contains(c)).ToArray());
            var ws = wb.Worksheets.Add(pulito.Length > 31 ? pulito[..31] : pulito);
            for (int i = 0; i < colonne.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = colonne[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3EBF3");
            }
            ws.SheetView.FreezeRows(1);
            return ws;
        }

        private static void Riga(IXLWorksheet ws, int riga, params string?[] valori)
        {
            for (int i = 0; i < valori.Length; i++)
                ws.Cell(riga, i + 1).Value = valori[i] ?? string.Empty;
        }
    }
}
