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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;


namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService
{

    public class Par_OrarioIntervalloHHService : ServiceBase, IPar_OrarioIntervalloHHService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_OrarioIntervalloHHService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_OrarioRepository par_OrarioRepository,
                                  IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_OrarioRepository = par_OrarioRepository;
        }

        public virtual async Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll(GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHHOutModel retVal = new Par_OrarioIntervalloHHOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<Par_OrarioIntervalloHH_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Par_OrarioIntervalloHH_GetAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHH_GetAll_4Edit_OutModel retVal = new Par_OrarioIntervalloHH_GetAll_4Edit_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    var par_Orario =_par_OrarioRepository.FindById(model.Data.Id);
                    if( par_Orario != null )
                    {
                        var par_OrarioIntervalloHH = _par_OrarioIntervalloHHRepository.FindAll(x=>x.IdPar_Orario == model.Data.Id).ToList();
                        retVal.Par_OrarioIntervalloHH = _mapper.Map<List<Par_OrarioIntervalloHHModel>>(par_OrarioIntervalloHH);

                        for(var i=1; i<= par_Orario.NumeroCoppie; i++)
                        {
                            if (!retVal.Par_OrarioIntervalloHH.Any(x => x.NumCoppia == i))
                            {
                                retVal.Par_OrarioIntervalloHH.Add(new Par_OrarioIntervalloHHModel()
                                {
                                    Id = 0,
                                    IdPar_Orario = model.Data.Id,
                                    NumCoppia = i,
                                });
                            }
                        }
                        retVal.Par_OrarioIntervalloHH = retVal.Par_OrarioIntervalloHH.OrderBy(x => x.NumCoppia).ToList();
                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_OrarioIntervalloHH_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Par_OrarioIntervalloHH_PutAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHH_PutAll_4Edit_OutModel retVal = new Par_OrarioIntervalloHH_PutAll_4Edit_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_Orario =_par_OrarioRepository.FindById(model.Data.Id);
                    if( par_Orario != null )
                    {
                        retVal.Id = model.Data.Id;

                        var par_OrarioIntervalloHH_All = _par_OrarioIntervalloHHRepository.FindAll(x=>x.IdPar_Orario == model.Data.Id).ToList();
                        //cancellazione sub commesse eliminate
                        foreach (var item in par_OrarioIntervalloHH_All)
                        {
                            //cerco il record nei dati tornati dal client
                            var orig_TMP = model.Data.Par_OrarioIntervalloHH.Where(x => x.Id == item.Id).FirstOrDefault();
                            //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                            if (orig_TMP == null)
                            {
                                //procedo alla cancelazione
                                await _par_OrarioIntervalloHHRepository.DeleteAsync(item);
                            }
                        }

                        foreach (var item in model.Data.Par_OrarioIntervalloHH)
                        {
                            var    par_OrarioIntervalloHH = _mapper.Map<Par_OrarioIntervalloHH>(item);
                            par_OrarioIntervalloHH.IdPar_Orario = model.Data.Id;
                            await _par_OrarioIntervalloHHRepository.UpsertAsync(par_OrarioIntervalloHH);
                        }

                        //rireggo gli i dati
                        var req_1 = new GenericRequest<Par_OrarioIntervalloHH_GetAll_4Edit_InModel>();
                        req_1.Data =  new Par_OrarioIntervalloHH_GetAll_4Edit_InModel(){ Id = model.Data.Id };
                        var res_1 = await GetAll_4Edit(req_1,true);
                        if (res_1.Success && res_1.Data != null)
                        {
                            retVal.Par_OrarioIntervalloHH = res_1.Data.Par_OrarioIntervalloHH;
                        }

                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
         
    }

    public interface IPar_OrarioIntervalloHHService : IServiceBase
    {
        public Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll( GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess);


        Task<GenericResult<Par_OrarioIntervalloHH_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Par_OrarioIntervalloHH_GetAll_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_OrarioIntervalloHH_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Par_OrarioIntervalloHH_PutAll_4Edit_InModel> model, bool isSubProcess);

    }
}
