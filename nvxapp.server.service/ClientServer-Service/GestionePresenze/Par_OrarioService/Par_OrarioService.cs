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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService
{
    public class Par_OrarioService : ServiceBase, IPar_OrarioService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_OrarioIntervalloHHService _par_OrarioIntervalloHHService;
        private readonly IPar_OrarioPausaRepository _par_OrarioPausaRepository;



        public Par_OrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_OrarioIntervalloHHService par_OrarioIntervalloHHService,
                                  IPar_OrarioRepository par_OrarioRepository,
                                  IPar_CausaliRepository par_CausaliRepository,
                                  IPar_OrarioPausaRepository par_OrarioPausaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_OrarioRepository = par_OrarioRepository;
            _par_CausaliRepository = par_CausaliRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioIntervalloHHService = par_OrarioIntervalloHHService;
            _par_OrarioPausaRepository = par_OrarioPausaRepository;
        }

        private List<Par_OrarioPausaModel> PauseOrario(int idPar_Orario) =>
            _mapper.Map<List<Par_OrarioPausaModel>>(_par_OrarioPausaRepository.FindAll(x => x.IdPar_Orario == idPar_Orario)
                                                                              .OrderBy(x => x.NumPausa)
                                                                              .ToList());

        public virtual async Task<GenericResult<Par_Orario_GetAllOutModel>> GetAll(GenericRequest<Par_Orario_GetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_GetAllOutModel();
                var entities = await _par_OrarioRepository.FindAll();
                retVal.Par_Orario = _mapper.Map<List<Par_OrarioModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_GetOutModel>> Par_OrarioGet(GenericRequest<Par_Orario_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_GetOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entity = await _par_OrarioRepository.FindByIdAsync(model.Data.Id);
                    if (entity == null)
                    {
                        entity = new Par_Orario()
                        {
                            IdAz_Anagrafica = company_DATA.az_Anagrafica.Id,
                            Codice = "0000",
                            NumeroCoppie = 1,
                            Descrizione = "Nuovo Orario",
                        };
                    }
                    retVal.Par_Orario = _mapper.Map<Par_OrarioModel>(entity);

                    var req_1 = new GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel>();
                    req_1.Data = new Par_OrarioIntervalloHH_Get_4Edit_InModel() { Id = model.Data.Id };
                    var res_1 = await _par_OrarioIntervalloHHService.Par_OrarioIntervalloHH_Get(req_1, true);
                    if (res_1.Success && res_1.Data != null)
                    {
                        retVal.Par_OrarioIntervalloHH = res_1.Data.Par_OrarioIntervalloHH;
                    }

                    retVal.Par_OrarioPausa = PauseOrario(model.Data.Id);
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_PutOutModel>> Par_OrarioPut(GenericRequest<Par_Orario_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_PutOutModel();
                // la causale del monte ore riceve ore: niente causali a importo
                GestionePresenzeTipoValore.ControllaCausaliOrarie(_par_CausaliRepository, model.Data.Par_Orario.IdCausale_HH_Lav_MonteOre);

                // intervalli di riconoscimento coerenti e pause valide (ANALISI_ATTENDANCETRACKING_PAUSE.md)
                if (model.Data.Par_Orario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                    GestionePresenzeOrarioValidazione.ControllaIntervalli(model.Data.Par_OrarioIntervalloHH);
                GestionePresenzeOrarioValidazione.ControllaPause(model.Data.Par_Orario.TimbratureTipo, model.Data.Par_OrarioIntervalloHH, model.Data.Par_OrarioPausa);
                // i minuti detratti vanno su una causale a ore
                GestionePresenzeTipoValore.ControllaCausaliOrarie(_par_CausaliRepository, model.Data.Par_OrarioPausa.Select(p => p.IdCausale_Pausa).ToArray());

                var entity = _mapper.Map<Par_Orario>(model.Data.Par_Orario);

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    entity.IdAz_Anagrafica = companyData.az_Anagrafica.Id;
                }
                var updatedEntity = await _par_OrarioRepository.UpsertAsync(entity);
                retVal.Par_Orario = _mapper.Map<Par_OrarioModel>(updatedEntity);


                var req_1 = new GenericRequest<Par_OrarioIntervalloHH_Put_4Edit_InModel>();
                req_1.Data = new Par_OrarioIntervalloHH_Put_4Edit_InModel()
                {
                    Id = retVal.Par_Orario.Id,
                    Par_OrarioIntervalloHH = model.Data.Par_OrarioIntervalloHH
                };
                var res_1 = await _par_OrarioIntervalloHHService.Par_OrarioIntervalloHH_Put(req_1, true);
                if (res_1.Success && res_1.Data != null)
                {
                    retVal.Par_OrarioIntervalloHH = res_1.Data.Par_OrarioIntervalloHH;
                }

                // pause: cancellate quelle tolte dal client, salvate le altre
                var idOrario = retVal.Par_Orario.Id;
                var inviate = model.Data.Par_OrarioPausa;
                inviate.Where(x => x.Id < 0).ToList().ForEach(x => x.Id = 0);
                foreach (var esistente in _par_OrarioPausaRepository.FindAll(x => x.IdPar_Orario == idOrario).ToList())
                    if (!inviate.Any(x => x.Id == esistente.Id))
                        await _par_OrarioPausaRepository.DeleteAsync(esistente);
                foreach (var item in inviate)
                {
                    var pausa = _mapper.Map<Par_OrarioPausa>(item);
                    pausa.IdPar_Orario = idOrario;
                    if (pausa.IdCausale_Pausa is <= 0)
                        pausa.IdCausale_Pausa = null;
                    await _par_OrarioPausaRepository.UpsertAsync(pausa);
                }
                retVal.Par_OrarioPausa = PauseOrario(idOrario);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_DeleteOutModel>> Par_OrarioDelete(GenericRequest<Par_Orario_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_OrarioRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_OrarioRepository.DeleteAsync(entity);
                }
                return new Par_Orario_DeleteOutModel();
            }, isSubProcess);
        }
    }

    public interface IPar_OrarioService : IServiceBase
    {
        Task<GenericResult<Par_Orario_GetAllOutModel>> GetAll(GenericRequest<Par_Orario_GetAllInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_GetOutModel>> Par_OrarioGet(GenericRequest<Par_Orario_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_PutOutModel>> Par_OrarioPut(GenericRequest<Par_Orario_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_DeleteOutModel>> Par_OrarioDelete(GenericRequest<Par_Orario_DeleteInModel> model, bool isSubProcess);
    }
}
