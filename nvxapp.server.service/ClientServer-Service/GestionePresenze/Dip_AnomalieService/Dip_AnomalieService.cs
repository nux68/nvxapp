using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService
{
    /*
     Anomalie delle timbrature, giornate da sistemare prima dell'export paghe e previsione delle assenze
     (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md, AT-AI.1-4). Le regole sono in GestionePresenzeAnomalie, il testo AI
     in GestionePresenzeAnomalieWriter. Riservato a CompanyPowerAdmin (tutti i dipendenti) e CompanyAdmin (solo i
     dipendenti dei reparti che amministra, come report, widget ed export).
    */
    public class Dip_AnomalieService : ServiceBase, IDip_AnomalieService
    {
        public const int MaxGiorniPeriodo = 92;

        private readonly IAccountService _accountService;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IGestionePresenzeAnomalie _anomalie;
        private readonly IGestionePresenzeAnomalieWriter _writer;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;

        public Dip_AnomalieService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,
                                   IAccountService accountService,
                                   ICommonCompanyUtility commonCompanyUtility,
                                   IGestionePresenzeAnomalie anomalie,
                                   IGestionePresenzeAnomalieWriter writer,
                                   IDip_AnagraficaRepository dip_AnagraficaRepository,
                                   IAz_SediRepartoUserRepository az_SediRepartoUserRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _commonCompanyUtility = commonCompanyUtility;
            _anomalie = anomalie;
            _writer = writer;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
        }

        public virtual async Task<GenericResult<Dip_Anomalie_GetOutModel>> AnomalieGet(GenericRequest<Dip_Anomalie_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Anomalie_GetOutModel { Risultato = await Analizza(model.Data) };
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Anomalie_SpiegaOutModel>> AnomalieSpiega(GenericRequest<Dip_Anomalie_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var testo = await _writer.WriteAsync(await Analizza(model.Data));
                return new Dip_Anomalie_SpiegaOutModel { Testo = testo.Testo, GeneratoConAI = testo.GeneratoConAI, ProviderAI = testo.ProviderAI };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_PrevisioneAssenze_GetOutModel>> PrevisioneAssenzeGet(GenericRequest<Dip_PrevisioneAssenze_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var oggi = DateTime.Today;
                var dipendenti = await DipendentiVisibili(model.Data.UsersId, oggi);
                var giorni = model.Data.Giorni <= 0 ? 30 : model.Data.Giorni;
                return new Dip_PrevisioneAssenze_GetOutModel { Risultato = _anomalie.Prevedi(dipendenti, oggi, giorni) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Widget_GiornateDaSistemareGetOutModel>> Widget_GiornateDaSistemareGet(GenericRequest<Widget_GiornateDaSistemareGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (dal, al) = PeriodoWidget(DateTime.Today);
                var dipendenti = await DipendentiVisibili(new List<string>(), DateTime.Today);
                var r = _anomalie.Analizza(dipendenti, dal, al, DateTime.Today);
                return new Widget_GiornateDaSistemareGetOutModel
                {
                    Dal = dal,
                    Al = al,
                    DaSistemare = r.GiornateDaSistemare,
                    DaVerificare = r.GiornateDaVerificare,
                    Dipendenti = r.DipendentiCoinvolti,
                    Conteggi = r.Conteggi.Take(3).ToList(),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Widget_PrevisioneAssenzeGetOutModel>> Widget_PrevisioneAssenzeGet(GenericRequest<Widget_PrevisioneAssenzeGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var dipendenti = await DipendentiVisibili(new List<string>(), DateTime.Today);
                var r = _anomalie.Prevedi(dipendenti, DateTime.Today, 7);
                return new Widget_PrevisioneAssenzeGetOutModel
                {
                    MediaPreviste = r.MediaPreviste,
                    Pianificate = r.Giorni.Sum(g => g.Pianificate),
                    Giorni = r.Giorni,
                };
            }, isSubProcess);
        }

        /// <summary>
        /// periodo del widget: nei primi 10 giorni del mese (tempo di export paghe) il mese precedente fino a ieri,
        /// poi il mese corrente fino a ieri
        /// </summary>
        public static (DateTime Dal, DateTime Al) PeriodoWidget(DateTime oggi)
        {
            var inizioMese = new DateTime(oggi.Year, oggi.Month, 1);
            var dal = oggi.Day <= 10 ? inizioMese.AddMonths(-1) : inizioMese;
            return (dal, oggi.Date.AddDays(-1));
        }

        // ----------------------------------------------------------------------------------------

        private async Task<AnomalieRisultato> Analizza(Dip_Anomalie_GetInModel richiesta)
        {
            var dal = richiesta.Dal.Date;
            var al = richiesta.Al.Date;
            if (dal == DateTime.MinValue || al < dal || (al - dal).TotalDays >= MaxGiorniPeriodo)
                throw new UserMessageException(AttendanceTrackingMessages.Get("Anomalie_PeriodoNonValido", MaxGiorniPeriodo));

            var dipendenti = await DipendentiVisibili(richiesta.UsersId, al < DateTime.Today ? al : DateTime.Today);
            var risultato = _anomalie.Analizza(dipendenti, dal, al, DateTime.Today);
            if (richiesta.SoloDaSistemare)
                risultato.Giornate = risultato.Giornate.Where(g => g.DaSistemare).ToList();
            return risultato;
        }

        /// <summary>
        /// Dip_Anagrafica dell'azienda visibili all'utente: dipendenti (ruolo User) dell'azienda, per il CompanyAdmin solo
        /// quelli dei reparti che amministra alla data; se usersId non e' vuoto, solo quelli tra i selezionati
        /// </summary>
        public async Task<List<int>> DipendentiVisibili(List<string>? usersId, DateTime data)
        {
            bool powerAdmin = await HaRuolo("CompanyPowerAdmin");
            if (!powerAdmin && !await HaRuolo("CompanyAdmin"))
                throw new UserMessageException(AttendanceTrackingMessages.Get("Anomalie_SoloAmministratori"));

            var utenti = (await DipendentiAzienda()).ToHashSet();
            if (!powerAdmin)
                utenti.IntersectWith(DipendentiAmministrati(this.CurrentUserId, data));
            if (usersId != null && usersId.Count > 0)
                utenti.IntersectWith(usersId);

            var az = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
            var lista = utenti.ToList();
            return _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == az.Id && a.IdAspNetUsers != null && lista.Contains(a.IdAspNetUsers))
                                            .Select(a => a.Id)
                                            .ToList();
        }

        // utenti con ruolo User dell'azienda corrente
        private async Task<List<string>> DipendentiAzienda()
        {
            var req = new GenericRequest<UserCompanyListInModel>();
            req.Data.FilteredRoles = new List<string> { "User" };
            var res = await _accountService.UserCompanyList(req, true);
            return res.Success && res.Data?.UserCompanyList != null
                ? res.Data.UserCompanyList.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).Distinct().ToList()
                : new List<string>();
        }

        // dipendenti dei reparti in cui l'utente e' amministratore, alla data (come report ed export)
        private HashSet<string> DipendentiAmministrati(string idAspNetUsers, DateTime data)
        {
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.EnabledToAdmin)
                                                       .Select(x => x.IdAz_SediReparto).ToList();
            return _az_SediRepartoUserRepository.FindAll(x => reparti.Contains(x.IdAz_SediReparto) && x.UserInDepartment
                                                              && (x.DataDal == null || x.DataDal <= data)
                                                              && (x.DataAl == null || x.DataAl >= data))
                                                .Select(x => x.IdAspNetUsers).ToHashSet();
        }

        private async Task<bool> HaRuolo(string ruolo)
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            return user != null && await _userManager.IsInRoleAsync(user, ruolo);
        }
    }

    public interface IDip_AnomalieService : IServiceBase
    {
        Task<GenericResult<Dip_Anomalie_GetOutModel>> AnomalieGet(GenericRequest<Dip_Anomalie_GetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Dip_Anomalie_SpiegaOutModel>> AnomalieSpiega(GenericRequest<Dip_Anomalie_GetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Dip_PrevisioneAssenze_GetOutModel>> PrevisioneAssenzeGet(GenericRequest<Dip_PrevisioneAssenze_GetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Widget_GiornateDaSistemareGetOutModel>> Widget_GiornateDaSistemareGet(GenericRequest<Widget_GiornateDaSistemareGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Widget_PrevisioneAssenzeGetOutModel>> Widget_PrevisioneAssenzeGet(GenericRequest<Widget_PrevisioneAssenzeGetInModel> model, Boolean isSubProcess);
        Task<List<int>> DipendentiVisibili(List<string>? usersId, DateTime data);
    }
}
