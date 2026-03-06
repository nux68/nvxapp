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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService
{

    public class Par_ProfiloOrarioService : ServiceBase, IPar_ProfiloOrarioService
    {
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_ProfiloOrarioGGService _par_ProfiloOrarioGGService;
        private readonly IPar_OrarioIntervalloHHService _par_OrarioIntervalloHHService;
        private readonly IPar_OrarioRepository _par_OrarioRepository;



        public Par_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IConfiguration configuration,
                                  IPar_ProfiloOrarioGGService par_ProfiloOrarioGGService,
                                  IPar_OrarioIntervalloHHService par_OrarioIntervalloHHService,
                                  IPar_OrarioRepository par_OrarioRepository,

                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_ProfiloOrarioGGService = par_ProfiloOrarioGGService;
            _par_OrarioIntervalloHHService = par_OrarioIntervalloHHService;
            _par_OrarioRepository = par_OrarioRepository;
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_GetAllOutModel>> GetAll(GenericRequest<Par_ProfiloOrario_GetAllInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrario_GetAllOutModel retVal = new Par_ProfiloOrario_GetAllOutModel();

                var entities = await _par_ProfiloOrarioRepository.FindAll();
                retVal.Par_ProfiloOrario = _mapper.Map<List<Par_ProfiloOrarioModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_GetOutModel>> Par_ProfiloOrarioGet(GenericRequest<Par_ProfiloOrario_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ProfiloOrario_GetOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    var profilo = await _par_ProfiloOrarioRepository.FindByIdAsync(model.Data.Id);
                    if (profilo != null)
                        retVal.Par_ProfiloOrario = _mapper.Map<Par_ProfiloOrarioModel>(profilo);
                    else
                    {
                        profilo = Init_Par_ProfiloOrario(company_DATA.az_Anagrafica.Id);
                        retVal.Par_ProfiloOrario = _mapper.Map<Par_ProfiloOrarioModel>(profilo);
                    }

                    var reqAz_Sub = new GenericRequest<Par_ProfiloOrarioGG_Get_4Edit_InModel>();
                    reqAz_Sub.Data.Id = model.Data.Id; // id del profilo orario
                    var resAz_Sub = await _par_ProfiloOrarioGGService.Par_ProfiloOrarioGG_Get(reqAz_Sub, true);

                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                        retVal.Par_ProfiloOrarioGG = resAz_Sub.Data.Par_ProfiloOrarioGG;

                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;


            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_PutOutModel>> Par_ProfiloOrarioPut(GenericRequest<Par_ProfiloOrario_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ProfiloOrario_PutOutModel();


                int.TryParse(this.CurrentCompany, out int idCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    var entity = _mapper.Map<Par_ProfiloOrario>(model.Data.Par_ProfiloOrario);
                    if (model.Data.Par_ProfiloOrario.IdAz_Anagrafica == 0)
                        entity.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;

                    var par_ProfiloOrario = await _par_ProfiloOrarioRepository.UpsertAsync(entity);
                    retVal.Par_ProfiloOrario = _mapper.Map<Par_ProfiloOrarioModel>(par_ProfiloOrario);

                    var reqPar_ProfiloOrarioGG = new GenericRequest<Par_ProfiloOrarioGG_Put_4Edit_InModel>();
                    reqPar_ProfiloOrarioGG.Data.Id = retVal.Par_ProfiloOrario.Id;
                    reqPar_ProfiloOrarioGG.Data.Par_ProfiloOrarioGG = model.Data.Par_ProfiloOrarioGG;
                    var resAz_Sub = await _par_ProfiloOrarioGGService.Par_ProfiloOrarioGG_Put(reqPar_ProfiloOrarioGG, true);

                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        retVal.Par_ProfiloOrarioGG = resAz_Sub.Data.Par_ProfiloOrarioGG;
                    }

                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_DeleteOutModel>> Par_ProfiloOrarioDelete(GenericRequest<Par_ProfiloOrario_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_ProfiloOrarioRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_ProfiloOrarioRepository.DeleteAsync(entity);
                }
                return new Par_ProfiloOrario_DeleteOutModel();
            }, isSubProcess);
        }


        private Par_ProfiloOrario Init_Par_ProfiloOrario(int IdAz_Anagrafica)
        {
            var par_Orario = _par_OrarioRepository.GetAll().FirstOrDefault();

            Par_ProfiloOrario retVal = new Par_ProfiloOrario()
            {
                IdAz_Anagrafica = IdAz_Anagrafica,
                Codice = "000",
                Descrizione = "Nuovo Profilo Orario",
                NumGiorniCiclo = 7,
                TipoProfilo = TipoProfilo.Settimanale,
                StraoSogliaHHFullTime = new TimeOnly(8, 0),
                StraoTipoConteggio = StraoTipoConteggio.Giornaliero,
                SupplTipoConteggio = StraoTipoConteggio.Giornaliero,
                IdPar_Orario_Festivo = par_Orario != null ? par_Orario.Id : 0 //MIGLIORARE
            };
            return retVal;
        }

    }

    public interface IPar_ProfiloOrarioService : IServiceBase
    {
        public Task<GenericResult<Par_ProfiloOrario_GetAllOutModel>> GetAll(GenericRequest<Par_ProfiloOrario_GetAllInModel> model, Boolean isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_GetOutModel>> Par_ProfiloOrarioGet(GenericRequest<Par_ProfiloOrario_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_PutOutModel>> Par_ProfiloOrarioPut(GenericRequest<Par_ProfiloOrario_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_DeleteOutModel>> Par_ProfiloOrarioDelete(GenericRequest<Par_ProfiloOrario_DeleteInModel> model, bool isSubProcess);

    }
}
