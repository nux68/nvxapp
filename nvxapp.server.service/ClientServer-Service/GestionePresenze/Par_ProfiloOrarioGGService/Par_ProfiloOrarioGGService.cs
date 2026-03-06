using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService
{

    public class Par_ProfiloOrarioGGService : ServiceBase, IPar_ProfiloOrarioGGService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;
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

                                  IPar_OrarioRepository par_OrarioRepository,
                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                  IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioRepository = par_OrarioRepository;
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
                    int NumGiorniCiclo = 7; //default 
                    var par_ProfiloOrario = _par_ProfiloOrarioRepository.FindById(model.Data.Id);
                    if (par_ProfiloOrario != null)
                        NumGiorniCiclo = par_ProfiloOrario.NumGiorniCiclo;

                    var par_ProfiloOrarioGG = _par_ProfiloOrarioGGRepository.GetAll().Where(x => x.IdPar_ProfiloOrario == model.Data.Id).ToList();
                    retVal.Par_ProfiloOrarioGG = _mapper.Map<List<Par_ProfiloOrarioGGModel>>(par_ProfiloOrarioGG);

                    var par_Orario = _par_OrarioRepository.GetAll().FirstOrDefault();

                    int tmp_counter = 0;
                    for (var i = 1; i <= NumGiorniCiclo; i++)
                    {
                        if (!retVal.Par_ProfiloOrarioGG.Any(x => x.NumGiorno == i && x.ZOrder == 1))
                        {
                            retVal.Par_ProfiloOrarioGG.Add(new Par_ProfiloOrarioGGModel()
                            {
                                Id = --tmp_counter,
                                IdPar_ProfiloOrario = model.Data.Id,
                                NumGiorno = i,
                                ZOrder = 1,
                                IdPar_Orario = par_Orario != null ? par_Orario.Id : 0 //MIGLIORARE
                            });
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
                                par_ProfiloOrarioGG.ZOrder = item.ZOrder;
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
        public virtual async Task<GenericResult<Par_ProfiloOrarioGG_Arrange_NumDay_OutModel>> Par_ProfiloOrarioGG_Arrange_NumDay(GenericRequest<Par_ProfiloOrarioGG_Arrange_NumDay_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrarioGG_Arrange_NumDay_OutModel retVal = new Par_ProfiloOrarioGG_Arrange_NumDay_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var currentGG = model.Data.Par_ProfiloOrarioGG;

                    var uniqueDays = currentGG.Select(g => g.NumGiorno).Distinct().ToList();
                    int newValue = model.Data.NumGiorniCiclo;

                    int defaultIdParOrario = 0;
                    if (uniqueDays.Count < newValue)
                    {
                        // Aggiungi giorni mancanti
                        if (currentGG.Any())
                        {
                            defaultIdParOrario = currentGG.First().IdPar_Orario;
                        }
                        else
                        {
                            var firstOrario = await _par_OrarioRepository.GetAll().FirstOrDefaultAsync();
                            if (firstOrario != null)
                            {
                                defaultIdParOrario = firstOrario.Id;
                            }
                        }

                        int tmpCounter = currentGG.Any(g => g.Id < 0) ? currentGG.Where(g => g.Id < 0).Min(g => g.Id) : 0;
                        for (int i = 1; i <= newValue; i++)
                        {
                            if (!uniqueDays.Contains(i))
                            {
                                currentGG.Add(new Par_ProfiloOrarioGGModel
                                {
                                    Id = --tmpCounter, // ID temporaneo negativo
                                    IdPar_ProfiloOrario = model.Data.Id,
                                    NumGiorno = i,
                                    ZOrder = 1,
                                    IdPar_Orario = defaultIdParOrario
                                });
                            }
                        }
                    }
                    else if (uniqueDays.Count > newValue)
                    {
                        // Rimuovi giorni in eccesso
                        currentGG.RemoveAll(g => g.NumGiorno > newValue);
                    }
                    else if (uniqueDays.Count == newValue)
                    {
                        // stiamo alterando le righe all'interno di un giorno
                        retVal.Par_ProfiloOrarioGG = model.Data.Par_ProfiloOrarioGG;

                        if (model.Data.NumGiorno_Incrementa != 0)
                        {
                            if(model.Data.NumGiorno_Incrementa>0)
                            {
                                int tmpCounter = currentGG.Any(g => g.Id < 0) ? currentGG.Where(g => g.Id < 0).Min(g => g.Id) : 0;

                                int numGiorno =  Math.Abs( model.Data.NumGiorno_Incrementa);

                                var recOfDay = retVal.Par_ProfiloOrarioGG.Where( x=> x.NumGiorno == numGiorno ).OrderBy(x=> x.ZOrder).ToList();
                                var ZOrder = recOfDay[recOfDay.Count-1].ZOrder; 
                                defaultIdParOrario = recOfDay.First().IdPar_Orario;

                                currentGG.Add(new Par_ProfiloOrarioGGModel
                                {
                                    Id = --tmpCounter, // ID temporaneo negativo
                                    IdPar_ProfiloOrario = model.Data.Id,
                                    NumGiorno = numGiorno,
                                    ZOrder = ++ZOrder,
                                    IdPar_Orario = defaultIdParOrario
                                });
                            }
                            else
                            {
                                // Trova il record con ZOrder più alto (l'ultimo)
                                var recordToRemove = currentGG.OrderByDescending(g => g.ZOrder).First();
                                currentGG.Remove(recordToRemove);
                            }

                        }
                    }

                    retVal.Par_ProfiloOrarioGG = currentGG.OrderBy(g => g.NumGiorno).ThenBy(g => g.ZOrder).ToList();
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

        public Task<GenericResult<Par_ProfiloOrarioGG_Arrange_NumDay_OutModel>> Par_ProfiloOrarioGG_Arrange_NumDay(GenericRequest<Par_ProfiloOrarioGG_Arrange_NumDay_InModel> model, bool isSubProcess);
    }
}
