using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService
{
    /*
     Cartellino di quadratura mensile (Dip_Periodo): lettura per il dipendente e per l'HR,
     approvazione / riapertura del mese e ore in piu' riconosciute.
     I totali li scrive il motore di calcolo; qui si cambia solo lo stato e il tetto delle ore riconosciute
     (che fa ripartire il calcolo del mese).
    */
    public class Dip_PeriodoService : ServiceBase, IDip_PeriodoService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_PeriodoRepository _dip_PeriodoRepository;
        private readonly IDip_GG_ResultRepository _dip_GG_ResultRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;

        public Dip_PeriodoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_PeriodoRepository dip_PeriodoRepository,
                                  IDip_GG_ResultRepository dip_GG_ResultRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService
                                  ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_PeriodoRepository = dip_PeriodoRepository;
            _dip_GG_ResultRepository = dip_GG_ResultRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        public virtual async Task<GenericResult<Dip_Periodo_Get_OutModel>> Dip_Periodo_Get(GenericRequest<Dip_Periodo_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Periodo_Get_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;
                var userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, false);
                var rapporto = userData?.dip_RapportoLavoro;
                if (rapporto == null)
                    return retVal;

                int anno = model.Data.Anno, mese = model.Data.Mese;
                var inizioMese = new DateTime(anno, mese, 1);
                var fineMese = inizioMese.AddMonths(1).AddDays(-1);

                retVal.TipoContratto = rapporto.TipoContratto;
                retVal.OreSettimanaliContratto = rapporto.OreSettimanaliContratto;
                retVal.DestinazioneEccedenze = rapporto.DestinazioneEccedenze;

                var profilo = ProfiloAl(rapporto.Id, fineMese);
                retVal.TipoConteggio = profilo?.TipoConteggio ?? StraoTipoConteggio.Giornaliero;
                retVal.EccedenzeAttive = profilo?.EccedenzeAttive ?? false;

                var periodo = _dip_PeriodoRepository.FindAll(p => p.IdDip_RapportoLavoro == rapporto.Id && p.Anno == anno && p.Mese == mese).FirstOrDefault();
                if (periodo != null)
                    retVal.Dip_Periodo = _mapper.Map<Dip_PeriodoModel>(periodo);

                var giorni = _dip_GG_ResultRepository.FindAll(r => r.IdDip_RapportoLavoro == rapporto.Id && r.Data >= inizioMese && r.Data <= fineMese)
                                                     .ToList()
                                                     .Where(r => NelRapporto(rapporto, r.Data))
                                                     .OrderBy(r => r.Data)
                                                     .ToList();

                retVal.Min_Saldo = giorni.Where(r => r.Data.Date < DateTime.Today).Sum(r => r.Min_Ecc);

                retVal.Giorni = giorni.Where(r => r.Min_Ecc > 0 || r.Min_NonAutorizzate > 0)
                                      .Select(r => new Dip_Periodo_Giorno
                                      {
                                          Data = r.Data,
                                          Festivo = r.Festivo,
                                          Bloccato = r.Stato.HasFlag(GG_ResultStato.Locked),
                                          EccProvvisoria = r.EccProvvisoria,
                                          BancaOre = r.BancaOre,
                                          Min_Teo = (int)r.HH_Teo.ToTimeSpan().TotalMinutes,
                                          Min_Lav = (int)r.HH_Lav.ToTimeSpan().TotalMinutes,
                                          Min_Ecc = r.Min_Ecc,
                                          Min_Suppl = r.Min_Suppl,
                                          Min_SupplEcc = r.Min_SupplEcc,
                                          Min_Strao = r.Min_Strao,
                                          Min_StraoNott = r.Min_StraoNott,
                                          Min_StraoFest = r.Min_StraoFest,
                                          Min_StraoFestNott = r.Min_StraoFestNott,
                                          Min_EccOrd = r.Min_EccOrd,
                                          Min_NonRiconosciute = r.Min_NonRiconosciute,
                                          Min_NonAutorizzate = r.Min_NonAutorizzate,
                                          Min_DaAutorizzare = r.Min_DaAutorizzare
                                      })
                                      .ToList();

                retVal.Settimane = giorni.GroupBy(r => GestionePresenzeEccedenze.InizioSettimana(r.Data))
                                         .OrderBy(g => g.Key)
                                         .Select(g => new Dip_Periodo_Settimana
                                         {
                                             Dal = g.Key < inizioMese ? inizioMese : g.Key,
                                             Al = g.Key.AddDays(6) > fineMese ? fineMese : g.Key.AddDays(6),
                                             Provvisoria = g.Any(r => r.EccProvvisoria),
                                             Min_Teo = g.Sum(r => (int)r.HH_Teo.ToTimeSpan().TotalMinutes),
                                             Min_Lav = g.Sum(r => (int)r.HH_Lav.ToTimeSpan().TotalMinutes),
                                             Min_Ecc = g.Sum(r => Math.Max(0, r.Min_Ecc)),
                                             Min_Qualificate = g.Sum(r => r.Min_Suppl + r.Min_SupplEcc + r.Min_Strao + r.Min_StraoNott + r.Min_StraoFest + r.Min_StraoFestNott + r.Min_EccOrd)
                                         })
                                         .ToList();

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Periodo_GetAll_OutModel>> Dip_Periodo_GetAll(GenericRequest<Dip_Periodo_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Periodo_GetAll_OutModel();

                var anagrafiche = await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(model.Data.UsersId);
                var idAnagrafiche = anagrafiche.Select(a => a.Id).ToList();

                var inizioMese = new DateTime(model.Data.Anno, model.Data.Mese, 1);
                var fineMese = inizioMese.AddMonths(1).AddDays(-1);

                // rapporti attivi nel mese
                var rapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafiche.Contains(r.IdDip_Anagrafica) &&
                                                                         r.DataAss <= fineMese &&
                                                                         (r.DataLic == null || r.DataLic >= inizioMese))
                                                            .ToList();
                var idRapporti = rapporti.Select(r => r.Id).ToList();
                var periodi = _dip_PeriodoRepository.FindAll(p => idRapporti.Contains(p.IdDip_RapportoLavoro) && p.Anno == model.Data.Anno && p.Mese == model.Data.Mese)
                                                    .ToList();

                foreach (var rapporto in rapporti)
                {
                    var ana = anagrafiche.First(a => a.Id == rapporto.IdDip_Anagrafica);
                    var periodo = periodi.FirstOrDefault(p => p.IdDip_RapportoLavoro == rapporto.Id);
                    retVal.Righe.Add(new Dip_Periodo_Riga
                    {
                        IdAspNetUsers = ana.IdAspNetUsers,
                        Cognome = ana.Cognome,
                        Nome = ana.Nome,
                        UserName = ana.UserName,
                        TipoContratto = rapporto.TipoContratto,
                        DestinazioneEccedenze = rapporto.DestinazioneEccedenze,
                        Dip_Periodo = periodo != null ? _mapper.Map<Dip_PeriodoModel>(periodo) : null
                    });
                }

                retVal.Righe = retVal.Righe.OrderBy(r => r.Cognome).ThenBy(r => r.Nome).ToList();

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Periodo_SetStato_OutModel>> Dip_Periodo_SetStato(GenericRequest<Dip_Periodo_SetStato_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Periodo_SetStato_OutModel();

                foreach (var id in model.Data.Id)
                {
                    var periodo = await _dip_PeriodoRepository.FindByIdAsync(id);
                    if (periodo == null)
                        continue;

                    var etichetta = $"{periodo.Mese:00}/{periodo.Anno}";

                    if (model.Data.Stato == PeriodoStato.Approvato)
                    {
                        if (periodo.Stato == PeriodoStato.Approvato)
                            continue;
                        if (periodo.Stato != PeriodoStato.DaApprovare)
                        {
                            retVal.Messages.Add(new Message($"Il cartellino {etichetta} e' ancora provvisorio: si approva a periodo concluso, dopo il ricalcolo.", MessageType.Warning));
                            continue;
                        }

                        periodo.Stato = PeriodoStato.Approvato;
                        periodo.DataApprovazione = DateTime.Now;
                        periodo.IdAspNetUsers_Approvazione = this.CurrentUserId;
                        await BloccaGiorni(periodo, true);
                    }
                    else
                    {
                        // riapertura: i giorni tornano modificabili e ricalcolabili
                        if (periodo.Stato != PeriodoStato.Approvato)
                            continue;

                        periodo.Stato = PeriodoStato.DaApprovare;
                        periodo.DataApprovazione = null;
                        periodo.IdAspNetUsers_Approvazione = null;
                        await BloccaGiorni(periodo, false);
                    }

                    periodo = await _dip_PeriodoRepository.UpsertAsync(periodo);
                    retVal.Dip_Periodo.Add(_mapper.Map<Dip_PeriodoModel>(periodo));
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Periodo_SetRiconosciute_OutModel>> Dip_Periodo_SetRiconosciute(GenericRequest<Dip_Periodo_SetRiconosciute_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Periodo_SetRiconosciute_OutModel();

                var periodo = await _dip_PeriodoRepository.FindByIdAsync(model.Data.Id);
                if (periodo == null)
                    return retVal;

                if (periodo.Stato == PeriodoStato.Approvato)
                {
                    retVal.Messages.Add(new Message("Il cartellino e' approvato: va riaperto prima di cambiare le ore riconosciute.", MessageType.Warning));
                    retVal.Dip_Periodo = _mapper.Map<Dip_PeriodoModel>(periodo);
                    return retVal;
                }

                periodo.Min_Riconosciute = model.Data.Min_Riconosciute.HasValue ? Math.Max(0, model.Data.Min_Riconosciute.Value) : null;
                periodo.Note = model.Data.Note;
                periodo = await _dip_PeriodoRepository.UpsertAsync(periodo);
                retVal.Dip_Periodo = _mapper.Map<Dip_PeriodoModel>(periodo);

                // ricalcolo del mese (in background: al termine arriva la notifica di fine calcolo)
                var rapporto = _dip_RapportoLavoroRepository.FindAll(r => r.Id == periodo.IdDip_RapportoLavoro).FirstOrDefault();
                var anagrafica = rapporto != null ? _dip_AnagraficaRepository.FindAll(a => a.Id == rapporto.IdDip_Anagrafica).FirstOrDefault() : null;
                if (anagrafica != null)
                {
                    var req = new GenericRequest<TimeSheet_CalculateInModel>();
                    req.Data.TimeSheet_Calculate.Year = periodo.Anno;
                    req.Data.TimeSheet_Calculate.Month = periodo.Mese;
                    req.Data.TimeSheet_Calculate.Dal = new DateTime(periodo.Anno, periodo.Mese, 1);
                    req.Data.TimeSheet_Calculate.Al = new DateTime(periodo.Anno, periodo.Mese, 1).AddMonths(1).AddDays(-1);
                    req.Data.TimeSheet_Calculate.SelectedUserId = new List<string> { anagrafica.IdAspNetUsers };
                    await _timeSheet_EngineService.Calculate(req, true);
                }

                return retVal;
            }, isSubProcess);
        }

        private async Task BloccaGiorni(Dip_Periodo periodo, bool blocca)
        {
            var inizioMese = new DateTime(periodo.Anno, periodo.Mese, 1);
            var fineMese = inizioMese.AddMonths(1).AddDays(-1);
            var giorni = _dip_GG_ResultRepository.FindAll(r => r.IdDip_RapportoLavoro == periodo.IdDip_RapportoLavoro && r.Data >= inizioMese && r.Data <= fineMese).ToList();
            foreach (var g in giorni)
            {
                var stato = blocca ? g.Stato | GG_ResultStato.Locked : g.Stato & ~GG_ResultStato.Locked;
                if (stato == g.Stato)
                    continue;
                g.Stato = stato;
                await _dip_GG_ResultRepository.UpsertAsync(g);
            }
        }

        private Par_ProfiloOrario? ProfiloAl(int idRapporto, DateTime giorno)
        {
            var dipProfilo = _dip_ProfiloOrarioRepository.FindAll(p => p.IdDip_RapportoLavoro == idRapporto && p.Dal <= giorno && p.Al >= giorno && p.IdPar_ProfiloOrario != null)
                                                        .FirstOrDefault()
                             ?? _dip_ProfiloOrarioRepository.FindAll(p => p.IdDip_RapportoLavoro == idRapporto && p.IdPar_ProfiloOrario != null)
                                                           .OrderByDescending(p => p.Dal)
                                                           .FirstOrDefault();
            if (dipProfilo == null)
                return null;
            return _par_ProfiloOrarioRepository.FindAll(p => p.Id == dipProfilo.IdPar_ProfiloOrario).FirstOrDefault();
        }

        private static bool NelRapporto(Dip_RapportoLavoro rapporto, DateTime giorno)
        {
            return (rapporto.DataAss == null || giorno.Date >= rapporto.DataAss.Value.Date) &&
                   (rapporto.DataLic == null || giorno.Date <= rapporto.DataLic.Value.Date);
        }
    }

    public interface IDip_PeriodoService : IServiceBase
    {
        Task<GenericResult<Dip_Periodo_Get_OutModel>> Dip_Periodo_Get(GenericRequest<Dip_Periodo_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Periodo_GetAll_OutModel>> Dip_Periodo_GetAll(GenericRequest<Dip_Periodo_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Periodo_SetStato_OutModel>> Dip_Periodo_SetStato(GenericRequest<Dip_Periodo_SetStato_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_Periodo_SetRiconosciute_OutModel>> Dip_Periodo_SetRiconosciute(GenericRequest<Dip_Periodo_SetRiconosciute_InModel> model, bool isSubProcess);
    }
}
