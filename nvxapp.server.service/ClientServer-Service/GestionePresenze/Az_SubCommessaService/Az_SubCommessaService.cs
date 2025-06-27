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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService
{
    public class Az_SubCommessaService : ServiceBase, IAz_SubCommessaService
    {
        private readonly IAz_SubCommessaRepository _az_SubCommessaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;

        public Az_SubCommessaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaUserRepository az_SubCommessaUserRepository,
                                  IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository,
                                  IAz_SubCommessaRepository az_SubCommessaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaRepository = az_SubCommessaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;
        }

        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_OutModel retVal = new Az_SubCommessa_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = (await _az_SubCommessaRepository.FindAll()).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessaModel>>(az_SubCommessa);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_4Edit_OutModel retVal = new Az_SubCommessa_GetAll_4Edit_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == model.Data.Id).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessa_4EditModel>>(az_SubCommessa);
                    if (retVal.Az_SubCommessa != null)
                    {
                        var Az_SubCommessa_Id = retVal.Az_SubCommessa.Select(x => x.Id).ToList();

                        var az_SubCommessaUser = _az_SubCommessaUserRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();
                        var az_SubCommessaAttivita = _az_SubCommessaAttivitaRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();
                        var az_SubCommessaSediReparto = _az_SubCommessaSediRepartoRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();

                        foreach (var itemSubCommessa in retVal.Az_SubCommessa)
                        {
                            az_SubCommessaUser.Where(x => x.IdAz_SubCommessa == itemSubCommessa.Id).ToList().ForEach(x =>
                            {
                                itemSubCommessa.Az_SubCommessaUser.Add(new CheckObjOn_Id_Text()
                                {
                                    Id = x.IdAspNetUsers,
                                    Checked = true
                                });
                            });

                            az_SubCommessaAttivita.Where(x => x.IdAz_SubCommessa == itemSubCommessa.Id).ToList().ForEach(x =>
                            {
                                itemSubCommessa.Az_SubCommessaAttivita.Add(new CheckObjOn_Id_Number()
                                {
                                    Id = x.IdAz_SediAttivita,
                                    Checked = true
                                });
                            });

                            az_SubCommessaSediReparto.Where(x => x.IdAz_SubCommessa == itemSubCommessa.Id).ToList().ForEach(x =>
                            {
                                itemSubCommessa.Az_SubCommessaSediReparto.Add(new CheckObjOn_Id_Number()
                                {
                                    Id = x.IdAz_SediReparto,
                                    Checked = true
                                });
                            });
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_PutAll_4Edit_OutModel retVal = new Az_SubCommessa_PutAll_4Edit_OutModel();
                retVal.Id = model.Data.Id;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);


                var az_SubCommessaList = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == model.Data.Id).ToList();
                //cancellazione
                foreach (var item in az_SubCommessaList)
                {
                    var recRemote = model.Data.Az_SubCommessa.Where(x => x.Id == item.Id).FirstOrDefault();
                    if (recRemote == null)
                    {
                        _az_SubCommessaRepository.DeleteAsync(item).Wait();
                    }
                }

                //aggiornamento
                foreach (var item in model.Data.Az_SubCommessa)
                {
                    Az_SubCommessa? az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == item.Id).FirstOrDefault();
                    if(az_SubCommessa==null)
                    {
                        az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                        az_SubCommessa.IdAz_Commessa = model.Data.Id;
                    }
                    else
                    {
                        az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                    }
                    //aggiurna il valore ritornato al client
                    az_SubCommessa = await _az_SubCommessaRepository.UpsertAsync(az_SubCommessa);
                    
                    //sooka
                }

                
                //rileggo i dati 
                var reqAz_Sub = new GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel>();
                reqAz_Sub.Data.Id= model.Data.Id; 
                var resAz_Sub = await GetAll_4Edit(reqAz_Sub, true);
                if (resAz_Sub.Success && resAz_Sub.Data != null)
                {
                    retVal.Az_SubCommessa = resAz_Sub.Data.Az_SubCommessa;
                }



                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess);

    }
}
