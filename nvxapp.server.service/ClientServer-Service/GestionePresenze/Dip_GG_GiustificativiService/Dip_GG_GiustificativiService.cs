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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService
{

    public class Dip_GG_GiustificativiService : ServiceBase, IDip_GG_GiustificativiService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_GiustificativiRepository _Dip_GG_GiustificativiRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public Dip_GG_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IPar_GiustificativiRepository par_GiustificativiRepository,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_GiustificativiRepository Dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _Dip_GG_GiustificativiRepository = Dip_GG_GiustificativiRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Giustificativi_GetAll_OutModel retVal = new Dip_GG_Giustificativi_GetAll_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    List<Dip_GG_Giustificativi> just = _Dip_GG_GiustificativiRepository.FindAll(x => x.Data.Year == model.Data.Year &&
                                                                                              x.Data.Month == model.Data.Month &&
                                                                                              x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                                          //.OrderBy(x => x.TimbraturaOriginale)
                                                                                          .ToList();



                    retVal.Dip_GG_Giustificativi = _mapper.Map<List<Dip_GG_GiustificativiModel>>(just);

                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Giustificativi_Get_4Calculation_OutModel retVal = new Dip_GG_Giustificativi_Get_4Calculation_OutModel();

                List<int> idRapportoLavoroList = new List<int>();

                foreach (string userId in model.Data.UsersId)
                {
                    User_DATA_COMB_DipAna_DipRapp userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, false);
                    if (userData?.dip_RapportoLavoro != null)
                    {
                        idRapportoLavoroList.Add(userData.dip_RapportoLavoro.Id);
                    }
                }

                if (idRapportoLavoroList.Count > 0)
                {
                    var giustificativi = _Dip_GG_GiustificativiRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        .ThenBy(x => x.Data)
                        .ToList();

                    retVal.Dip_GG_Giustificativi = _mapper.Map<List<Dip_GG_GiustificativiModel>>(giustificativi);
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_GiustificativiPutOutModel>> Dip_GG_GiustificativiPut(GenericRequest<Dip_GG_GiustificativiPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_GiustificativiPutOutModel retVal = new Dip_GG_GiustificativiPutOutModel();
                retVal.Dip_GG_Giustificativi = model.Data.Dip_GG_Giustificativi;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);



                Dip_GG_Giustificativi? dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Dip_GG_Giustificativi.Id);
                if (dip_GG_Giustificativi == null)
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                    if (model.Data.IdDip_RapportoLavoro > 0)
                        dip_GG_Giustificativi.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;
                }
                else
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                }

                dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.UpsertAsync(dip_GG_Giustificativi);
                retVal.Dip_GG_Giustificativi = _mapper.Map<Dip_GG_GiustificativiModel>(dip_GG_Giustificativi);



                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>> Dip_GG_GiustificativiDelete(GenericRequest<Dip_GG_Giustificativi_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _Dip_GG_GiustificativiRepository.DeleteAsync(entity);
                }
                return new Dip_GG_Giustificativi_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_GiustificativiGetOutModel>> Dip_GG_GiustificativiGet(GenericRequest<Dip_GG_GiustificativiGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_GiustificativiGetOutModel retVal = new Dip_GG_GiustificativiGetOutModel();
                
                

                int IdCompany, IdAnagrafica=0;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                    IdAnagrafica=company_DATA.az_Anagrafica.Id;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipRapp_DipAna(model.Data.IdDip_RapportoLavoro);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Giustificativi? dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                    int IdPar_Giustificativi =0;
                    var cau = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == IdAnagrafica).FirstOrDefault();
                    if(cau!=null)
                        IdPar_Giustificativi = cau.Id;

                    if (dip_GG_Giustificativi == null)
                        dip_GG_Giustificativi = new Dip_GG_Giustificativi()
                        {
                            Id = 0,
                            IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                            Data = model.Data.Data != null ? model.Data.Data.Value : DateTime.Now,
                            IdPar_Giustificativi = IdPar_Giustificativi,
                            Hours = new TimeSpan(1,0,0)
                        };


                    retVal.Dip_GG_Giustificativi = _mapper.Map<Dip_GG_GiustificativiModel>(dip_GG_Giustificativi);
                }

                return retVal;
            }, isSubProcess);
        }
    

    }

    public interface IDip_GG_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_GiustificativiPutOutModel>> Dip_GG_GiustificativiPut(GenericRequest<Dip_GG_GiustificativiPutInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>> Dip_GG_GiustificativiDelete(GenericRequest<Dip_GG_Giustificativi_DeleteInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_GiustificativiGetOutModel>> Dip_GG_GiustificativiGet(GenericRequest<Dip_GG_GiustificativiGetInModel> model, bool isSubProcess);
    }
}
