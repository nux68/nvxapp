using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Globalization;
using System.Net;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Offerte (ANALISI_CRM_APPLICATIVO.md §6.1): righe con prodotto, quantita', prezzo, sconto e IVA; totali calcolati qui.
     Numerazione per anno (Crm_Cfg: prefisso, anno, ultimo numero) assegnata al primo salvataggio.
     Preventivi (CRM-PREV, ANALISI_CRM_PREVENTIVI.md):
        - stati Bozza / Inviata / Accettata / Rifiutata / Scaduta, con la scadenza nello scadenzario (CrmOfferteCiclo)
        - PDF con MigraDoc (CrmOffertaPdf) SOLO tramite job (Crm_OffertaPdfJob, barra dei job del client)
        - invio per email (IEmailService, PDF allegato): la bozza diventa "Inviata" e l'invio si registra come attivita' fatta
        - offerta accettata collegata a un'opportunita' aperta: l'opportunita' passa alla prima fase "vinta" (con lo storico)
    */
    public class Crm_OffertaService : ServiceBase, ICrm_OffertaService
    {
        public const string JobTypePdf = "Crm.OffertaPdf";

        private readonly ICrm_OffertaRepository _Crm_Offerta;
        private readonly ICrm_OffertaRigaRepository _Crm_OffertaRiga;
        private readonly ICrm_Az_ClienteReadRepository _Az_Cliente;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_AllegatoRepository _Crm_Allegato;
        private readonly ICrm_CfgRepository _Crm_Cfg;
        private readonly ICrm_Par_ProdottoRepository _Crm_Par_Prodotto;
        private readonly ICompanyRepository _Company;
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly ICrm_OpportunitaStoricoRepository _Crm_OpportunitaStorico;
        private readonly ICrm_Par_FaseRepository _Crm_Par_Fase;
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_Par_TipoAttivitaRepository _Crm_Par_TipoAttivita;
        private readonly ICrm_ClienteDatiRepository _Crm_ClienteDati;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly ICrmUtility _utility;
        private readonly IEmailService _email;

        public Crm_OffertaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  ICrm_OffertaRepository crm_OffertaRepository,
                                  ICrm_OffertaRigaRepository crm_OffertaRigaRepository,
                                  ICrm_Az_ClienteReadRepository az_ClienteRepository,
                                  ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                                  ICrm_AllegatoRepository crm_AllegatoRepository,
                                  ICrm_CfgRepository crm_CfgRepository,
                                  ICrm_Par_ProdottoRepository crm_Par_ProdottoRepository,
                                  ICompanyRepository companyRepository,
                                  ICrm_OpportunitaRepository crm_OpportunitaRepository,
                                  ICrm_OpportunitaStoricoRepository crm_OpportunitaStoricoRepository,
                                  ICrm_Par_FaseRepository crm_Par_FaseRepository,
                                  ICrm_AttivitaRepository crm_AttivitaRepository,
                                  ICrm_Par_TipoAttivitaRepository crm_Par_TipoAttivitaRepository,
                                  ICrm_ClienteDatiRepository crm_ClienteDatiRepository,
                                  IDeadlineRegistry deadlineRegistry,
                                  ICrmUtility utility,
                                  IEmailService email) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Offerta = crm_OffertaRepository;
            _Crm_OffertaRiga = crm_OffertaRigaRepository;
            _Az_Cliente = az_ClienteRepository;
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _Crm_Allegato = crm_AllegatoRepository;
            _Crm_Cfg = crm_CfgRepository;
            _Crm_Par_Prodotto = crm_Par_ProdottoRepository;
            _Company = companyRepository;
            _Crm_Opportunita = crm_OpportunitaRepository;
            _Crm_OpportunitaStorico = crm_OpportunitaStoricoRepository;
            _Crm_Par_Fase = crm_Par_FaseRepository;
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_Par_TipoAttivita = crm_Par_TipoAttivitaRepository;
            _Crm_ClienteDati = crm_ClienteDatiRepository;
            _deadlineRegistry = deadlineRegistry;
            _utility = utility;
            _email = email;
        }

        public virtual async Task<GenericResult<Crm_OffertaListOutModel>> Crm_OffertaList(GenericRequest<Crm_OffertaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                await CrmOfferteCiclo.ScadiAsync(_Crm_Offerta, _deadlineRegistry, ctx.IdCompany, ctx.IdAz_Anagrafica);
                var q = _utility.OfferteVisibili(ctx);
                if (model.Data.IdAz_Cliente.HasValue)
                    q = q.Where(o => o.IdAz_Cliente == model.Data.IdAz_Cliente);
                if (model.Data.IdCrm_Opportunita.HasValue)
                    q = q.Where(o => o.IdCrm_Opportunita == model.Data.IdCrm_Opportunita);
                return new Crm_OffertaListOutModel { Offerte = await CrmLists.OfferteAsync(q, await _utility.CfgAsync(ctx)) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_OffertaGetOutModel>> Crm_OffertaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                await CrmOfferteCiclo.ScadiAsync(_Crm_Offerta, _deadlineRegistry, ctx.IdCompany, ctx.IdAz_Anagrafica);
                var o = await Visibile(ctx, model.Data.Id, false);
                return new Crm_OffertaGetOutModel
                {
                    Offerta = Modello(o, await _utility.CfgAsync(ctx)),
                    Contatti = await CrmLists.ContattiAsync(_Az_ClienteContatto, o.IdAz_Cliente),
                    OpportunitaCliente = await CrmLists.OpportunitaAsync(_utility.OpportunitaVisibili(ctx).Where(x => x.IdAz_Cliente == o.IdAz_Cliente), _utility),
                    Allegati = await CrmLists.AllegatiAsync(_Crm_Allegato.FindAll(a => a.IdCrm_Offerta == o.Id), _utility),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaPut(GenericRequest<Crm_OffertaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Offerta;
                var cliente = await _utility.ClienteAsync(ctx, d.IdAz_Cliente);
                if (!Enum.IsDefined(d.Stato))
                    throw new UserMessageException(CrmMessages.Get("OfferStateInvalid"));
                if (d.Righe.Count == 0)
                    throw new UserMessageException(CrmMessages.Get("OfferNoLines"));
                if (d.Righe.Any(r => string.IsNullOrWhiteSpace(r.Descrizione)))
                    throw new UserMessageException(CrmMessages.Get("OfferLineDescriptionRequired"));
                if (d.IdCrm_Opportunita is int idOpp && !await _utility.OpportunitaVisibili(ctx).AnyAsync(x => x.Id == idOpp && x.IdAz_Cliente == d.IdAz_Cliente))
                    throw new UserMessageException(CrmMessages.Get("OpportunityNotFound"));

                var cfg = await _Crm_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                Crm_Offerta o;
                Crm_StatoOfferta? statoPrima = null;
                if (d.Id != 0)
                {
                    o = await Visibile(ctx, d.Id, true);
                    statoPrima = o.Stato;
                }
                else
                {
                    // numerazione per anno
                    var anno = (d.Data == default ? DateTime.Today : d.Data).Year;
                    if (cfg.OffertaAnno != anno)
                    {
                        cfg.OffertaAnno = anno;
                        cfg.OffertaUltimoNumero = 0;
                    }
                    cfg.OffertaUltimoNumero++;
                    o = new Crm_Offerta { IdAz_Anagrafica = ctx.IdAz_Anagrafica, Anno = anno, Numero = cfg.OffertaUltimoNumero, CreationDate = DateTime.Now };
                    await _Crm_Offerta.CreateAsync(o, false);
                }

                o.Data = d.Data == default ? DateTime.Today : d.Data.Date;
                o.ValidaFinoAl = d.ValidaFinoAl?.Date ?? (cfg.OffertaGiorniValidita > 0 ? o.Data.AddDays(cfg.OffertaGiorniValidita) : null);
                if (d.Stato == Crm_StatoOfferta.Inviata && o.Stato != Crm_StatoOfferta.Inviata && o.DataInvio == null)
                    o.DataInvio = DateTime.Now;
                o.Stato = d.Stato;
                if (!CrmOfferteCiclo.Esito(o.Stato))
                    o.DataEsito = null;
                else if (statoPrima != o.Stato)
                    o.DataEsito = DateTime.Now;
                o.IdAz_Cliente = d.IdAz_Cliente;
                o.IdAz_ClienteContatto = d.IdAz_ClienteContatto is int c && await _Az_ClienteContatto.FindAll(_ => true).AnyAsync(x => x.Id == c && x.IdAz_Cliente == d.IdAz_Cliente) ? c : null;
                o.IdCrm_Opportunita = d.IdCrm_Opportunita;
                o.Oggetto = string.IsNullOrWhiteSpace(d.Oggetto) ? null : d.Oggetto.Trim();
                o.Note = string.IsNullOrWhiteSpace(d.Note) ? null : d.Note.Trim();
                o.ModifiedDate = DateTime.Now;
                o.ChangeUser = ctx.IdUtente;

                // righe: si riscrivono tutte, con i totali ricalcolati
                var prodotti = await _Crm_Par_Prodotto.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Select(x => x.Id).ToListAsync();
                if (o.Id != 0)
                    await _Crm_OffertaRiga.DeleteRangeAsync(o.Righe);
                o.Righe = new List<Crm_OffertaRiga>();
                int ordine = 0;
                decimal imponibile = 0, iva = 0;
                foreach (var r in d.Righe)
                {
                    var quantita = r.Quantita;
                    var sconto = Math.Clamp(r.ScontoPerc, 0, 100);
                    var aliquota = Math.Clamp(r.AliquotaIva, 0, 100);
                    var importo = Math.Round(quantita * r.Prezzo * (1 - sconto / 100m), 2, MidpointRounding.AwayFromZero);
                    imponibile += importo;
                    iva += Math.Round(importo * aliquota / 100m, 2, MidpointRounding.AwayFromZero);
                    o.Righe.Add(new Crm_OffertaRiga
                    {
                        Ordine = (ordine += 10), IdCrm_Par_Prodotto = r.IdCrm_Par_Prodotto is int p && prodotti.Contains(p) ? p : null,
                        Descrizione = r.Descrizione.Trim(), Quantita = quantita, Um = string.IsNullOrWhiteSpace(r.Um) ? null : r.Um.Trim(),
                        Prezzo = r.Prezzo, ScontoPerc = sconto, AliquotaIva = aliquota, Importo = importo, CreationDate = DateTime.Now
                    });
                }
                o.Imponibile = imponibile;
                o.Iva = iva;
                o.Totale = imponibile + iva;
                await _Crm_Offerta.SaveChange();

                await ScadenzaAsync(ctx, o, cfg, cliente.Descrizione);
                if (o.Stato == Crm_StatoOfferta.Accettata && statoPrima != Crm_StatoOfferta.Accettata)
                    await OpportunitaVintaAsync(ctx, o);
                return new Crm_OffertaPutOutModel { Offerta = Modello(o, cfg) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_OffertaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var o = await Visibile(ctx, model.Data.Id, true);
                if (o.Stato != Crm_StatoOfferta.Bozza)
                    throw new UserMessageException(CrmMessages.Get("OfferNotDraft"));
                if (await _Crm_Allegato.FindAll(_ => true).AnyAsync(a => a.IdCrm_Offerta == o.Id))
                    throw new UserMessageException(CrmMessages.Get("OfferHasAttachments"));
                await _Crm_Offerta.DeleteAsync(o);
                await _Crm_Offerta.SaveChange();
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Crm, CrmOfferteCiclo.FonteScadenza, model.Data.Id);
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        /*
         PDF dell'offerta con un job (regola del progetto per gli export): la richiesta controlla la visibilita', raccoglie i
         dati e i testi nella lingua dell'utente e restituisce subito il JobId; il job scrive il PDF (NVXSystem.SalvaFileJob)
         e lo notifica con il link di download (ILongJobNotifier, barra dei job del client).
        */
        public virtual async Task<GenericResult<Crm_OffertaPdfJobOutModel>> Crm_OffertaPdfJob(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                    throw new UserMessageException(CrmMessages.Get("NotAllowed"));

                var dati = await DatiPdfAsync(ctx, model.Data.Id);
                var nome = Path.GetFileNameWithoutExtension(NomeFile(dati.Modello.NumeroCompleto));
                var jobId = Guid.NewGuid().ToString();
                var idOfferta = model.Data.Id;
                var payload = new { Id = idOfferta };
                var testi = new
                {
                    Avviato = CrmMessages.Get("OfferPdfJobStarted", dati.Modello.NumeroCompleto),
                    Pronto = CrmMessages.Get("OfferPdfJobReady", dati.Modello.NumeroCompleto),
                    Errore = CrmMessages.Get("OfferPdfJobFailed", dati.Modello.NumeroCompleto),
                };
                // il PDF si scrive nella lingua della richiesta (testi e formati)
                var cultura = CultureInfo.CurrentCulture;
                var culturaUi = CultureInfo.CurrentUICulture;

                RunInBackground(async scope =>
                {
                    CultureInfo.CurrentCulture = cultura;
                    CultureInfo.CurrentUICulture = culturaUi;
                    var notifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    Task Notifica(int progress, string testo, MessageType tipo, JobFile? file = null, bool fine = false) =>
                        notifier.LongJobProgressAsync(userId, new LongJobProgressUpdate
                        {
                            JobId = jobId, JobType = JobTypePdf, Payload = payload, Category = LongJobCategory.FileGeneration,
                            DownloadUrl = file?.DownloadUrl, FileName = file?.FileName, ProgressPercentage = progress,
                            Message = new Message { Text = testo, MsgType = tipo }, IsFinished = fine
                        });
                    try
                    {
                        await Notifica(10, testi.Avviato, MessageType.Information);
                        var pdf = CrmOffertaPdf.Crea(dati.Modello, dati.Azienda, dati.Cliente, dati.Contatto, dati.Valuta, dati.Condizioni);
                        var file = NVXSystem.SalvaFileJob(jobId, userId, nome, "pdf", pdf);
                        await Notifica(100, testi.Pronto, MessageType.Information, file, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "PDF dell'offerta {Id}: job {JobId} non riuscito", idOfferta, jobId);
                        await Notifica(100, testi.Errore, MessageType.Exception, null, true);
                    }
                });

                await Task.Delay(DelayAsyncMethod);
                return new Crm_OffertaPdfJobOutModel { JobId = jobId };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaInvia(GenericRequest<Crm_OffertaInviaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data;
                if (!_email.Configured)
                    throw new UserMessageException(CrmMessages.Get("EmailNotConfigured"));

                var dati = await DatiPdfAsync(ctx, d.Id);
                // destinatario: quello indicato, altrimenti l'email del contatto dell'offerta, altrimenti quella del cliente
                var email = !string.IsNullOrWhiteSpace(d.Email) ? d.Email.Trim() : (dati.EmailContatto ?? dati.Cliente.Email)?.Trim();
                if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                    throw new UserMessageException(CrmMessages.Get("EmailInvalid"));

                var numero = dati.Modello.NumeroCompleto;
                var pdf = CrmOffertaPdf.Crea(dati.Modello, dati.Azienda, dati.Cliente, dati.Contatto, dati.Valuta, dati.Condizioni);
                var oggetto = CrmMessages.Get("OfferEmailSubject", numero, dati.Azienda);
                var testo = string.IsNullOrWhiteSpace(d.Messaggio) ? CrmMessages.Get("OfferEmailBody", numero, dati.Azienda) : d.Messaggio.Trim();
                var html = "<p>" + WebUtility.HtmlEncode(testo).Replace("\n", "<br>") + "</p>";
                try
                {
                    await _email.SendAsync(email, oggetto, html, testo,
                                           new[] { new EmailAttachment { FileName = NomeFile(numero), ContentType = "application/pdf", Content = pdf } });
                }
                catch (Exception ex)
                {
                    // nessun cambio di stato e nessuna attivita': l'offerta non e' partita
                    Log.Warning(ex, "Offerta {Id}: email a {Email} non inviata", d.Id, email);
                    throw new UserMessageException(CrmMessages.Get("OfferEmailFailed", email));
                }

                var o = await Visibile(ctx, d.Id, true);
                var statoPrima = o.Stato;
                if (o.Stato == Crm_StatoOfferta.Bozza)
                    o.Stato = Crm_StatoOfferta.Inviata;
                o.DataInvio = DateTime.Now;
                o.ModifiedDate = DateTime.Now;
                o.ChangeUser = ctx.IdUtente;
                await _Crm_Offerta.SaveChange();

                await RegistraInvioAsync(ctx, o, numero, email);
                var cfg = await _utility.CfgAsync(ctx);
                if (o.Stato != statoPrima)
                    await ScadenzaAsync(ctx, o, cfg, dati.Cliente.Descrizione);
                o.Righe = await _Crm_OffertaRiga.FindAll(_ => true).Where(r => r.IdCrm_Offerta == o.Id).OrderBy(r => r.Ordine).ToListAsync();
                return new Crm_OffertaPutOutModel { Offerta = Modello(o, cfg) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        // tutto cio' che serve al PDF (anche fuori dalla richiesta, nel job)
        private sealed record DatiPdf(Crm_OffertaModel Modello, string Azienda, nvxapp.server.data.Entities.Tenant.Az_Cliente Cliente, string? Contatto,
                                      string? EmailContatto, string Valuta, string? Condizioni);

        private async Task<DatiPdf> DatiPdfAsync(CrmContesto ctx, int id)
        {
            var o = await Visibile(ctx, id, false);
            var cfg = await _utility.CfgAsync(ctx);
            var m = Modello(o, cfg);
            var cliente = await _Az_Cliente.FindAll(_ => true).AsNoTracking().FirstAsync(c => c.Id == o.IdAz_Cliente);
            var contatto = o.IdAz_ClienteContatto is int c
                ? await _Az_ClienteContatto.FindAll(x => x.Id == c).Select(x => new { Nome = ((x.Nome ?? "") + " " + (x.Cognome ?? "")).Trim(), x.Email }).FirstOrDefaultAsync()
                : null;
            var azienda = await _Company.FindAll(x => x.Id == ctx.IdCompany).Select(x => x.Descrizione).FirstOrDefaultAsync() ?? string.Empty;
            return new DatiPdf(m, azienda, cliente, contatto?.Nome, string.IsNullOrWhiteSpace(contatto?.Email) ? null : contatto!.Email, cfg.Valuta, cfg.OffertaNote);
        }

        // scadenza nello scadenzario: responsabile dell'opportunita', altrimenti del cliente, altrimenti gli amministratori
        private async Task ScadenzaAsync(CrmContesto ctx, Crm_Offerta o, Crm_Cfg cfg, string cliente)
        {
            string? responsabile = null;
            if (o.IdCrm_Opportunita is int idOpp)
                responsabile = await _Crm_Opportunita.FindAll(x => x.Id == idOpp).Select(x => x.IdAspNetUsersResponsabile).FirstOrDefaultAsync();
            responsabile ??= await _Crm_ClienteDati.FindAll(x => x.IdAz_Cliente == o.IdAz_Cliente).Select(x => x.IdAspNetUsersResponsabile).FirstOrDefaultAsync();
            await CrmOfferteCiclo.ScadenzaAsync(_deadlineRegistry, ctx.IdCompany, o, CrmLists.NumeroOfferta(cfg, o.Anno, o.Numero), cliente, responsabile);
        }

        /*
         Offerta accettata (CRM-PREV.3): l'opportunita' collegata, se ancora aperta, passa alla prima fase "vinta" con
         probabilita' 100, chiusura oggi e valore = imponibile dell'offerta; il passaggio va nello storico (report).
         Senza fasi "vinte" attive o con l'opportunita' gia' chiusa non cambia nulla.
        */
        private async Task OpportunitaVintaAsync(CrmContesto ctx, Crm_Offerta o)
        {
            if (o.IdCrm_Opportunita is not int idOpp)
                return;
            var opp = await _Crm_Opportunita.FindAll(x => x.Id == idOpp && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                            .AsTracking().FirstOrDefaultAsync();
            if (opp == null)
                return;
            var faseAttuale = await _Crm_Par_Fase.FindAll(x => x.Id == opp.IdCrm_Par_Fase).Select(x => x.Tipo).FirstOrDefaultAsync();
            if (faseAttuale != Crm_FaseTipo.Aperta)
                return;
            var vinta = await _Crm_Par_Fase.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Tipo == Crm_FaseTipo.Vinta && x.Attivo)
                                          .OrderBy(x => x.Ordine).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            if (vinta == null)
                return;

            int faseDa = opp.IdCrm_Par_Fase;
            decimal valoreDa = opp.Valore;
            opp.IdCrm_Par_Fase = vinta.Value;
            opp.Probabilita = 100;
            opp.DataChiusura = DateTime.Today;
            opp.IdCrm_Par_MotivoPerdita = null;
            opp.Valore = o.Imponibile;
            opp.ModifiedDate = DateTime.Now;
            opp.ChangeUser = ctx.IdUtente;
            await _Crm_OpportunitaStorico.CreateAsync(new Crm_OpportunitaStorico
            {
                IdCrm_Opportunita = opp.Id, Data = DateTime.Now, IdAspNetUsers = ctx.IdUtente, IdCrm_Par_FaseDa = faseDa,
                IdCrm_Par_FaseA = vinta.Value, ValoreDa = valoreDa, ValoreA = opp.Valore, CreationDate = DateTime.Now
            }, false);
            await _Crm_Opportunita.SaveChange();
        }

        // invio registrato come attivita' fatta (tipo "email": icona mail, altrimenti il primo tipo attivo)
        private async Task RegistraInvioAsync(CrmContesto ctx, Crm_Offerta o, string numero, string email)
        {
            var tipo = await _Crm_Par_TipoAttivita.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attivo)
                                                  .OrderBy(x => x.Icona == "mail" ? 0 : 1).ThenBy(x => x.Ordine)
                                                  .Select(x => (int?)x.Id).FirstOrDefaultAsync();
            if (tipo == null || string.IsNullOrEmpty(ctx.IdUtente))
                return;
            var ora = DateTime.Now;
            await _Crm_Attivita.CreateAsync(new Crm_Attivita
            {
                IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdCrm_Par_TipoAttivita = tipo.Value,
                Oggetto = CrmMessages.Get("OfferSentActivitySubject", numero), Descrizione = o.Oggetto,
                Inizio = ora, Fine = ora, IdAspNetUsersAssegnatario = ctx.IdUtente, Stato = Crm_StatoAttivita.Fatta,
                Esito = CrmMessages.Get("OfferSentActivityOutcome", email),
                IdAz_Cliente = o.IdAz_Cliente, IdAz_ClienteContatto = o.IdAz_ClienteContatto, IdCrm_Opportunita = o.IdCrm_Opportunita,
                PromemoriaInviato = true, CreationDate = ora, ChangeUser = ctx.IdUtente
            });
        }

        private static string NomeFile(string numero) => CrmMessages.Get("OfferFileName") + "_" + numero.Replace('/', '-') + ".pdf";

        private async Task<Crm_Offerta> Visibile(CrmContesto ctx, int id, bool tracking)
        {
            var q = _utility.OfferteVisibili(ctx).Include(o => o.Righe);
            var o = await (tracking ? q.AsTracking() : q.AsNoTracking()).FirstOrDefaultAsync(o => o.Id == id) ?? throw new UserMessageException(CrmMessages.Get("OfferNotFound"));
            o.Righe = o.Righe.OrderBy(r => r.Ordine).ToList();
            return o;
        }

        private static Crm_OffertaModel Modello(Crm_Offerta o, Crm_Cfg cfg) => new()
        {
            Id = o.Id, Anno = o.Anno, Numero = o.Numero, NumeroCompleto = CrmLists.NumeroOfferta(cfg, o.Anno, o.Numero), Data = o.Data, ValidaFinoAl = o.ValidaFinoAl,
            Stato = o.Stato, IdAz_Cliente = o.IdAz_Cliente, IdAz_ClienteContatto = o.IdAz_ClienteContatto, IdCrm_Opportunita = o.IdCrm_Opportunita,
            Oggetto = o.Oggetto, Note = o.Note, Imponibile = o.Imponibile, Iva = o.Iva, Totale = o.Totale, DataInvio = o.DataInvio,
            DataEsito = o.DataEsito, IdOrdineAmministrazione = o.IdOrdineAmministrazione,
            Righe = o.Righe.OrderBy(r => r.Ordine).Select(r => new Crm_OffertaRigaModel
            {
                Id = r.Id, IdCrm_Par_Prodotto = r.IdCrm_Par_Prodotto, Descrizione = r.Descrizione, Quantita = r.Quantita, Um = r.Um, Prezzo = r.Prezzo,
                ScontoPerc = r.ScontoPerc, AliquotaIva = r.AliquotaIva, Importo = r.Importo
            }).ToList()
        };
    }

    public interface ICrm_OffertaService : IServiceBase
    {
        Task<GenericResult<Crm_OffertaListOutModel>> Crm_OffertaList(GenericRequest<Crm_OffertaListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OffertaGetOutModel>> Crm_OffertaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaPut(GenericRequest<Crm_OffertaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_OffertaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OffertaPdfJobOutModel>> Crm_OffertaPdfJob(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaInvia(GenericRequest<Crm_OffertaInviaInModel> model, Boolean isSubProcess);
    }
}
