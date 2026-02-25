using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService
{

    public class Par_ProfiloOrarioGGService : ServiceBase, IPar_ProfiloOrarioGGService
    {
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_ProfiloOrarioGGService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                  IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
        }




        public virtual async Task<GenericResult<Par_ProfiloOrarioGG_Get_4Edit_OutModel>> Par_ProfiloOrarioGG_Get(GenericRequest<Par_ProfiloOrarioGG_Get_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrarioGG_Get_4Edit_OutModel retVal = new Par_ProfiloOrarioGG_Get_4Edit_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_ProfiloOrario = _par_ProfiloOrarioRepository.FindById(model.Data.Id);
                    if (par_ProfiloOrario != null)
                    {
                        var par_ProfiloOrarioGG = _par_ProfiloOrarioGGRepository.GetAll().Where(x => x.IdPar_ProfiloOrario == model.Data.Id).ToList();
                        retVal.Par_ProfiloOrarioGG = _mapper.Map<List<Par_ProfiloOrarioGGModel>>(par_ProfiloOrarioGG);

                        for(var i=1; i<= par_ProfiloOrario.NumGiorniCiclo; i++)
                        {
                            if (!retVal.Par_ProfiloOrarioGG.Any(x => x.NumGiorno == i))
                            {
                                retVal.Par_ProfiloOrarioGG.Add(new Par_ProfiloOrarioGGModel()
                                {
                                    Id = 0,
                                    IdPar_ProfiloOrario  = model.Data.Id,
                                    NumGiorno  = i,
                                });
                            }
                        }
                    }
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloOrarioGG_Put_4Edit_OutModel>> Par_ProfiloOrarioGG_Put(GenericRequest<Par_ProfiloOrarioGG_Put_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrarioGG_Put_4Edit_OutModel retVal = new Par_ProfiloOrarioGG_Put_4Edit_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    //rileggo i dati originali
                    var reqPar_ProfiloOrarioGG = new GenericRequest<Par_ProfiloOrarioGG_Get_4Edit_InModel>();
                    reqPar_ProfiloOrarioGG.Data.Id = model.Data.Id;

                    var resAz_Sub = await Par_ProfiloOrarioGG_Get(reqPar_ProfiloOrarioGG, true);
                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        //cancellazione sub Par_ProfiloOrarioGG eliminate
                        foreach (var item in resAz_Sub.Data.Par_ProfiloOrarioGG)
                        {   //ciclo le commesse originali

                            //ottengo il record orig del db
                            Par_ProfiloOrarioGG? az_SubCommessa = _par_ProfiloOrarioGGRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();

                            if (az_SubCommessa != null)
                            {
                                //cerco la Par_ProfiloOrarioGG nei dati tornati dal client
                                var orig_TMP = model.Data.Par_ProfiloOrarioGG.Where(x => x.Id == item.Id).FirstOrDefault();

                                //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                                if (orig_TMP == null)
                                {
                                    //procedo alla cancelazione
                                    await _par_ProfiloOrarioGGRepository.DeleteAsync(az_SubCommessa);
                                }
                            }
                        }

                        //upsert Par_ProfiloOrarioGG
                        foreach (var item in model.Data.Par_ProfiloOrarioGG)
                        {
                            //ottengo il record orig del db
                            Par_ProfiloOrarioGG? par_ProfiloOrarioGG = _par_ProfiloOrarioGGRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                            if (par_ProfiloOrarioGG == null)
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Par_ProfiloOrarioGG>(item);
                                par_ProfiloOrarioGG.IdPar_ProfiloOrario = model.Data.Id;
                                par_ProfiloOrarioGG.Id = 0;
                            }
                            else
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Par_ProfiloOrarioGG>(item);
                            }
                            par_ProfiloOrarioGG = await _par_ProfiloOrarioGGRepository.UpsertAsync(par_ProfiloOrarioGG);
                        }

                        //rileggo i dati dopo le varizioni per ritornare il valore corrente
                        resAz_Sub = await Par_ProfiloOrarioGG_Get(reqPar_ProfiloOrarioGG, true);
                        if (resAz_Sub.Success && resAz_Sub.Data != null)
                        {
                            retVal.Par_ProfiloOrarioGG = resAz_Sub.Data.Par_ProfiloOrarioGG;
                        }
                    }
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_ProfiloOrarioGGService : IServiceBase
    {
        public Task<GenericResult<Par_ProfiloOrarioGG_Get_4Edit_OutModel>> Par_ProfiloOrarioGG_Get(GenericRequest<Par_ProfiloOrarioGG_Get_4Edit_InModel> model, bool isSubProcess);
        public Task<GenericResult<Par_ProfiloOrarioGG_Put_4Edit_OutModel>> Par_ProfiloOrarioGG_Put(GenericRequest<Par_ProfiloOrarioGG_Put_4Edit_InModel> model, bool isSubProcess);
    }
}
