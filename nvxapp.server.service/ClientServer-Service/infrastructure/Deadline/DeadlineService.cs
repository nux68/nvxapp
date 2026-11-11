using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline
{
    /*
     Pagina "Scadenzario" e widget "In scadenza" dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.3).
        - CompanyPowerAdmin e CompanyAdmin vedono e gestiscono tutte le scadenze dell'azienda
        - gli altri utenti dell'azienda vedono solo quelle di cui sono responsabili
        - le scadenze di un applicativo non attivo per l'azienda non si mostrano
        - le scadenze dei moduli si possono chiudere e riaprire; quelle inserite a mano anche modificare e cancellare
          (le altre le aggiorna il modulo quando cambia l'oggetto)
    */
    public class DeadlineService : ServiceBase, IDeadlineService
    {
        public const string FonteManuale = "manuale";

        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;

        public DeadlineService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,

                               IScadenzaRepository scadenzaRepository,
                               ICompanyApplicationRepository companyApplicationRepository,
                               IUserCompanyRepository userCompanyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _scadenzaRepository = scadenzaRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _userCompanyRepository = userCompanyRepository;
        }

        public virtual async Task<GenericResult<DeadlineListOutModel>> DeadlineList(GenericRequest<DeadlineListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlineListOutModel();
                var (idCompany, admin) = await Contesto();
                retVal.Amministratore = admin;

                var filtro = model.Data ?? new DeadlineListInModel();
                var query = await Visibili(idCompany, admin || false, filtro.SoloMie || !admin);
                if (filtro.Stato != null)
                    query = query.Where(x => x.Stato == (StatoScadenza)filtro.Stato.Value);
                if (filtro.EntroGiorni != null)
                {
                    var entro = DateTime.Today.AddDays(filtro.EntroGiorni.Value);
                    query = query.Where(x => x.DataScadenza <= entro);
                }
                if (!string.IsNullOrWhiteSpace(filtro.Testo))
                {
                    var testo = "%" + filtro.Testo.Trim() + "%";
                    query = query.Where(x => EF.Functions.ILike(x.Descrizione, testo) || (x.Note != null && EF.Functions.ILike(x.Note, testo)));
                }

                retVal.Deadline = await Modelli(query.OrderBy(x => x.Stato).ThenBy(x => x.DataScadenza).Take(1000));
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeadlineGetOutModel>> DeadlineGet(GenericRequest<DeadlineGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlineGetOutModel();
                var (idCompany, admin) = await Contesto();

                if (model.Data.Id == 0)
                {
                    if (!admin)
                        throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotAllowed"));
                    retVal.Deadline = new DeadlineModel { Manuale = true, Fonte = FonteManuale, DataScadenza = DateTime.Today.AddMonths(1), GiorniPreavviso = 30 };
                }
                else
                    retVal.Deadline = (await Modelli((await Visibili(idCompany, admin, !admin)).Where(x => x.Id == model.Data.Id))).FirstOrDefault()
                                      ?? throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotFound"));

                if (admin)
                    retVal.Utenti = await UtentiAzienda(idCompany);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeadlinePutOutModel>> DeadlinePut(GenericRequest<DeadlinePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlinePutOutModel();
                var (idCompany, admin) = await Contesto();
                if (!admin)
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotAllowed"));

                var d = model.Data;
                if (string.IsNullOrWhiteSpace(d.Descrizione))
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineDescriptionRequired"));
                if (d.DataScadenza == default)
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineDateRequired"));
                if (!string.IsNullOrEmpty(d.IdAspNetUsersResponsabile)
                    && !await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && x.IdAspNetUsers == d.IdAspNetUsersResponsabile && !x.Disabled).AnyAsync())
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineUserInvalid"));

                Scadenza riga;
                if (d.Id == 0)
                    riga = new Scadenza { IdCompany = idCompany, Application = (int)ApplicationType.Common, Fonte = FonteManuale };
                else
                {
                    riga = await _scadenzaRepository.FindAll(x => x.Id == d.Id && x.IdCompany == idCompany).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotFound"));
                    if (!Manuale(riga))
                        throw new UserMessageException(InfrastructureMessages.Get("DeadlineManualOnly"));
                    if (riga.DataScadenza.Date != d.DataScadenza.Date)
                    {
                        riga.Stato = StatoScadenza.Aperta;
                        riga.DataChiusura = null;
                        riga.DataAvvisoPreavviso = null;
                        riga.DataAvvisoScaduta = null;
                    }
                }
                riga.Descrizione = DeadlineRegistry.Taglia(d.Descrizione.Trim(), 200)!;
                riga.Note = string.IsNullOrWhiteSpace(d.Note) ? null : DeadlineRegistry.Taglia(d.Note.Trim(), 500);
                riga.DataScadenza = d.DataScadenza.Date;
                riga.GiorniPreavviso = Math.Clamp(d.GiorniPreavviso, 0, 365);
                riga.IdAspNetUsersResponsabile = string.IsNullOrEmpty(d.IdAspNetUsersResponsabile) ? null : d.IdAspNetUsersResponsabile;

                riga = d.Id == 0 ? await _scadenzaRepository.CreateAsync(riga) : await _scadenzaRepository.UpdateAsync(riga);
                retVal.Deadline = (await Modelli(_scadenzaRepository.FindAll(x => x.Id == riga.Id))).First();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeadlineCloseOutModel>> DeadlineClose(GenericRequest<DeadlineCloseInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlineCloseOutModel();
                var (idCompany, admin) = await Contesto();

                // chiude il responsabile o un amministratore
                var riga = await (await Visibili(idCompany, admin, !admin)).Where(x => x.Id == model.Data.Id).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotFound"));
                if (model.Data.Chiudi)
                {
                    riga.Stato = StatoScadenza.Chiusa;
                    riga.DataChiusura = DateTime.Now;
                }
                else
                {
                    riga.Stato = StatoScadenza.Aperta;
                    riga.DataChiusura = null;
                }
                await _scadenzaRepository.UpdateAsync(riga);
                retVal.Deadline = (await Modelli(_scadenzaRepository.FindAll(x => x.Id == riga.Id))).First();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeadlineDeleteOutModel>> DeadlineDelete(GenericRequest<DeadlineDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlineDeleteOutModel();
                var (idCompany, admin) = await Contesto();
                if (!admin)
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotAllowed"));

                var riga = await _scadenzaRepository.FindAll(x => x.Id == model.Data.Id && x.IdCompany == idCompany).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotFound"));
                if (!Manuale(riga))
                    throw new UserMessageException(InfrastructureMessages.Get("DeadlineManualOnly"));
                await _scadenzaRepository.DeleteAsync(riga);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeadlineSummaryOutModel>> DeadlineSummary(GenericRequest<DeadlineSummaryInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DeadlineSummaryOutModel();
                var (idCompany, admin) = await Contesto();
                int giorni = Math.Clamp(model.Data?.Giorni ?? 30, 1, 365);
                int massimo = Math.Clamp(model.Data?.Massimo ?? 5, 1, 20);

                var oggi = DateTime.Today;
                var entro = oggi.AddDays(giorni);
                var aperte = (await Visibili(idCompany, admin, !admin)).Where(x => x.Stato == StatoScadenza.Aperta);
                retVal.Scadute = await aperte.CountAsync(x => x.DataScadenza < oggi);
                retVal.InScadenza = await aperte.CountAsync(x => x.DataScadenza >= oggi && x.DataScadenza <= entro);
                retVal.Prossime = await Modelli(aperte.Where(x => x.DataScadenza <= entro).OrderBy(x => x.DataScadenza).Take(massimo));
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        // azienda corrente (obbligatoria) e se l'utente e' un suo amministratore
        private async Task<(int IdCompany, bool Admin)> Contesto()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("DeadlineNoCompany"));
            var user = await _userManager.FindByIdAsync(this.CurrentUserId)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("DeadlineNotAllowed"));
            return (idCompany, await AmministratoreAsync(_userManager, user));
        }

        private Task<IQueryable<Scadenza>> Visibili(int idCompany, bool admin, bool soloMie)
            => VisibiliPerUtente(_scadenzaRepository, _companyApplicationRepository, idCompany, this.CurrentUserId, admin, soloMie);

        // Regole di visibilita', usate anche dal feed del calendario (DeadlineCalendarEventProvider, INF-CAL.2)
        // dove l'utente non e' quello collegato.

        /// <summary>l'utente vede e gestisce tutte le scadenze dell'azienda (CompanyPowerAdmin, CompanyAdmin)</summary>
        public static async Task<bool> AmministratoreAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            var ruoli = await userManager.GetRolesAsync(user);
            return ruoli.Any(r => r == nameof(RoleCode.CompanyPowerAdmin) || r == nameof(RoleCode.CompanyAdmin));
        }

        /// <summary>scadenze dell'azienda degli applicativi attivi (e delle funzioni base); solo quelle dell'utente se non e' amministratore o se richiesto</summary>
        public static async Task<IQueryable<Scadenza>> VisibiliPerUtente(IScadenzaRepository scadenzaRepository,
                                                                         ICompanyApplicationRepository companyApplicationRepository,
                                                                         int idCompany, string idAspNetUsers, bool admin, bool soloMie)
        {
            var attivi = (await companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.Active)
                                                            .Select(x => x.ApplicationType).ToListAsync())
                         .Select(x => (int)x).Append((int)ApplicationType.Common).ToList();
            var query = scadenzaRepository.FindAll(x => x.IdCompany == idCompany && attivi.Contains(x.Application));
            if (soloMie || !admin)
                query = query.Where(x => x.IdAspNetUsersResponsabile == idAspNetUsers);
            return query;
        }

        private async Task<List<DeadlineModel>> Modelli(IQueryable<Scadenza> query)
        {
            var righe = await query.ToListAsync();
            var idUtenti = righe.Where(x => x.IdAspNetUsersResponsabile != null).Select(x => x.IdAspNetUsersResponsabile!).Distinct().ToList();
            var nomi = await _aspNetUsersRepository.FindAll(x => idUtenti.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? x.Id);
            var oggi = DateTime.Today;
            return righe.Select(x => new DeadlineModel
            {
                Id = x.Id,
                Application = x.Application,
                Fonte = x.Fonte,
                IdRiferimento = x.IdRiferimento,
                Descrizione = x.Descrizione,
                Note = x.Note,
                DataScadenza = x.DataScadenza,
                GiorniPreavviso = x.GiorniPreavviso,
                IdAspNetUsersResponsabile = x.IdAspNetUsersResponsabile,
                Responsabile = x.IdAspNetUsersResponsabile != null && nomi.TryGetValue(x.IdAspNetUsersResponsabile, out var n) ? n : null,
                Url = x.Url,
                UrlState = x.UrlState,
                Stato = (int)x.Stato,
                DataChiusura = x.DataChiusura,
                Manuale = Manuale(x),
                Scaduta = x.Stato == StatoScadenza.Aperta && x.DataScadenza < oggi,
                InScadenza = x.Stato == StatoScadenza.Aperta && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(x.GiorniPreavviso)
            }).ToList();
        }

        private async Task<List<DeadlineUserModel>> UtentiAzienda(int idCompany)
        {
            var utenti = await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            return await _aspNetUsersRepository.FindAll(x => utenti.Contains(x.Id))
                                               .OrderBy(x => x.UserName)
                                               .Select(x => new DeadlineUserModel { Id = x.Id, Nome = x.UserName ?? x.Email ?? x.Id })
                                               .ToListAsync();
        }

        private static bool Manuale(Scadenza x) => x.Application == (int)ApplicationType.Common && x.Fonte == FonteManuale;
    }

    public interface IDeadlineService : IServiceBase
    {
        Task<GenericResult<DeadlineListOutModel>> DeadlineList(GenericRequest<DeadlineListInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeadlineGetOutModel>> DeadlineGet(GenericRequest<DeadlineGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeadlinePutOutModel>> DeadlinePut(GenericRequest<DeadlinePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeadlineCloseOutModel>> DeadlineClose(GenericRequest<DeadlineCloseInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeadlineDeleteOutModel>> DeadlineDelete(GenericRequest<DeadlineDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeadlineSummaryOutModel>> DeadlineSummary(GenericRequest<DeadlineSummaryInModel> model, Boolean isSubProcess);
    }
}
