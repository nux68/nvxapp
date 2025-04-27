using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{

    public class Dip_GG_RichiestaService : ServiceBase, IDip_GG_RichiestaService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IDip_GG_GiustificativiRepository _Dip_GG_GiustificativiRepository;

        public Dip_GG_RichiestaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_RichiestaRepository dip_GG_RichiestaRepository,
                                  IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                  IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _Dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Richiesta_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_GetAll_OutModel retVal = new Dip_GG_Richiesta_GetAll_OutModel();

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {


                    //TODO qui non va bene , trovare criterio per periodo cavallo mese
                    List<Dip_GG_Richiesta> richiesta = _dip_GG_RichiestaRepository.FindAll(x => x.Data.Year == model.Data.Year &&
                                                                                            x.Data.Month == model.Data.Month &&
                                                                                            x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                              .OrderBy(x => x.Data).ToList();

                    retVal.Dip_GG_RichiestaModel = _mapper.Map<List<Dip_GG_RichiestaModel>>(richiesta);
                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_Send_OutModel retVal = new Dip_GG_Richiesta_Send_OutModel();

                if (false)
                {
                    User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, true);

                    if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                    {
                        Dip_GG_Richiesta dip_GG_Richiesta = _mapper.Map<Dip_GG_Richiesta>(model.Data.Dip_GG_RichiestaModel);
                        dip_GG_Richiesta.IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id;
                        dip_GG_Richiesta = await _dip_GG_RichiestaRepository.UpsertAsync(dip_GG_Richiesta);

                        switch (dip_GG_Richiesta.RichiestaTipo)
                        {
                            case TipoRichiesta.Timbratura:
                                await Add_Dip_GG_Timbratura(dip_GG_Richiesta,user_DATA_COMB_DipAna_DipRapp);
                                break;

                            case TipoRichiesta.Giustificativo:
                                await Add_Dip_GG_Giustificativi(dip_GG_Richiesta, user_DATA_COMB_DipAna_DipRapp);

                                break;
                            case TipoRichiesta.NotaSpesa:
                                break;
                        }

                    }
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        private async Task Add_Dip_GG_Timbratura(Dip_GG_Richiesta dip_GG_Richiesta,
                                                 User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {

                if (dip_GG_Richiesta.Dati != null)
                {
                    Dip_GG_Richiesta_Body_Timbratura richiesta = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_Timbratura>(dip_GG_Richiesta.Dati);

                    if(richiesta!=null)
                    {
                        string fullDateTime = $"{dip_GG_Richiesta.Data.ToString("dd/MM/yyyy")} {richiesta.hhmm}";

                        // Fai il parsing della stringa completa
                        DateTime parsedDateTime = DateTime.ParseExact(fullDateTime, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        Dip_GG_Timbratura dip_GG_Timbratura = new Dip_GG_Timbratura()
                        {
                            IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                            IdDip_Richiesta = dip_GG_Richiesta.Id,
                            Timbratura = parsedDateTime,
                            TimbraturaOriginale = parsedDateTime,
                            TimbraturaArrotondata = parsedDateTime,
                            GiornoCompetenza = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day),
                            TimbraturaTipo = TipoTimbratura.SenzaVerso,
                            RichiestaStato = StatoRichiesta.Immessa,
                        };
                        await _dip_GG_TimbraturaRepository.UpsertAsyncGuid(dip_GG_Timbratura);
                    }
                   
                }
            }
        }

        private async Task Add_Dip_GG_Giustificativi(Dip_GG_Richiesta dip_GG_Richiesta,
                                                     User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {
                Dip_GG_Giustificativi dip_GG_Giustificativi = new Dip_GG_Giustificativi()
                {
                    IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                    IdDip_Richiesta = dip_GG_Richiesta.Id,
                    IdPar_Giustificativi = 0
                };
                await _Dip_GG_GiustificativiRepository.UpsertAsyncGuid(dip_GG_Giustificativi);
            }
        }

    }

    public interface IDip_GG_RichiestaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Richiesta_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Richiesta_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess);
    }
}
