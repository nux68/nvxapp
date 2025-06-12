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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService
{

    public class Az_SediService : ServiceBase, IAz_SediService
    {
        private readonly IAz_SediRepository _az_SediRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAz_SediAttivitaRepository _az_SediAttivitaRepository;
        private readonly IAz_SediAttivitaService _az_SediAttivitaService;
        


        public Az_SediService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediAttivitaRepository az_SediAttivitaRepository,
                                  IAz_SediAttivitaService az_SediAttivitaService,

                                  IAz_SediRepository az_SediRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepository = az_SediRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediAttivitaRepository = az_SediAttivitaRepository;
            _az_SediAttivitaService = az_SediAttivitaService;
        }

        public virtual async Task<GenericResult<Az_Sedi_GetAll_OutModel>> GetAll(GenericRequest<Az_Sedi_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Sedi_GetAll_OutModel retVal = new Az_Sedi_GetAll_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg != null)
                {
                    var az_Sedi = _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto!.az_Anagrafica!.Id).ToList();
                    retVal.Az_Sedi = _mapper.Map<List<Az_SediModel>>(az_Sedi);
                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediGetOutModel>> AzSediGet(GenericRequest<Az_SediGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediGetOutModel retVal = new Az_SediGetOutModel();
                var entity = await _az_SediRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    retVal.Az_Sedi = _mapper.Map<Az_SediModel>(entity);
                }
                else
                {
                    retVal.Az_Sedi = new Az_SediModel();
                }

                GenericRequest<Az_SediAttivita_Selected_GetInModel> az_SediAttivita_Selected_GetInModel = new GenericRequest<Az_SediAttivita_Selected_GetInModel>();
                az_SediAttivita_Selected_GetInModel.Data.IdAz_Sedi = retVal.Az_Sedi.Id;

                var res2 = await _az_SediAttivitaService.GetSelected_On_Az_Sedi(az_SediAttivita_Selected_GetInModel, true);
                if (res2.Success && res2.Data != null)
                {
                    res2.Data.Az_SediAttivita.ForEach(item =>
                    {
                        retVal.Az_SediAttivita.Add(new CheckObjOn_Id_Number()
                        {
                            Id = item.IdPar_Attivita,
                            Checked = true
                        });
                    });
                }


                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediPutOutModel>> AzSediPut(GenericRequest<Az_SediPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediPutOutModel retVal = new Az_SediPutOutModel();
                retVal.Az_Sedi = model.Data.Az_Sedi;
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    Az_Sedi? az_Sedi = await _az_SediRepository.FindByIdAsync(model.Data.Az_Sedi.Id);
                    if (az_Sedi == null)
                    {
                        az_Sedi = _mapper.Map<Az_Sedi>(model.Data.Az_Sedi);
                        az_Sedi.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Sedi = _mapper.Map<Az_Sedi>(model.Data.Az_Sedi);
                    }
                    az_Sedi = await _az_SediRepository.UpsertAsync(az_Sedi);
                    retVal.Az_Sedi = _mapper.Map<Az_SediModel>(az_Sedi);


                    GenericRequest<Az_SediAttivita_Selected_PutInModel> az_SediAttivita_Selected_PutInModel = new GenericRequest<Az_SediAttivita_Selected_PutInModel>();
                    az_SediAttivita_Selected_PutInModel.Data.IdAz_Sedi = retVal.Az_Sedi.Id;

                    foreach (var item in model.Data.Az_SediAttivita.Where(x=>x.Checked).ToList())
                    {
                        az_SediAttivita_Selected_PutInModel.Data.Az_SediAttivita.Add(new Az_SediAttivitaModel()
                        {
                             IdPar_Attivita= item.Id,
                             IdAz_Sedi = retVal.Az_Sedi.Id
                        });
                    }
                    var res2 = await _az_SediAttivitaService.PutSelected_On_Az_Sedi(az_SediAttivita_Selected_PutInModel, true);
                    if (res2.Success && res2.Data != null){}



                }
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediDeleteOutModel>> AzSediDelete(GenericRequest<Az_SediDeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediDeleteOutModel retVal = new Az_SediDeleteOutModel();
                var entity = await _az_SediRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _az_SediRepository.DeleteAsync(entity);
                    retVal.Az_Sedi = _mapper.Map<Az_SediModel>(entity);
                }
                else
                {
                    retVal.AddMessage($"Elemento con Id {model.Data.Id} non trovato.", MessageType.Warning);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SediService : IServiceBase
    {
        public Task<GenericResult<Az_Sedi_GetAll_OutModel>> GetAll(GenericRequest<Az_Sedi_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediGetOutModel>> AzSediGet(GenericRequest<Az_SediGetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediPutOutModel>> AzSediPut(GenericRequest<Az_SediPutInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediDeleteOutModel>> AzSediDelete(GenericRequest<Az_SediDeleteInModel> model, Boolean isSubProcess);
    }
}
