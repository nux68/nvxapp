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
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Net;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Offerte (ANALISI_CRM_APPLICATIVO.md §6.1): righe con prodotto, quantita', prezzo, sconto e IVA; totali calcolati qui.
     Numerazione per anno (Crm_Cfg: prefisso, anno, ultimo numero) assegnata al primo salvataggio.
     PDF con MigraDoc (CrmOffertaPdf) e invio per email (IEmailService, PDF allegato): l'invio la segna "Inviata".
    */
    public class Crm_OffertaService : ServiceBase, ICrm_OffertaService
    {
        private readonly ICrm_OffertaRepository _Crm_Offerta;
        private readonly ICrm_OffertaRigaRepository _Crm_OffertaRiga;
        private readonly ICrm_Az_ClienteReadRepository _Az_Cliente;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_AllegatoRepository _Crm_Allegato;
        private readonly ICrm_CfgRepository _Crm_Cfg;
        private readonly ICrm_Par_ProdottoRepository _Crm_Par_Prodotto;
        private readonly ICompanyRepository _Company;
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
            _utility = utility;
            _email = email;
        }

        public virtual async Task<GenericResult<Crm_OffertaListOutModel>> Crm_OffertaList(GenericRequest<Crm_OffertaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
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
                await _utility.ClienteAsync(ctx, d.IdAz_Cliente);
                if (d.Righe.Count == 0)
                    throw new UserMessageException(CrmMessages.Get("OfferNoLines"));
                if (d.Righe.Any(r => string.IsNullOrWhiteSpace(r.Descrizione)))
                    throw new UserMessageException(CrmMessages.Get("OfferLineDescriptionRequired"));
                if (d.IdCrm_Opportunita is int idOpp && !await _utility.OpportunitaVisibili(ctx).AnyAsync(x => x.Id == idOpp && x.IdAz_Cliente == d.IdAz_Cliente))
                    throw new UserMessageException(CrmMessages.Get("OpportunityNotFound"));

                var cfg = await _Crm_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                Crm_Offerta o;
                if (d.Id != 0)
                    o = await Visibile(ctx, d.Id, true);
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
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        public virtual async Task<Crm_FileDownload> Crm_OffertaPdf(int id)
        {
            var ctx = await _utility.ContestoAsync();
            var (pdf, numero, _) = await PdfAsync(ctx, id);
            return new Crm_FileDownload { Content = new MemoryStream(pdf), NomeFile = NomeFile(numero), ContentType = "application/pdf" };
        }

        public virtual async Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaInvia(GenericRequest<Crm_OffertaInviaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data;
                if (!_email.Configured)
                    throw new UserMessageException(CrmMessages.Get("EmailNotConfigured"));
                if (string.IsNullOrWhiteSpace(d.Email) || !d.Email.Contains('@'))
                    throw new UserMessageException(CrmMessages.Get("EmailInvalid"));

                var (pdf, numero, azienda) = await PdfAsync(ctx, d.Id);
                var oggetto = CrmMessages.Get("OfferEmailSubject", numero, azienda);
                var testo = string.IsNullOrWhiteSpace(d.Messaggio) ? CrmMessages.Get("OfferEmailBody", numero, azienda) : d.Messaggio.Trim();
                var html = "<p>" + WebUtility.HtmlEncode(testo).Replace("\n", "<br>") + "</p>";
                await _email.SendAsync(d.Email.Trim(), oggetto, html, testo,
                                       new[] { new EmailAttachment { FileName = NomeFile(numero), ContentType = "application/pdf", Content = pdf } });

                var o = await Visibile(ctx, d.Id, true);
                if (o.Stato == Crm_StatoOfferta.Bozza)
                    o.Stato = Crm_StatoOfferta.Inviata;
                o.DataInvio = DateTime.Now;
                o.ModifiedDate = DateTime.Now;
                o.ChangeUser = ctx.IdUtente;
                await _Crm_Offerta.SaveChange();
                o.Righe = await _Crm_OffertaRiga.FindAll(_ => true).Where(r => r.IdCrm_Offerta == o.Id).OrderBy(r => r.Ordine).ToListAsync();
                return new Crm_OffertaPutOutModel { Offerta = Modello(o, await _utility.CfgAsync(ctx)) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<(byte[] Pdf, string Numero, string Azienda)> PdfAsync(CrmContesto ctx, int id)
        {
            var o = await Visibile(ctx, id, false);
            var cfg = await _utility.CfgAsync(ctx);
            var m = Modello(o, cfg);
            var cliente = await _Az_Cliente.FindAll(_ => true).FirstAsync(c => c.Id == o.IdAz_Cliente);
            var contatto = o.IdAz_ClienteContatto is int c
                ? await _Az_ClienteContatto.FindAll(x => x.Id == c).Select(x => ((x.Nome ?? "") + " " + (x.Cognome ?? "")).Trim()).FirstOrDefaultAsync()
                : null;
            var azienda = await _Company.FindAll(x => x.Id == ctx.IdCompany).Select(x => x.Descrizione).FirstOrDefaultAsync() ?? string.Empty;
            return (CrmOffertaPdf.Crea(m, azienda, cliente, contatto, cfg.Valuta, cfg.OffertaNote), m.NumeroCompleto, azienda);
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
        Task<Crm_FileDownload> Crm_OffertaPdf(int id);
        Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaInvia(GenericRequest<Crm_OffertaInviaInModel> model, Boolean isSubProcess);
    }
}
