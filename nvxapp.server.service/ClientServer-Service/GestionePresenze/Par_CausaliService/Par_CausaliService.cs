using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService
{

    public class Par_CausaliService : ServiceBase, IPar_CausaliService
    {
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        // per sapere se una causale e' gia' usata (il tipo Orario/Importo non si puo' piu' cambiare)
        private readonly IDip_GG_CausaliRepository _dip_GG_CausaliRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IPar_ExportCau_CausaliRepository _par_ExportCau_CausaliRepository;

        public Par_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_CausaliRepository par_CausaliRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_CausaliRepository dip_GG_CausaliRepository,
                                  IPar_GiustificativiRepository par_GiustificativiRepository,
                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                  IPar_OrarioRepository par_OrarioRepository,
                                  IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository,
                                  IPar_ExportCau_CausaliRepository par_ExportCau_CausaliRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_CausaliRepository = par_CausaliRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_CausaliRepository = dip_GG_CausaliRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_ExportCau_CausaliRepository = par_ExportCau_CausaliRepository;
        }

        public virtual async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliOutModel retVal = new Par_CausaliOutModel();

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var par_Causali_all = _par_CausaliRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).ToList();
                    retVal.Par_Causali = _mapper.Map<List<Par_CausaliModel>>(par_Causali_all);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CausaliGetOutModel>> Par_CausaliGet(GenericRequest<Par_CausaliGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliGetOutModel retVal = new Par_CausaliGetOutModel();

                var par_Causale = await _par_CausaliRepository.FindByIdAsync(model.Data.Id);
                if (par_Causale != null)
                {
                    retVal.Par_Causale = _mapper.Map<Par_CausaliModel>(par_Causale);
                }
                else
                {
                    retVal.Par_Causale = new Par_CausaliModel { };
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CausaliPutOutModel>> Par_CausaliPut(GenericRequest<Par_CausaliPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliPutOutModel retVal = new Par_CausaliPutOutModel();
                retVal.Par_Causale = model.Data.Par_Causale;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    Par_Causali? par_Causale = await _par_CausaliRepository.FindByIdAsync(model.Data.Par_Causale.Id);
                    if (par_Causale == null)
                    {
                        par_Causale = _mapper.Map<Par_Causali>(model.Data.Par_Causale);
                        par_Causale.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        // il tipo (ore / importo) non si cambia se la causale e' gia' usata: i valori registrati cambierebbero significato
                        if (par_Causale.Tipo != model.Data.Par_Causale.Tipo && CausaleUsata(par_Causale.Id))
                            throw new UserMessageException(AttendanceTrackingMessages.Get("ParCausaleTipoNonModificabile", par_Causale.Codice));

                        //update 
                        par_Causale = _mapper.Map<Par_Causali>(model.Data.Par_Causale);
                    }

                    //aggiurna il valore ritornato al client
                    par_Causale = await _par_CausaliRepository.UpsertAsync(par_Causale);
                    retVal.Par_Causale = _mapper.Map<Par_CausaliModel>(par_Causale);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    
        /// <summary>causale registrata nei giorni dei dipendenti o collegata a giustificativi, profili, orari o modelli di export</summary>
        private bool CausaleUsata(int id) =>
            _dip_GG_CausaliRepository.FindAll(x => x.IdPar_Causali == id).Any()
            || _par_GiustificativiRepository.FindAll(x => x.IdCausale == id).Any()
            || _par_ProfiloOrarioRepository.FindAll(x => x.IdCausale_Lavoro_Strao == id || x.IdCausale_Lavoro_StraoNott == id || x.IdCausale_Lavoro_StraoFest == id
                                                         || x.IdCausale_Lavoro_StraoFestNott == id || x.IdCausale_Lavoro_Suppl == id || x.IdCausale_Lavoro_SupplEcc == id
                                                         || x.IdCausale_Lavoro_EccOrd == id || x.IdCausale_BancaOre == id).Any()
            || _par_OrarioRepository.FindAll(x => x.IdCausale_HH_Lav_MonteOre == id).Any()
            || _par_OrarioIntervalloHHRepository.FindAll(x => x.IdCausale_HH_Lav == id).Any()
            || _par_ExportCau_CausaliRepository.FindAll(x => x.IdCausale == id).Any();

        public virtual async Task<GenericResult<Par_Causali_DeleteOutModel>> Par_CausaliDelete(GenericRequest<Par_Causali_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_CausaliRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_CausaliRepository.DeleteAsync(entity);
                }
                return new Par_Causali_DeleteOutModel();
            }, isSubProcess);
        }
        
    }

    public interface IPar_CausaliService : IServiceBase
    {
        public Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, bool isSubProcess);
        public Task<GenericResult<Par_CausaliGetOutModel>> Par_CausaliGet(GenericRequest<Par_CausaliGetInModel> model, bool isSubProcess);
        public Task<GenericResult<Par_CausaliPutOutModel>> Par_CausaliPut(GenericRequest<Par_CausaliPutInModel> model, bool isSubProcess);
        public Task<GenericResult<Par_Causali_DeleteOutModel>> Par_CausaliDelete(GenericRequest<Par_Causali_DeleteInModel> model, bool isSubProcess);
    }
}
