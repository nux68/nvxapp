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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService
{

    public class Dip_RapportoLavoroService : ServiceBase, IDip_RapportoLavoroService
    {
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Dip_RapportoLavoroService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IConfiguration configuration,

                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Dip_RapportoLavoro_Get_OutModel>> Dip_RapportoLavoroGet(GenericRequest<Dip_RapportoLavoro_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_RapportoLavoro_Get_OutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entity = _dip_RapportoLavoroRepository.GetAll().Where( x=> x.IdDip_Anagrafica == model.Data.Id).ToList();
                    retVal.Dip_RapportoLavoro = _mapper.Map<List<Dip_RapportoLavoroModel>>(entity);
                }
                await Task.Delay(DelayAsyncMethod);
                
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_RapportoLavoro_Put_OutModel>> Dip_RapportoLavoroPut(GenericRequest<Dip_RapportoLavoro_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_RapportoLavoro_Put_OutModel();
                //var entity = _mapper.Map<Dip_RapportoLavoro>(model.Data.Dip_RapportoLavoro);

                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    
                    //rileggo i dati originali
                    var reqOrig_Data = new GenericRequest<Dip_RapportoLavoro_Get_InModel>();
                    reqOrig_Data.Data.Id = model.Data.Id;

                    var Orig_Data = await Dip_RapportoLavoroGet(reqOrig_Data, true);
                    if (Orig_Data.Success && Orig_Data.Data != null)
                    {
                        //cancellazione record eliminati
                        foreach (var item in Orig_Data.Data.Dip_RapportoLavoro)
                        {   

                            //ottengo il record orig del db
                            var origRec = _dip_RapportoLavoroRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();

                            if (origRec != null)
                            {
                                 //cerco la Par_ProfiloOrarioGG nei dati tornati dal client
                                 var orig_TMP = model.Data.Dip_RapportoLavoro.Where(x => x.Id == item.Id).FirstOrDefault();

                                //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                                if (orig_TMP == null)
                                {
                                    //procedo alla cancelazione
                                    await _dip_RapportoLavoroRepository.DeleteAsync(origRec);
                                }
                            }
                        }
                        
                        //upsert 
                        foreach (var item in model.Data.Dip_RapportoLavoro)
                        {
                            //ottengo il record orig del db
                            var par_ProfiloOrarioGG = _dip_RapportoLavoroRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                            if (par_ProfiloOrarioGG == null)
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_RapportoLavoro>(item);
                                par_ProfiloOrarioGG.IdDip_Anagrafica = model.Data.Id;
                                par_ProfiloOrarioGG.Id = 0;
                            }
                            else
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_RapportoLavoro>(item);
                            }
                            par_ProfiloOrarioGG = await _dip_RapportoLavoroRepository.UpsertAsync(par_ProfiloOrarioGG);
                        }


                        //rileggo i dati dopo le varizioni per ritornare il valore corrente
                        Orig_Data = await Dip_RapportoLavoroGet(reqOrig_Data, true);    
                    }

                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }

    }

    public interface IDip_RapportoLavoroService : IServiceBase
    {
        
        Task<GenericResult<Dip_RapportoLavoro_Get_OutModel>> Dip_RapportoLavoroGet(GenericRequest<Dip_RapportoLavoro_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_RapportoLavoro_Put_OutModel>> Dip_RapportoLavoroPut(GenericRequest<Dip_RapportoLavoro_Put_InModel> model, bool isSubProcess);
        
    
    }
}
