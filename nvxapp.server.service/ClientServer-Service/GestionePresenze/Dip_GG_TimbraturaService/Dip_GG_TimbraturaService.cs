using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService
{

    public class Dip_GG_TimbraturaService : ServiceBase, IDip_GG_TimbraturaService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        //private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        //private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;


        public Dip_GG_TimbraturaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  //IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  //IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository
                                  ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            //_dip_AnagraficaRepository = dip_AnagraficaRepository;
            //_dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;

        }

        public virtual async Task<GenericResult<Dip_GG_Timbratura_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Timbratura_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Timbratura_GetAll_OutModel retVal = new Dip_GG_Timbratura_GetAll_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    List<Dip_GG_Timbratura> timbratura = _dip_GG_TimbraturaRepository.FindAll(x => x.GiornoCompetenza.Year == model.Data.Year &&
                                                                                              x.GiornoCompetenza.Month == model.Data.Month &&
                                                                                              x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                                     .OrderBy(x => x.TimbraturaOriginale).ToList();

                    var groupedByDay = timbratura.GroupBy(x => x.GiornoCompetenza.Date).ToList();

                    foreach (var group in groupedByDay)
                    {
                        for (int i = 0; i < group.Count(); i++)
                        {
                            var timbraturaItem = group.ElementAt(i);

                            // Applica la logica solo se il TipoTimbratura è diverso da Attivita
                            if (timbraturaItem.TimbraturaTipo != TipoTimbratura.Attivita)
                            {
                                timbraturaItem.TimbraturaTipo = (i % 2 == 0) ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;
                            }
                        }
                    }

                    retVal.Dip_GG_Timbratura = _mapper.Map<List<Dip_GG_TimbraturaModel>>(timbratura);

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Timbratura_Stamp_OutModel>> Stamp(GenericRequest<Dip_GG_Timbratura_Stamp_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Timbratura_Stamp_OutModel retVal = new Dip_GG_Timbratura_Stamp_OutModel();

                // Il client manda ISO 8601 senza offset, es. "2026-03-12T10:28:00"
                // DateTimeOffset.Parse lo interpreta come ora locale del server → corretto
                DateTime finalDate = DateTimeOffset.Parse(
                    model.Data.DateStamp,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeLocal
                ).LocalDateTime;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Timbratura dip_GG_Timbratura = new Dip_GG_Timbratura()
                    {
                        IdDip_RapportoLavoro  = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                        Timbratura            = finalDate,
                        TimbraturaOriginale   = finalDate,
                        TimbraturaArrotondata = finalDate,
                        GiornoCompetenza      = finalDate,
                        TimbraturaTipo        = TipoTimbratura.SenzaVerso,
                        RichiestaStato        = StatoRichiesta.Diretta,
                    };
                    await _dip_GG_TimbraturaRepository.UpsertAsync(dip_GG_Timbratura);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDip_GG_TimbraturaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Timbratura_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Timbratura_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Timbratura_Stamp_OutModel>> Stamp(GenericRequest<Dip_GG_Timbratura_Stamp_InModel> model, Boolean isSubProcess);
    }


}