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
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Configurazione del CRM (ANALISI_CRM_APPLICATIVO.md §5.1): impostazioni e tabelle (fasi, tipi di attivita', motivi di perdita,
     fonti, prodotti, tag). Le legge chiunque lavori nel CRM (servono alle scelte); le modifica il CompanyPowerAdmin.
     Riassegnazione in blocco dei dati di un commerciale: CompanyPowerAdmin e CompanyAdmin.
    */
    public class Crm_ConfigService : ServiceBase, ICrm_ConfigService
    {
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_CfgRepository _Crm_Cfg;
        private readonly ICrm_ClienteDatiRepository _Crm_ClienteDati;
        private readonly ICrm_LeadRepository _Crm_Lead;
        private readonly ICrm_OffertaRigaRepository _Crm_OffertaRiga;
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly ICrm_Par_FaseRepository _Crm_Par_Fase;
        private readonly ICrm_Par_FonteRepository _Crm_Par_Fonte;
        private readonly ICrm_Par_MotivoPerditaRepository _Crm_Par_MotivoPerdita;
        private readonly ICrm_Par_ProdottoRepository _Crm_Par_Prodotto;
        private readonly ICrm_Par_TagRepository _Crm_Par_Tag;
        private readonly ICrm_Par_TipoAttivitaRepository _Crm_Par_TipoAttivita;
        private readonly ICrmUtility _utility;

        public Crm_ConfigService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 ICrm_AttivitaRepository crm_AttivitaRepository,
                                 ICrm_CfgRepository crm_CfgRepository,
                                 ICrm_ClienteDatiRepository crm_ClienteDatiRepository,
                                 ICrm_LeadRepository crm_LeadRepository,
                                 ICrm_OffertaRigaRepository crm_OffertaRigaRepository,
                                 ICrm_OpportunitaRepository crm_OpportunitaRepository,
                                 ICrm_Par_FaseRepository crm_Par_FaseRepository,
                                 ICrm_Par_FonteRepository crm_Par_FonteRepository,
                                 ICrm_Par_MotivoPerditaRepository crm_Par_MotivoPerditaRepository,
                                 ICrm_Par_ProdottoRepository crm_Par_ProdottoRepository,
                                 ICrm_Par_TagRepository crm_Par_TagRepository,
                                 ICrm_Par_TipoAttivitaRepository crm_Par_TipoAttivitaRepository,
                                 ICrmUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_Cfg = crm_CfgRepository;
            _Crm_ClienteDati = crm_ClienteDatiRepository;
            _Crm_Lead = crm_LeadRepository;
            _Crm_OffertaRiga = crm_OffertaRigaRepository;
            _Crm_Opportunita = crm_OpportunitaRepository;
            _Crm_Par_Fase = crm_Par_FaseRepository;
            _Crm_Par_Fonte = crm_Par_FonteRepository;
            _Crm_Par_MotivoPerdita = crm_Par_MotivoPerditaRepository;
            _Crm_Par_Prodotto = crm_Par_ProdottoRepository;
            _Crm_Par_Tag = crm_Par_TagRepository;
            _Crm_Par_TipoAttivita = crm_Par_TipoAttivitaRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Crm_SetupGetOutModel>> Crm_SetupGet(GenericRequest<Crm_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var cfg = await _utility.CfgAsync(ctx);
                int az = ctx.IdAz_Anagrafica;
                return new Crm_SetupGetOutModel
                {
                    Permessi = new Crm_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin, VedeTutto = ctx.VedeTutto, IdUtente = ctx.IdUtente },
                    Cfg = CfgModel(cfg),
                    Fasi = (await _Crm_Par_Fase.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ToListAsync()).Select(Par).ToList(),
                    TipiAttivita = (await _Crm_Par_TipoAttivita.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ToListAsync()).Select(Par).ToList(),
                    MotiviPerdita = (await _Crm_Par_MotivoPerdita.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ToListAsync()).Select(Par).ToList(),
                    Fonti = (await _Crm_Par_Fonte.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ToListAsync()).Select(Par).ToList(),
                    Prodotti = (await _Crm_Par_Prodotto.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ThenBy(x => x.Descrizione).ToListAsync()).Select(Par).ToList(),
                    Tag = (await _Crm_Par_Tag.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == az).OrderBy(x => x.Ordine).ThenBy(x => x.Descrizione).ToListAsync()).Select(Par).ToList(),
                    Utenti = await _utility.UtentiAsync(ctx),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_CfgPutOutModel>> Crm_CfgPut(GenericRequest<Crm_CfgPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var d = model.Data.Cfg;
                var cfg = await _Crm_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                cfg.Visibilita = d.Visibilita;
                cfg.Valuta = string.IsNullOrWhiteSpace(d.Valuta) ? "EUR" : d.Valuta.Trim().ToUpperInvariant()[..Math.Min(3, d.Valuta.Trim().Length)];
                cfg.MinutiPreavviso = Math.Clamp(d.MinutiPreavviso, 0, 7 * 24 * 60);
                cfg.OffertaPrefisso = string.IsNullOrWhiteSpace(d.OffertaPrefisso) ? null : d.OffertaPrefisso.Trim();
                cfg.OffertaGiorniValidita = Math.Clamp(d.OffertaGiorniValidita, 0, 3650);
                cfg.OffertaNote = string.IsNullOrWhiteSpace(d.OffertaNote) ? null : d.OffertaNote.Trim();
                cfg.ModifiedDate = DateTime.Now;
                cfg.ChangeUser = ctx.IdUtente;
                await _Crm_Attivita.SaveChange();
                return new Crm_CfgPutOutModel { Cfg = CfgModel(cfg) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_ParPutOutModel>> Crm_ParPut(GenericRequest<Crm_ParPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var d = model.Data.Par;
                if (string.IsNullOrWhiteSpace(d.Descrizione))
                    throw new UserMessageException(CrmMessages.Get("DescriptionRequired"));

                Crm_ParBase row = d.Tabella switch
                {
                    Crm_ParTabella.Fase => await Riga(_Crm_Par_Fase, d.Id, ctx),
                    Crm_ParTabella.TipoAttivita => await Riga(_Crm_Par_TipoAttivita, d.Id, ctx),
                    Crm_ParTabella.MotivoPerdita => await Riga(_Crm_Par_MotivoPerdita, d.Id, ctx),
                    Crm_ParTabella.Fonte => await Riga(_Crm_Par_Fonte, d.Id, ctx),
                    Crm_ParTabella.Prodotto => await Riga(_Crm_Par_Prodotto, d.Id, ctx),
                    Crm_ParTabella.Tag => await Riga(_Crm_Par_Tag, d.Id, ctx),
                    _ => throw new UserMessageException(CrmMessages.Get("NotAllowed"))
                };

                // almeno una fase aperta, una vinta e una persa devono restare attive (la pipeline e i report ne hanno bisogno)
                if (row is Crm_Par_Fase faseEsistente && faseEsistente.Id != 0 && (faseEsistente.Tipo != d.Tipo || !d.Attivo))
                    await ControllaFasi(ctx, faseEsistente.Id, faseEsistente.Tipo);

                row.Descrizione = d.Descrizione.Trim();
                row.Ordine = d.Ordine;
                row.Attivo = d.Attivo;
                row.ModifiedDate = DateTime.Now;
                row.ChangeUser = ctx.IdUtente;
                switch (row)
                {
                    case Crm_Par_Fase f: f.Probabilita = Math.Clamp(d.Probabilita, 0, 100); f.Tipo = d.Tipo; break;
                    case Crm_Par_TipoAttivita t: t.Icona = d.Icona; t.Colore = d.Colore; t.DurataMinuti = Math.Clamp(d.DurataMinuti, 0, 24 * 60); break;
                    case Crm_Par_Prodotto p: p.Codice = string.IsNullOrWhiteSpace(d.Codice) ? null : d.Codice.Trim(); p.Prezzo = d.Prezzo; p.Um = d.Um; p.AliquotaIva = Math.Clamp(d.AliquotaIva, 0, 100); break;
                    case Crm_Par_Tag g: g.Colore = d.Colore; break;
                }
                await _Crm_Attivita.SaveChange();
                return new Crm_ParPutOutModel { Par = Par(row) };
            }, isSubProcess);
        }

        // si cancella solo se non e' usata (altrimenti si disattiva)
        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_ParDelete(GenericRequest<Crm_ParDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                int id = model.Data.Id;
                bool usata = model.Data.Tabella switch
                {
                    Crm_ParTabella.Fase => await _Crm_Opportunita.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_Fase == id),
                    Crm_ParTabella.TipoAttivita => await _Crm_Attivita.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_TipoAttivita == id),
                    Crm_ParTabella.MotivoPerdita => await _Crm_Opportunita.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_MotivoPerdita == id),
                    Crm_ParTabella.Fonte => await _Crm_ClienteDati.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_Fonte == id) || await _Crm_Lead.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_Fonte == id) || await _Crm_Opportunita.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_Fonte == id),
                    Crm_ParTabella.Prodotto => await _Crm_OffertaRiga.FindAll(_ => true).AnyAsync(x => x.IdCrm_Par_Prodotto == id),
                    _ => false,
                };
                if (usata)
                    throw new UserMessageException(CrmMessages.Get("ParInUse"));

                switch (model.Data.Tabella)
                {
                    case Crm_ParTabella.Fase:
                        var fase = await Riga(_Crm_Par_Fase, id, ctx, false);
                        await ControllaFasi(ctx, fase.Id, fase.Tipo);
                        await _Crm_Par_Fase.DeleteAsync(fase);
                        break;
                    case Crm_ParTabella.TipoAttivita:
                        if (await _Crm_Par_TipoAttivita.FindAll(_ => true).CountAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attivo && x.Id != id) == 0)
                            throw new UserMessageException(CrmMessages.Get("LastActivityType"));
                        await _Crm_Par_TipoAttivita.DeleteAsync(await Riga(_Crm_Par_TipoAttivita, id, ctx, false));
                        break;
                    case Crm_ParTabella.MotivoPerdita: await _Crm_Par_MotivoPerdita.DeleteAsync(await Riga(_Crm_Par_MotivoPerdita, id, ctx, false)); break;
                    case Crm_ParTabella.Fonte: await _Crm_Par_Fonte.DeleteAsync(await Riga(_Crm_Par_Fonte, id, ctx, false)); break;
                    case Crm_ParTabella.Prodotto: await _Crm_Par_Prodotto.DeleteAsync(await Riga(_Crm_Par_Prodotto, id, ctx, false)); break;
                    case Crm_ParTabella.Tag: await _Crm_Par_Tag.DeleteAsync(await Riga(_Crm_Par_Tag, id, ctx, false)); break;
                }
                await _Crm_Attivita.SaveChange();
                return new Crm_IdOutModel { Id = id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_RiassegnaOutModel>> Crm_Riassegna(GenericRequest<Crm_RiassegnaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.Amministratore)
                    throw new UserMessageException(CrmMessages.Get("NotAllowed"));
                var d = model.Data;
                var utenti = (await _utility.UtentiAsync(ctx)).Select(u => u.Id).ToHashSet();
                if (string.IsNullOrEmpty(d.IdDa) || (d.IdA != null && !utenti.Contains(d.IdA)) || d.IdA == d.IdDa)
                    throw new UserMessageException(CrmMessages.Get("ReassignInvalid"));

                int az = ctx.IdAz_Anagrafica;
                var now = DateTime.Now;
                var retVal = new Crm_RiassegnaOutModel();
                if (d.Clienti)
                    retVal.Clienti = await _Crm_ClienteDati.FindAll(x => x.Az_ClienteNavigation!.IdAz_Anagrafica == az && x.IdAspNetUsersResponsabile == d.IdDa)
                                              .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, d.IdA).SetProperty(x => x.ModifiedDate, now));
                if (d.Opportunita)
                    retVal.Opportunita = await _Crm_Opportunita.FindAll(x => x.IdAz_Anagrafica == az && x.IdAspNetUsersResponsabile == d.IdDa)
                                                  .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, d.IdA).SetProperty(x => x.ModifiedDate, now));
                if (d.Lead)
                    retVal.Lead = await _Crm_Lead.FindAll(x => x.IdAz_Anagrafica == az && x.IdAspNetUsersResponsabile == d.IdDa)
                                           .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersResponsabile, d.IdA).SetProperty(x => x.ModifiedDate, now));
                // le attivita' hanno sempre un assegnatario: senza destinatario restano a chi le aveva
                if (d.AttivitaDaFare && d.IdA != null)
                    retVal.Attivita = await _Crm_Attivita.FindAll(x => x.IdAz_Anagrafica == az && x.IdAspNetUsersAssegnatario == d.IdDa && x.Stato == Crm_StatoAttivita.DaFare)
                                               .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsersAssegnatario, d.IdA).SetProperty(x => x.ModifiedDate, now));
                return retVal;
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<CrmContesto> SoloPowerAdmin()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.PowerAdmin)
                throw new UserMessageException(CrmMessages.Get("NotAllowed"));
            return ctx;
        }

        // riga della tabella (nuova se id = 0) letta con il suo repository, tracciata per le modifiche
        private async Task<T> Riga<T>(IRepository<T> repo, int id, CrmContesto ctx, bool nuovaSeZero = true) where T : Crm_ParBase, new()
        {
            if (id == 0 && nuovaSeZero)
            {
                var nuova = new T { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                await repo.CreateAsync(nuova, false);
                return nuova;
            }
            return await repo.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                   ?? throw new UserMessageException(CrmMessages.Get("ParNotFound"));
        }

        private async Task ControllaFasi(CrmContesto ctx, int idEscluso, Crm_FaseTipo tipo)
        {
            if (!await _Crm_Par_Fase.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attivo && x.Tipo == tipo && x.Id != idEscluso))
                throw new UserMessageException(CrmMessages.Get("LastPhaseOfType"));
        }

        private static Crm_CfgModel CfgModel(Crm_Cfg c) => new()
        {
            Visibilita = c.Visibilita, Valuta = c.Valuta, MinutiPreavviso = c.MinutiPreavviso, OffertaPrefisso = c.OffertaPrefisso,
            OffertaGiorniValidita = c.OffertaGiorniValidita, OffertaNote = c.OffertaNote
        };

        public static Crm_ParModel Par(Crm_ParBase x)
        {
            var m = new Crm_ParModel { Id = x.Id, Descrizione = x.Descrizione, Ordine = x.Ordine, Attivo = x.Attivo };
            switch (x)
            {
                case Crm_Par_Fase f: m.Tabella = Crm_ParTabella.Fase; m.Probabilita = f.Probabilita; m.Tipo = f.Tipo; break;
                case Crm_Par_TipoAttivita t: m.Tabella = Crm_ParTabella.TipoAttivita; m.Icona = t.Icona; m.Colore = t.Colore; m.DurataMinuti = t.DurataMinuti; break;
                case Crm_Par_MotivoPerdita: m.Tabella = Crm_ParTabella.MotivoPerdita; break;
                case Crm_Par_Fonte: m.Tabella = Crm_ParTabella.Fonte; break;
                case Crm_Par_Prodotto p: m.Tabella = Crm_ParTabella.Prodotto; m.Codice = p.Codice; m.Prezzo = p.Prezzo; m.Um = p.Um; m.AliquotaIva = p.AliquotaIva; break;
                case Crm_Par_Tag g: m.Tabella = Crm_ParTabella.Tag; m.Colore = g.Colore; break;
            }
            return m;
        }
    }

    public interface ICrm_ConfigService : IServiceBase
    {
        Task<GenericResult<Crm_SetupGetOutModel>> Crm_SetupGet(GenericRequest<Crm_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_CfgPutOutModel>> Crm_CfgPut(GenericRequest<Crm_CfgPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_ParPutOutModel>> Crm_ParPut(GenericRequest<Crm_ParPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_ParDelete(GenericRequest<Crm_ParDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_RiassegnaOutModel>> Crm_Riassegna(GenericRequest<Crm_RiassegnaInModel> model, Boolean isSubProcess);
    }
}
