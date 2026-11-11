using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService
{
    /*
     Organizzazione dell'azienda in SOLA LETTURA (tabelle comuni Az_Sedi, Az_SediReparto, Par_Competenza, schema Common,
     INF-COM.5): per i moduli che non sono le presenze (HR, compliance, risorse aziendali).
     La gestione (inserimento, modifica, cancellazione) resta alle pagine delle presenze.
     Disponibile con qualsiasi applicativo attivo; ogni lettura e' limitata all'azienda della richiesta.
     Nome "CommonOrganizzazione" per non confondersi con i servizi delle presenze (Az_SediService, Par_CompetenzaService).
    */
    public class CommonOrganizzazioneService : ServiceBase, ICommonOrganizzazioneService
    {
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IAz_SediCommonRepository _az_SediRepository;
        private readonly IAz_SediRepartoCommonRepository _az_SediRepartoRepository;
        private readonly IPar_CompetenzaCommonRepository _par_CompetenzaRepository;

        public CommonOrganizzazioneService(IMapper mapper,
                                           UserManager<ApplicationUser> userManager,
                                           IAspNetUsersRepository aspNetUsersRepository,
                                           IOptions<JwtParameter> jwtParameter,
                                           IHttpContextAccessor httpContextAccessor,
                                           IConfiguration configuration,
                                           ICommonCompanyUtility commonCompanyUtility,
                                           IAz_SediCommonRepository az_SediRepository,
                                           IAz_SediRepartoCommonRepository az_SediRepartoRepository,
                                           IPar_CompetenzaCommonRepository par_CompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _commonCompanyUtility = commonCompanyUtility;
            _az_SediRepository = az_SediRepository;
            _az_SediRepartoRepository = az_SediRepartoRepository;
            _par_CompetenzaRepository = par_CompetenzaRepository;
        }

        // sedi dell'azienda con l'albero dei reparti
        public virtual async Task<GenericResult<CommonOrganizzazione_SediGetAll_OutModel>> SediGetAll(GenericRequest<CommonOrganizzazione_SediGetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CommonOrganizzazione_SediGetAll_OutModel retVal = new CommonOrganizzazione_SediGetAll_OutModel();

                var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
                var sedi = await _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == az_Anagrafica.Id)
                                                   .OrderBy(x => x.Descrizione).ToListAsync();
                var reparti = await _az_SediRepartoRepository.FindAll(x => x.Az_SediNavigation!.IdAz_Anagrafica == az_Anagrafica.Id)
                                                            .OrderBy(x => x.Descrizione).ToListAsync();

                // albero: ogni reparto sotto il padre; senza padre (o con un padre non trovato) sotto la sede
                var sediModel = _mapper.Map<List<Az_SediCommonModel>>(sedi);
                var sediById = sediModel.ToDictionary(x => x.Id);
                var repartiModel = _mapper.Map<List<Az_SediRepartoCommonModel>>(reparti);
                var repartiById = repartiModel.ToDictionary(x => x.Id);
                foreach (var reparto in repartiModel)
                {
                    if (reparto.IdAz_SediReparto.HasValue && reparto.IdAz_SediReparto != reparto.Id
                        && repartiById.TryGetValue(reparto.IdAz_SediReparto.Value, out var padre))
                        padre.Az_SediReparto_Sub.Add(reparto);
                    else if (sediById.TryGetValue(reparto.IdAz_Sedi, out var sede))
                        sede.Az_SediReparto.Add(reparto);
                }

                retVal.Az_Sedi = sediModel;
                return retVal;
            }, isSubProcess);
        }

        // competenze definite dall'azienda
        public virtual async Task<GenericResult<CommonOrganizzazione_CompetenzeGetAll_OutModel>> CompetenzeGetAll(GenericRequest<CommonOrganizzazione_CompetenzeGetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CommonOrganizzazione_CompetenzeGetAll_OutModel retVal = new CommonOrganizzazione_CompetenzeGetAll_OutModel();

                var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
                var competenze = await _par_CompetenzaRepository.FindAll(x => x.IdAz_Anagrafica == az_Anagrafica.Id)
                                                               .OrderBy(x => x.Descrizione).ToListAsync();
                retVal.Par_Competenza = _mapper.Map<List<Par_CompetenzaCommonModel>>(competenze);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface ICommonOrganizzazioneService : IServiceBase
    {
        Task<GenericResult<CommonOrganizzazione_SediGetAll_OutModel>> SediGetAll(GenericRequest<CommonOrganizzazione_SediGetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<CommonOrganizzazione_CompetenzeGetAll_OutModel>> CompetenzeGetAll(GenericRequest<CommonOrganizzazione_CompetenzeGetAll_InModel> model, bool isSubProcess);
    }
}
