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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService
{

    public class Dip_ProfiloOrarioService : ServiceBase, IDip_ProfiloOrarioService
    {
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Dip_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,

                                  IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrario_Get_OutModel>> Dip_ProfiloOrarioGet(GenericRequest<Dip_ProfiloOrario_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_ProfiloOrario_Get_OutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entity = _dip_ProfiloOrarioRepository.GetAll().Where( x=> x.IdDip_RapportoLavoro == model.Data.Id).ToList();
                    retVal.Dip_ProfiloOrario = _mapper.Map<List<Dip_ProfiloOrarioModel>>(entity);
                }
                await Task.Delay(DelayAsyncMethod);
                
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrario_Put_OutModel>> Dip_ProfiloOrarioPut(GenericRequest<Dip_ProfiloOrario_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_ProfiloOrario_Put_OutModel();
                //var entity = _mapper.Map<Dip_ProfiloOrario>(model.Data.Dip_ProfiloOrario);

                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    // sostituisce il valore negativo
                    var newRow = model.Data.Dip_ProfiloOrario.Where(x=> x.Id<0).ToList();
                    foreach(var item in newRow)
                        item.Id = 0;
                        

                    
                    //rileggo i dati originali
                    var reqOrig_Data = new GenericRequest<Dip_ProfiloOrario_Get_InModel>();
                    reqOrig_Data.Data.Id = model.Data.Id;

                    var Orig_Data = await Dip_ProfiloOrarioGet(reqOrig_Data, true);
                    if (Orig_Data.Success && Orig_Data.Data != null)
                    {
                        //cancellazione record eliminati
                        foreach (var item in Orig_Data.Data.Dip_ProfiloOrario)
                        {   

                            //ottengo il record orig del db
                            var origRec = _dip_ProfiloOrarioRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();

                            if (origRec != null)
                            {
                                 //cerco la Par_ProfiloOrarioGG nei dati tornati dal client
                                 var orig_TMP = model.Data.Dip_ProfiloOrario.Where(x => x.Id == item.Id).FirstOrDefault();

                                //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                                if (orig_TMP == null)
                                {
                                    //procedo alla cancelazione
                                    await _dip_ProfiloOrarioRepository.DeleteAsync(origRec);
                                }
                            }
                        }
                        
                        //upsert 
                        foreach (var item in model.Data.Dip_ProfiloOrario)
                        {
                            //ottengo il record orig del db
                            var par_ProfiloOrarioGG = _dip_ProfiloOrarioRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                            if (par_ProfiloOrarioGG == null)
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_ProfiloOrario>(item);
                                par_ProfiloOrarioGG.IdDip_RapportoLavoro = model.Data.Id;
                                par_ProfiloOrarioGG.Id = 0;
                            }
                            else
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_ProfiloOrario>(item);
                            }
                            par_ProfiloOrarioGG = await _dip_ProfiloOrarioRepository.UpsertAsync(par_ProfiloOrarioGG);
                        }


                        //rileggo i dati dopo le varizioni per ritornare il valore corrente
                        Orig_Data = await Dip_ProfiloOrarioGet(reqOrig_Data, true);    
                        if (Orig_Data.Success && Orig_Data.Data != null)
                        {
                            retVal.Dip_ProfiloOrario = Orig_Data.Data.Dip_ProfiloOrario;
                        }


                    }

                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }


    }

    public interface IDip_ProfiloOrarioService : IServiceBase
    {
        Task<GenericResult<Dip_ProfiloOrario_Get_OutModel>> Dip_ProfiloOrarioGet(GenericRequest<Dip_ProfiloOrario_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_ProfiloOrario_Put_OutModel>> Dip_ProfiloOrarioPut(GenericRequest<Dip_ProfiloOrario_Put_InModel> model, bool isSubProcess);
    }
}
