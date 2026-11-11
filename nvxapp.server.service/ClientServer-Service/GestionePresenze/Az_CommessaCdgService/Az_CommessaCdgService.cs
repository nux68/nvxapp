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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService
{
    /*
     Controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md, AT-CDG.1-3): ore del timesheet
     per sottocommessa (stesse regole di Statistiche attivita', mese per mese), costo orario e ricavi impostati nella
     pagina (non salvati, CDG1-2), note spese approvate ripartite in proporzione alle ore (CDG3). Calcolo in CdgCalcolo.
     Riservato al CompanyPowerAdmin (CDG7).
    */
    public class Az_CommessaCdgService : ServiceBase, IAz_CommessaCdgService
    {
        public const int MaxMesi = 12;
        private static readonly StatoRichiesta[] StatiValidi = { StatoRichiesta.Diretta, StatoRichiesta.Approvata };

        private readonly IAccountService _accountService;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IActivityStatisticsService _activityStatisticsService;
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;

        public Az_CommessaCdgService(IMapper mapper,
                                     UserManager<ApplicationUser> userManager,
                                     IAspNetUsersRepository aspNetUsersRepository,
                                     IOptions<JwtParameter> jwtParameter,
                                     IHttpContextAccessor httpContextAccessor,
                                     IConfiguration configuration,
                                     IAccountService accountService,
                                     ICommonCompanyUtility commonCompanyUtility,
                                     IActivityStatisticsService activityStatisticsService,
                                     IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService,
                                     IDip_AnagraficaRepository dip_AnagraficaRepository,
                                     IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                     IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _commonCompanyUtility = commonCompanyUtility;
            _activityStatisticsService = activityStatisticsService;
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
        }

        public virtual async Task<GenericResult<Az_CommessaCdg_GetOutModel>> CdgGet(GenericRequest<Az_CommessaCdg_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloPowerAdmin();
                return new Az_CommessaCdg_GetOutModel { Risultato = await Calcola(model.Data.Parametri) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Widget_OreCommesseGetOutModel>> Widget_OreCommesseGet(GenericRequest<Widget_OreCommesseGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloPowerAdmin();
                var oggi = DateTime.Today;
                var p = new CdgParametri { Year = oggi.Year, Month = oggi.Month, YearAl = oggi.Year, MonthAl = oggi.Month, CostoOrarioPredefinito = model.Data.CostoOrarioPredefinito };
                var r = await Calcola(p);
                return new Widget_OreCommesseGetOutModel
                {
                    Year = oggi.Year,
                    Month = oggi.Month,
                    MinutiTotali = r.Totale.Minuti,
                    CostoTotale = model.Data.CostoOrarioPredefinito > 0 ? r.Totale.Costo : null,
                    Commesse = r.Commesse.OrderByDescending(c => c.Minuti).Take(5)
                                .Select(c => new CdgCommessaModel
                                {
                                    IdAz_Commessa = c.IdAz_Commessa,
                                    Descrizione = c.Descrizione,
                                    Cliente = c.Cliente,
                                    Minuti = c.Minuti,
                                    CostoPersonale = c.CostoPersonale,
                                    NoteSpese = c.NoteSpese,
                                    Costo = c.Costo,
                                })
                                .ToList(),
                };
            }, isSubProcess);
        }

        /// <summary>calcolo per la pagina e per il report PDF (job)</summary>
        public async Task<CdgRisultato> Calcola(CdgParametri p)
        {
            if (p.Month < 1 || p.Month > 12 || p.MonthAl < 1 || p.MonthAl > 12 || p.Year < 2000 || p.YearAl < 2000)
                throw new UserMessageException(AttendanceTrackingMessages.Get("Cdg_PeriodoNonValido", MaxMesi));
            var dal = new DateTime(p.Year, p.Month, 1);
            var inizioUltimo = new DateTime(p.YearAl, p.MonthAl, 1);
            int mesi = (inizioUltimo.Year - dal.Year) * 12 + inizioUltimo.Month - dal.Month + 1;
            if (mesi < 1 || mesi > MaxMesi)
                throw new UserMessageException(AttendanceTrackingMessages.Get("Cdg_PeriodoNonValido", MaxMesi));
            var al = inizioUltimo.AddMonths(1).AddDays(-1);
            if (p.CostoOrarioPredefinito < 0 || p.CostiDipendenti.Any(c => c.CostoOrario < 0))
                throw new UserMessageException(AttendanceTrackingMessages.Get("Cdg_CostoNonValido"));

            // dipendenti dell'azienda (ruolo User)
            var utenti = await DipendentiAzienda();
            var az = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
            var anagrafiche = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == az.Id && a.IdAspNetUsers != null && utenti.Contains(a.IdAspNetUsers))
                                                       .Select(a => new { a.Id, a.IdAspNetUsers, a.Cognome, a.Nome })
                                                       .ToList();
            var nomi = anagrafiche.GroupBy(a => a.IdAspNetUsers!).ToDictionary(g => g.Key, g => GestionePresenzeAnomalie.NomeDipendente(g.First().Cognome, g.First().Nome));

            // sottocommesse dell'azienda; attivita' -> sottocommessa (per la sottocommessa predefinita del rapporto)
            var resFull = await _az_SubCommessaAttivitaService.GetAll_4FullList(new GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel>(), true);
            var attivita = resFull.Success && resFull.Data != null ? resFull.Data.Az_SubCommessaAttivita : new List<Az_SubCommessaAttivita_4FullListModel>();
            var subCommesse = attivita.GroupBy(a => a.SubCommessa_Id)
                                      .ToDictionary(g => g.Key, g => new CdgCalcolo.SubCommessa(g.Key, g.First().SubCommessa_Decrizione, g.First().Commessa_Id,
                                                                                                  g.First().Commessa_Decrizione, g.First().Cliente_Descrizione));
            var subDiAttivita = attivita.GroupBy(a => a.SubCommessaAttivita_Id).ToDictionary(g => g.Key, g => g.First().SubCommessa_Id);

            // ore del timesheet, mese per mese
            var ore = new List<CdgCalcolo.Ore>();
            int escluse = 0;
            if (utenti.Count > 0)
            {
                for (var mese = dal; mese <= inizioUltimo; mese = mese.AddMonths(1))
                {
                    var req = new GenericRequest<ActivityStatistics_GetInModel>();
                    req.Data.ActivityStatistics = new ActivityStatisticsModel { Year = mese.Year, Month = mese.Month, SelectedUserId = utenti };
                    var res = await _activityStatisticsService.ActivityStatisticsGet(req, true);
                    if (!res.Success || res.Data == null)
                        continue;
                    escluse += res.Data.GiornateEscluse.Count;
                    ore.AddRange(res.Data.RighePerDipendente.Select(r => new CdgCalcolo.Ore(r.UserId, r.NomeDipendente, r.IdAz_SubCommessa, mese.Year, mese.Month, r.TotaleMinuti)));
                }
            }

            // note spese approvate (o inserite direttamente) del periodo
            var idAnagrafiche = anagrafiche.Select(a => a.Id).ToList();
            var utenteDi = anagrafiche.ToDictionary(a => a.Id, a => a.IdAspNetUsers!);
            var rapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafiche.Contains(r.IdDip_Anagrafica))
                                                        .Select(r => new { r.Id, r.IdDip_Anagrafica, r.IdAz_SubCommessaAttivita })
                                                        .ToList()
                                                        .ToDictionary(r => r.Id);
            var idRapporti = rapporti.Keys.ToList();
            var dopo = al.AddDays(1);
            var note = _dip_GG_NotaSpesaRepository.FindAll(n => idRapporti.Contains(n.IdDip_RapportoLavoro) && StatiValidi.Contains(n.RichiestaStato)
                                                                && n.Data >= dal && n.Data < dopo)
                                                  .Select(n => new { n.IdDip_RapportoLavoro, n.Data, n.Importo })
                                                  .ToList()
                                                  .Select(n =>
                                                  {
                                                      var r = rapporti[n.IdDip_RapportoLavoro];
                                                      int? sub = subDiAttivita.TryGetValue(r.IdAz_SubCommessaAttivita, out var s) ? s : null;
                                                      return new CdgCalcolo.Nota(utenteDi[r.IdDip_Anagrafica], n.Data.Year, n.Data.Month, sub, n.Importo);
                                                  })
                                                  .ToList();

            return CdgCalcolo.Calcola(ore, note, subCommesse, nomi, p, dal, al, escluse);
        }

        private async Task SoloPowerAdmin()
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "CompanyPowerAdmin"))
                throw new UserMessageException(AttendanceTrackingMessages.Get("Cdg_SoloPowerAdmin"));
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
    }

    public interface IAz_CommessaCdgService : IServiceBase
    {
        Task<GenericResult<Az_CommessaCdg_GetOutModel>> CdgGet(GenericRequest<Az_CommessaCdg_GetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Widget_OreCommesseGetOutModel>> Widget_OreCommesseGet(GenericRequest<Widget_OreCommesseGetInModel> model, Boolean isSubProcess);
        Task<CdgRisultato> Calcola(CdgParametri p);
    }
}
