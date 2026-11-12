using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Pagine delle richieste da approvare (ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis): legge Dip_GG_Richiesta (schema Common)
     dell'azienda corrente, solo dei tipi dichiarati dai gestori (IRichiestaTipoHandler) degli applicativi attivi.
        - mie richieste: quelle fatte dall'utente collegato (IdAspNetUsers), escluse le annullate prima della decisione
        - da approvare (amministratori dell'azienda): quelle che l'utente puo' decidere adesso (RichiestaRegole.PuoDecidere,
          strategia del gestore del tipo); "Tutte" = tutte quelle dell'azienda, "Approvate" = le approvate
        - testi della riga e azioni dal gestore del tipo; un gestore che fallisce nei testi finisce nel log e non blocca gli altri
    */
    public class RequestService : ServiceBase, IRequestService
    {
        private readonly IEnumerable<IRichiestaTipoHandler> _handlers;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IAz_AnagraficaRepository _az_AnagraficaRepository;
        private readonly IDip_AnagraficaCommonRepository _dip_AnagraficaRepository;
        private readonly IDip_GG_RichiestaCommonRepository _dip_GG_RichiestaRepository;

        public RequestService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,

                              IEnumerable<IRichiestaTipoHandler> handlers,
                              ICompanyApplicationRepository companyApplicationRepository,
                              IAz_AnagraficaRepository az_AnagraficaRepository,
                              IDip_AnagraficaCommonRepository dip_AnagraficaRepository,
                              IDip_GG_RichiestaCommonRepository dip_GG_RichiestaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _handlers = handlers;
            _companyApplicationRepository = companyApplicationRepository;
            _az_AnagraficaRepository = az_AnagraficaRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
        }

        public virtual async Task<GenericResult<RequestCountOutModel>> RequestCount(GenericRequest<RequestCountInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestCountOutModel();
                var ctx = await ContestoAsync();
                if (ctx != null)
                {
                    var (counts, perApp) = await ContaAsync(ctx);
                    retVal.DaApprovare = counts.DaApprovare;
                    retVal.InAttesa = counts.InAttesa;
                    retVal.PerApplicativo = perApp;
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<RequestListOutModel>> RequestList(GenericRequest<RequestListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestListOutModel();
                var ctx = await ContestoAsync();
                if (ctx == null)
                    return retVal;
                if (model.Data.Admin && !ctx.Amministratore)
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));

                var scope = model.Data.Admin ? RequestScope.Admin : RequestScope.Mine;
                var filtro = model.Data.Filtro;

                var richieste = await QueryAsync(ctx, scope == RequestScope.Mine);
                var nomi = await RichiedentiAsync(richieste);

                var righe = new List<(Dip_GG_Richiesta Richiesta, RequestItem Item, IRichiestaTipoHandler Handler)>();
                foreach (var r in richieste)
                {
                    var h = ctx.Handlers[r.RichiestaTipo];
                    var strategia = ctx.Strategie[h];
                    bool puoDecidere = ctx.Amministratore && RichiestaRegole.PuoDecidere(r, ctx.IdAspNetUsers, ctx.Amministratore, strategia);
                    var stato = RichiestaRegole.StatoComune(r);

                    bool tenere = (scope, filtro) switch
                    {
                        (RequestScope.Mine, RequestFilter.DaApprovare) => RichiestaRegole.InAttesa(r),
                        (RequestScope.Admin, RequestFilter.DaApprovare) => puoDecidere,
                        (_, RequestFilter.Approvate) => stato == RequestState.Approvata,
                        _ => true,
                    };
                    if (!tenere)
                        continue;

                    righe.Add((r, new RequestItem
                    {
                        Key = r.Id.ToString(),
                        Id = r.Id.ToString(),
                        Application = (int)h.Application,
                        Tipo = ((int)r.RichiestaTipo).ToString(),
                        TipoTesto = r.RichiestaTipo.ToString(),
                        Richiedente = nomi.GetValueOrDefault(r.Id),
                        Dal = r.Data,
                        Al = r.DataA,
                        Stato = stato,
                        StatoTesto = RichiestaRegole.TestoStato(r),
                        CanApprove = scope == RequestScope.Admin && puoDecidere,
                        CanReject = scope == RequestScope.Admin && puoDecidere,
                        CanCancel = scope == RequestScope.Mine && r.IdAspNetUsers == ctx.IdAspNetUsers && RichiestaRegole.PuoAnnullare(r),
                        CancelKind = RichiestaRegole.TipoAnnullamento(r),
                        DataRichiesta = r.CreationDate,
                    }, h));
                }

                // testi del modulo, un gestore per volta
                foreach (var gruppo in righe.GroupBy(x => x.Handler))
                {
                    try
                    {
                        await gruppo.Key.CompletaAsync(gruppo.Select(x => (x.Richiesta, x.Item)).ToList(), scope);
                    }
                    catch (Exception ex) when (ex is not UserMessageException)
                    {
                        Log.Warning(ex, "Richieste: testi non disponibili per {Application}", gruppo.Key.Application);
                    }
                }

                retVal.Richieste = righe.Select(x => x.Item).OrderByDescending(x => x.Dal).ToList();
                retVal.Applicativi = righe.Select(x => x.Item.Application).Distinct().OrderBy(x => x).ToList();

                var (counts, perApp) = await ContaAsync(ctx);
                retVal.DaApprovare = counts.DaApprovare;
                retVal.InAttesa = counts.InAttesa;
                retVal.PerApplicativo = perApp;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestApprove(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (h, r) => h.ApproveAsync(r));

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestReject(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (h, r) => h.RejectAsync(r, string.IsNullOrWhiteSpace(model.Data.Motivo) ? null : model.Data.Motivo.Trim()));

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestCancel(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (h, r) => h.CancelAsync(r));

        // ---------------------------------------------------------------------------

        private async Task<GenericResult<RequestActionOutModel>> AzioneAsync(GenericRequest<RequestActionInModel> model, Boolean isSubProcess, Func<IRichiestaTipoHandler, Dip_GG_Richiesta, Task> azione)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestActionOutModel();

                var ctx = await ContestoAsync() ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));
                if (!int.TryParse(model.Data.Key, out int id))
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

                var tipi = ctx.Handlers.Keys.ToList();
                var r = await _dip_GG_RichiestaRepository.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && tipi.Contains(x.RichiestaTipo))
                                                         .AsNoTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

                await azione(ctx.Handlers[r.RichiestaTipo], r);

                var (counts, perApp) = await ContaAsync(ctx);
                retVal.DaApprovare = counts.DaApprovare;
                retVal.InAttesa = counts.InAttesa;
                retVal.PerApplicativo = perApp;
                return retVal;
            }, isSubProcess);
        }

        // contatori totali e per applicativo (per i badge delle voci dei moduli)
        private async Task<(RequestCounts Totale, List<RequestCountApp> PerApplicativo)> ContaAsync(Contesto ctx)
        {
            var tot = new RequestCounts();
            var perApp = new Dictionary<int, RequestCountApp>();
            foreach (var r in await QueryAsync(ctx, !ctx.Amministratore, soloInAttesa: true))
            {
                var h = ctx.Handlers[r.RichiestaTipo];
                if (!perApp.TryGetValue((int)h.Application, out var app))
                    perApp[(int)h.Application] = app = new RequestCountApp { Application = (int)h.Application };
                if (r.IdAspNetUsers == ctx.IdAspNetUsers)
                {
                    tot.InAttesa++;
                    app.InAttesa++;
                }
                if (ctx.Amministratore && RichiestaRegole.PuoDecidere(r, ctx.IdAspNetUsers, true, ctx.Strategie[h]))
                {
                    tot.DaApprovare++;
                    app.DaApprovare++;
                }
            }
            return (tot, perApp.Values.OrderBy(x => x.Application).ToList());
        }

        // richieste dell'azienda dei tipi gestiti (dell'utente se mie), escluse quelle annullate prima della decisione
        private async Task<List<Dip_GG_Richiesta>> QueryAsync(Contesto ctx, bool mie, bool soloInAttesa = false)
        {
            var tipi = ctx.Handlers.Keys.ToList();
            var q = _dip_GG_RichiestaRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && tipi.Contains(x.RichiestaTipo)
                                                             && x.RichiestaStato != StatoRichiesta.Cancellata);
            if (mie)
                q = q.Where(x => x.IdAspNetUsers == ctx.IdAspNetUsers);
            if (soloInAttesa)
            {
                var stati = RichiestaRegole.StatiInAttesa;
                q = q.Where(x => stati.Contains(x.RichiestaStato) || (x.RevocaStato != null && stati.Contains(x.RevocaStato.Value)));
            }
            return await q.AsNoTracking().ToListAsync();
        }

        // nome del richiedente: dipendente (cognome nome) o, se non censito, nome dell'utente
        private async Task<Dictionary<int, string>> RichiedentiAsync(List<Dip_GG_Richiesta> richieste)
        {
            var idDip = richieste.Where(x => x.IdDip_Anagrafica != null).Select(x => x.IdDip_Anagrafica!.Value).Distinct().ToList();
            var dip = await _dip_AnagraficaRepository.FindAll(x => idDip.Contains(x.Id))
                                                     .Select(x => new { x.Id, x.Cognome, x.Nome }).AsNoTracking().ToListAsync();
            var idUsr = richieste.Where(x => x.IdDip_Anagrafica == null && x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).Distinct().ToList();
            var usr = await _userManager.Users.Where(x => idUsr.Contains(x.Id)).Select(x => new { x.Id, x.UserName }).ToListAsync();

            var res = new Dictionary<int, string>();
            foreach (var r in richieste)
            {
                string? nome = null;
                if (r.IdDip_Anagrafica != null)
                {
                    var d = dip.FirstOrDefault(x => x.Id == r.IdDip_Anagrafica);
                    nome = d == null ? null : $"{d.Cognome} {d.Nome}".Trim();
                }
                else if (r.IdAspNetUsers != null)
                    nome = usr.FirstOrDefault(x => x.Id == r.IdAspNetUsers)?.UserName;
                if (!string.IsNullOrEmpty(nome))
                    res[r.Id] = nome;
            }
            return res;
        }

        private sealed class Contesto
        {
            public int IdAz_Anagrafica { get; init; }
            public string IdAspNetUsers { get; init; } = string.Empty;
            public bool Amministratore { get; init; }
            public Dictionary<TipoRichiesta, IRichiestaTipoHandler> Handlers { get; init; } = new();
            public Dictionary<IRichiestaTipoHandler, StrategiaApprovazione> Strategie { get; init; } = new();
        }

        // azienda corrente, ruolo e gestori degli applicativi attivi (null = nessuna azienda o nessun gestore)
        private async Task<Contesto?> ContestoAsync()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                return null;

            var attivi = await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.Active).Select(x => x.ApplicationType).ToListAsync();
            var handlers = _handlers.Where(h => h.Application == ApplicationType.Common || attivi.Contains(h.Application)).ToList();
            if (handlers.Count == 0)
                return null;

            var idAz = await _az_AnagraficaRepository.FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            if (idAz == null)
                return null;

            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            if (user == null)
                return null;
            var ruoli = await _userManager.GetRolesAsync(user);

            var ctx = new Contesto
            {
                IdAz_Anagrafica = idAz.Value,
                IdAspNetUsers = this.CurrentUserId,
                Amministratore = ruoli.Any(r => CompanyRecipients.RuoliAmministratori.Contains(r)),
            };
            foreach (var h in handlers)
            {
                StrategiaApprovazione strategia;
                try
                {
                    strategia = await h.StrategiaAsync();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Richieste: strategia non disponibile per {Application}", h.Application);
                    continue;
                }
                ctx.Strategie[h] = strategia;
                foreach (var t in h.Tipi)
                    ctx.Handlers.TryAdd(t, h);
            }
            return ctx.Handlers.Count == 0 ? null : ctx;
        }
    }

    public interface IRequestService : IServiceBase
    {
        Task<GenericResult<RequestCountOutModel>> RequestCount(GenericRequest<RequestCountInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestListOutModel>> RequestList(GenericRequest<RequestListInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestApprove(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestReject(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestCancel(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
    }
}
