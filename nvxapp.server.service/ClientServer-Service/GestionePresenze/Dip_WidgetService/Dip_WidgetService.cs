using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService
{
    /*
     Dati dei widget della dashboard delle presenze (ANALISI_ATTENDANCETRACKING_WIDGET.md).
       - Presenti oggi (CompanyPowerAdmin, CompanyAdmin): dipendenti (ruolo User) in servizio oggi; il CompanyAdmin vede
         solo quelli dei reparti che amministra (come report ed export). Presente = l'ultima timbratura valida di oggi
         non e' un'uscita (stessa regola della pagina "Personale presente"); tra gli assenti si contano a parte quelli
         con un giustificativo di giornata valido.
       - Richieste: da approvare e in attesa da RichiestaCount (stesse regole dei contatori del menu), piu' le approvate
         del mese corrente (dei dipendenti dell'azienda per gli amministratori, proprie per l'utente).
    */
    public class Dip_WidgetService : ServiceBase, IDip_WidgetService
    {
        // fasi valide: inserite direttamente o approvate
        private static readonly StatoRichiesta[] StatiValidi = { StatoRichiesta.Diretta, StatoRichiesta.Approvata };

        private readonly IAccountService _accountService;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;

        public Dip_WidgetService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IAccountService accountService,
                                 IDip_GG_RichiestaService dip_GG_RichiestaService,
                                 IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                 IDip_AnagraficaRepository dip_AnagraficaRepository,
                                 IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                 IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                 IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                 IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository,
                                 IDip_GG_RichiestaRepository dip_GG_RichiestaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
        }

        public virtual async Task<GenericResult<Widget_PresenzeOggiGetOutModel>> Widget_PresenzeOggiGet(GenericRequest<Widget_PresenzeOggiGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                bool powerAdmin = await HaRuolo("CompanyPowerAdmin");
                if (!powerAdmin && !await HaRuolo("CompanyAdmin"))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("WidgetAdminsOnly"));

                DateTime oggi = DateTime.Today;
                var retVal = new Widget_PresenzeOggiGetOutModel { Data = oggi };

                // dipendenti dell'azienda (ruolo User); il CompanyAdmin solo quelli dei reparti che amministra
                var utenti = (await DipendentiAzienda()).ToHashSet();
                if (!powerAdmin)
                    utenti.IntersectWith(DipendentiAmministrati(this.CurrentUserId, oggi));

                // rapporti di lavoro in servizio oggi
                var idAnagrafica = _dip_AnagraficaRepository.FindAll(a => a.IdAspNetUsers != null && utenti.Contains(a.IdAspNetUsers) && a.Attivo
                                                                          && (a.DataCessazione == null || a.DataCessazione >= oggi))
                                                            .Select(a => a.Id).ToList();
                var rapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafica.Contains(r.IdDip_Anagrafica)
                                                                          && (r.DataAss == null || r.DataAss <= oggi)
                                                                          && (r.DataLic == null || r.DataLic >= oggi))
                                                            .Select(r => new { r.Id, r.IdDip_Anagrafica })
                                                            .ToList();
                // un dipendente conta una volta anche con piu' rapporti
                var idRapporti = rapporti.Select(r => r.Id).ToList();
                var anagraficaDi = rapporti.ToDictionary(r => r.Id, r => r.IdDip_Anagrafica);
                retVal.Totale = rapporti.Select(r => r.IdDip_Anagrafica).Distinct().Count();

                // ultima timbratura valida di oggi per rapporto: presente se non e' un'uscita
                var timbrature = _dip_GG_TimbraturaRepository.FindAll(t => idRapporti.Contains(t.IdDip_RapportoLavoro)
                                                                           && t.GiornoCompetenza >= oggi && t.GiornoCompetenza < oggi.AddDays(1)
                                                                           && StatiValidi.Contains(t.RichiestaStato))
                                                             .Select(t => new { t.IdDip_RapportoLavoro, t.TimbraturaOriginale, t.TimbraturaTipo })
                                                             .ToList();
                var presenti = timbrature.GroupBy(t => t.IdDip_RapportoLavoro)
                                         .Where(g => g.OrderByDescending(t => t.TimbraturaOriginale).First().TimbraturaTipo != TipoTimbratura.Uscita)
                                         .Select(g => anagraficaDi[g.Key])
                                         .ToHashSet();
                retVal.Presenti = presenti.Count;
                retVal.Assenti = retVal.Totale - retVal.Presenti;

                // assenti con un giustificativo di giornata valido
                var giustificati = _dip_GG_GiustificativiRepository.FindAll(g => idRapporti.Contains(g.IdDip_RapportoLavoro)
                                                                                 && g.Data >= oggi && g.Data < oggi.AddDays(1)
                                                                                 && g.InputType == JustificationInputType.AllDay
                                                                                 && StatiValidi.Contains(g.RichiestaStato))
                                                                   .Select(g => g.IdDip_RapportoLavoro)
                                                                   .ToList()
                                                                   .Select(id => anagraficaDi[id])
                                                                   .Where(a => !presenti.Contains(a))
                                                                   .Distinct()
                                                                   .Count();
                retVal.AssentiGiustificati = giustificati;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Widget_RichiesteGetOutModel>> Widget_RichiesteGet(GenericRequest<Widget_RichiesteGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                bool admin = await HaRuolo("CompanyPowerAdmin") || await HaRuolo("CompanyAdmin");
                var retVal = new Widget_RichiesteGetOutModel { Admin = admin };

                // da approvare e in attesa: stesse regole dei contatori del menu "Richieste"
                var req = new GenericRequest<Dip_GG_Richiesta_Count_InModel>();
                req.Data.Admin = admin;
                var conteggi = await _dip_GG_RichiestaService.RichiestaCount(req, true);
                if (conteggi.Success && conteggi.Data != null)
                {
                    retVal.DaApprovare = conteggi.Data.DaApprovare;
                    retVal.InAttesa = conteggi.Data.InAttesa;
                }

                // approvate del mese corrente (giorno della richiesta nel mese)
                List<int> idRapporti;
                if (admin)
                {
                    var utenti = await DipendentiAzienda();
                    var idAnagrafica = _dip_AnagraficaRepository.FindAll(a => a.IdAspNetUsers != null && utenti.Contains(a.IdAspNetUsers)).Select(a => a.Id).ToList();
                    idRapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafica.Contains(r.IdDip_Anagrafica)).Select(r => r.Id).ToList();
                }
                else
                {
                    var propri = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, false);
                    idRapporti = propri?.dip_RapportoLavoro != null ? new List<int> { propri.dip_RapportoLavoro.Id } : new List<int>();
                }

                DateTime inizioMese = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime inizioMeseSucc = inizioMese.AddMonths(1);
                retVal.ApprovateMese = _dip_GG_RichiestaRepository.FindAll(r => idRapporti.Contains(r.IdDip_RapportoLavoro)
                                                                               && r.Data >= inizioMese && r.Data < inizioMeseSucc
                                                                               && r.RevocaStato == null
                                                                               && r.RichiestaStato == StatoRichiesta.Approvata)
                                                                  .Count();

                return retVal;
            }, isSubProcess);
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

    public interface IDip_WidgetService : IServiceBase
    {
        Task<GenericResult<Widget_PresenzeOggiGetOutModel>> Widget_PresenzeOggiGet(GenericRequest<Widget_PresenzeOggiGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Widget_RichiesteGetOutModel>> Widget_RichiesteGet(GenericRequest<Widget_RichiesteGetInModel> model, Boolean isSubProcess);
    }
}
