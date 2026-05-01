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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService
{
    public class ActivityStatisticsService : ServiceBase, IActivityStatisticsService
    {

        private readonly ITimeSheet_EngineService _timeSheet_EngineService;
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;

        public ActivityStatisticsService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,
                                         ITimeSheet_EngineService timeSheet_EngineService,
                                         IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService

                                         ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {

            _timeSheet_EngineService          = timeSheet_EngineService;
            _az_SubCommessaAttivitaService    = az_SubCommessaAttivitaService;
        }

        public virtual async Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal  = new ActivityStatistics_GetOutModel();
                var filters = model.Data.ActivityStatistics;

                DateTime dal = new DateTime(filters.Year, filters.Month, 1);
                DateTime al  = new DateTime(filters.Year, filters.Month, DateTime.DaysInMonth(filters.Year, filters.Month));

                // 1. Tutti i dati timbratura + profili orario per il periodo
                var req_AllData = new GenericRequest<Timesheet_AllData_InModel>();
                req_AllData.Data.Dal     = dal;
                req_AllData.Data.Al      = al;
                req_AllData.Data.UsersId = filters.SelectedUserId;
                var res_AllData = await _timeSheet_EngineService.Get_Timesheet_AllData(req_AllData, true);
                if (!res_AllData.Success || res_AllData.Data == null)
                    return retVal;

                var allData       = res_AllData.Data;
                var timbratureAll = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura;
                var risultatiGG   = allData.Dip_GG_AllData_OutModel.Dip_GG_Result;
                var daySlots      = allData.OrariSchema_4User_OutModel.DaySlots;
                var parOrari      = allData.OrariSchema_4User_OutModel.ParOrario;
                var dipAnagrafica = allData.OrariSchema_4User_OutModel.Dip_Anagrafica;

                // 2. Anagrafica completa attività ? commessa, sub-commessa, cliente
                var req4FullList = new GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel>();
                var res4FullList = await _az_SubCommessaAttivitaService.GetAll_4FullList(req4FullList, true);
                var attivitaLookup = (res4FullList.Success && res4FullList.Data != null)
                    ? res4FullList.Data.Az_SubCommessaAttivita.ToDictionary(a => a.SubCommessaAttivita_Id)
                    : new Dictionary<int, Az_SubCommessaAttivita_4FullListModel>();

                // 3. Dizionari di supporto
                var anagraficaByUserId = dipAnagrafica.ToDictionary(a => a.IdAspNetUsers);

                var risultatiDict = risultatiGG
                    .ToDictionary(r => (r.IdDip_RapportoLavoro, r.Data.Date));

                var parOrarioById = parOrari.ToDictionary(o => o.Id);

                var timbraturePerGiorno = timbratureAll
                    .GroupBy(t => (t.IdDip_RapportoLavoro, t.GiornoCompetenza.Date))
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(t => t.TimbraturaArrotondata ?? t.Timbratura).ToList());

                // Accumulatore: (userId, idAttivita) ? minuti totali
                var accumulator = new Dictionary<(string userId, int idAttivita), int>();

                // 4. Ciclo sui DaySlot
                foreach (var slot in daySlots)
                {
                    var dayKey = (slot.IdDip_RapportoLavoro, slot.Data.Date);
                    var userId = slot.IdAspNetUsers;

                    if (!anagraficaByUserId.TryGetValue(userId, out var anagrafica))
                        continue;

                    var nomeDip = $"{anagrafica.Cognome} {anagrafica.Nome}".Trim();

                    // Filtro stato giornata: solo giornate elaborate correttamente
                    if (!risultatiDict.TryGetValue(dayKey, out var ggResult))
                        continue;

                    var hasErr  = (ggResult.Stato & GG_ResultStato.Err)  != 0;
                    var hasInit = (ggResult.Stato & GG_ResultStato.STATE_MASK) == GG_ResultStato.Init;
                    if (hasErr || hasInit)
                    {
                        retVal.GiornateEscluse.Add(new ActivityStatistics_GiornataEsclusaModel
                        {
                            UserId         = userId,
                            NomeDipendente = nomeDip,
                            Data           = slot.Data,
                            Stato          = ggResult.Stato
                        });
                        continue;
                    }

                    // Recupero Par_Orario per questa giornata (ZOrder 1 = base)
                    var orarioEntry = slot.Orari.OrderBy(o => o.ZOrder).FirstOrDefault();
                    if (orarioEntry == null) continue;
                    if (!parOrarioById.TryGetValue(orarioEntry.IdPar_Orario, out var parOrario))
                        continue;

                    if (!timbraturePerGiorno.TryGetValue(dayKey, out var timbrature) || timbrature.Count == 0)
                        continue;

                    if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                    {
                        // Segmenti consecutivi: ogni record non-Uscita apre un segmento verso il record successivo.
                        // Il cambio attività si rileva da IdAz_SubCommessaAttivita diverso tra record consecutivi.
                        for (int i = 0; i < timbrature.Count - 1; i++)
                        {
                            var curr = timbrature[i];
                            var next = timbrature[i + 1];

                            // Un'Uscita non può aprire un segmento
                            if (curr.TimbraturaTipo == TipoTimbratura.Uscita)
                                continue;

                            var t1     = curr.TimbraturaArrotondata ?? curr.Timbratura;
                            var t2     = next.TimbraturaArrotondata ?? next.Timbratura;
                            var minuti = (int)(t2 - t1).TotalMinutes;
                            if (minuti <= 0) continue;

                            AggiungiMinuti(accumulator, userId, curr.IdAz_SubCommessaAttivita,
                                           minuti, filters, attivitaLookup);
                        }
                    }
                    else // MonteOre / MonteOreValore: il valore è nella parte oraria di Timbratura
                    {
                        foreach (var t in timbrature.Where(t => t.TimbraturaTipo == TipoTimbratura.SenzaVerso))
                        {
                            var minuti = (int)t.Timbratura.TimeOfDay.TotalMinutes;
                            if (minuti <= 0) continue;

                            AggiungiMinuti(accumulator, userId, t.IdAz_SubCommessaAttivita,
                                           minuti, filters, attivitaLookup);
                        }
                    }
                }

                // 5. Costruzione righe per dipendente dall'accumulatore
                foreach (var ((userId, idAttivita), minuti) in accumulator)
                {
                    if (!anagraficaByUserId.TryGetValue(userId, out var ana)) continue;
                    attivitaLookup.TryGetValue(idAttivita, out var attInfo);

                    retVal.RighePerDipendente.Add(new ActivityStatistics_RowModel
                    {
                        UserId                   = userId,
                        NomeDipendente           = $"{ana.Cognome} {ana.Nome}".Trim(),
                        IdAz_SubCommessaAttivita = idAttivita,
                        NomeAttivita             = attInfo?.SubCommessaAttivita_Par_Attivita_Descrizione ?? string.Empty,
                        IdAz_SubCommessa         = attInfo?.SubCommessa_Id ?? 0,
                        NomeSubCommessa          = attInfo?.SubCommessa_Decrizione ?? string.Empty,
                        IdAz_Commessa            = attInfo?.Commessa_Id ?? 0,
                        NomeCommessa             = attInfo?.Commessa_Decrizione ?? string.Empty,
                        IdCliente                = attInfo?.Commessa_IdAz_Cliente ?? 0,
                        NomeCliente              = attInfo?.Cliente_Descrizione ?? string.Empty,
                        TotaleMinuti             = minuti
                    });
                }

                // 6. Aggregazione totali per attività (tutti i dipendenti selezionati)
                retVal.TotaliPerAttivita = retVal.RighePerDipendente
                    .GroupBy(r => r.IdAz_SubCommessaAttivita)
                    .Select(g => new ActivityStatistics_TotaleAttivitaModel
                    {
                        IdAz_SubCommessaAttivita = g.Key,
                        NomeAttivita             = g.First().NomeAttivita,
                        IdAz_SubCommessa         = g.First().IdAz_SubCommessa,
                        NomeSubCommessa          = g.First().NomeSubCommessa,
                        IdAz_Commessa            = g.First().IdAz_Commessa,
                        NomeCommessa             = g.First().NomeCommessa,
                        IdCliente                = g.First().IdCliente,
                        NomeCliente              = g.First().NomeCliente,
                        TotaleMinuti             = g.Sum(r => r.TotaleMinuti)
                    })
                    .OrderBy(t => t.NomeCliente)
                    .ThenBy(t => t.NomeCommessa)
                    .ThenBy(t => t.NomeSubCommessa)
                    .ThenBy(t => t.NomeAttivita)
                    .ToList();

                await Task.Delay(0);
                return retVal;
            }, isSubProcess);
        }

        /// <summary>
        /// Aggiunge i minuti all'accumulatore applicando i filtri opzionali su attività/commessa/cliente.
        /// </summary>
        private static void AggiungiMinuti(
            Dictionary<(string userId, int idAttivita), int> accumulator,
            string userId,
            int idAttivita,
            int minuti,
            ActivityStatisticsModel filters,
            Dictionary<int, Az_SubCommessaAttivita_4FullListModel> attivitaLookup)
        {
            if (filters.IdsAttivita?.Count > 0 && !filters.IdsAttivita.Contains(idAttivita))
                return;

            if (filters.IdsSubCommessa?.Count > 0 || filters.IdsCommessa?.Count > 0 || filters.IdsCliente?.Count > 0)
            {
                if (!attivitaLookup.TryGetValue(idAttivita, out var attInfo)) return;
                if (filters.IdsSubCommessa?.Count > 0 && !filters.IdsSubCommessa.Contains(attInfo.SubCommessa_Id)) return;
                if (filters.IdsCommessa?.Count > 0  && !filters.IdsCommessa.Contains(attInfo.Commessa_Id)) return;
                if (filters.IdsCliente?.Count > 0   && !filters.IdsCliente.Contains(attInfo.Commessa_IdAz_Cliente)) return;
            }

            var key = (userId, idAttivita);
            accumulator[key] = accumulator.GetValueOrDefault(key) + minuti;
        }
    }

    public interface IActivityStatisticsService : IServiceBase
    {
        Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> model, bool isSubProcess);
    }
}
