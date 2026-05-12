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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService
{
    public interface IDip_Rapporto_Giustificativi_MaturazioneService
    {
        Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_GetAll_OutModel>> GetAll(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_GetAll_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_Upsert_OutModel>> Upsert(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_Upsert_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_Delete_OutModel>> Delete(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_Delete_InModel> model,
            bool isSubProcess = false);
    }

    public class Dip_Rapporto_Giustificativi_MaturazioneService : ServiceBase, IDip_Rapporto_Giustificativi_MaturazioneService
    {
        private readonly IDip_Rapporto_Giustificativi_MaturazioneRepository _maturazioneRepository;

        public Dip_Rapporto_Giustificativi_MaturazioneService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IDip_Rapporto_Giustificativi_MaturazioneRepository maturazioneRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _maturazioneRepository = maturazioneRepository;
        }

        // ?? GetAll ????????????????????????????????????????????????????????????

        public virtual async Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_GetAll_OutModel>> GetAll(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_GetAll_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Rapporto_Giustificativi_Maturazione_GetAll_OutModel();

                var entities = _maturazioneRepository
                    .FindAll(m => m.IdDip_RapportoLavoro == model.Data.IdDip_RapportoLavoro)
                    .ToList();

                retVal.Maturazioni = entities.Select(e => _mapper.Map<Dip_Rapporto_Giustificativi_MaturazioneModel>(e)).ToList();

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // ?? Upsert ????????????????????????????????????????????????????????????

        public virtual async Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_Upsert_OutModel>> Upsert(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_Upsert_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Rapporto_Giustificativi_Maturazione_Upsert_OutModel();
                var src    = model.Data.Maturazione;

                // Cerca prima per Id, poi per chiave business (IdDip_RapportoLavoro + IdPar_Giustificativi)
                Dip_Rapporto_Giustificativi_Maturazione? entity = null;
                if (src.Id > 0)
                    entity = await _maturazioneRepository.FindByIdAsync(src.Id);

                if (entity == null)
                    entity = _maturazioneRepository.FindAll(m =>
                        m.IdDip_RapportoLavoro == src.IdDip_RapportoLavoro &&
                        m.IdPar_Giustificativi == src.IdPar_Giustificativi).FirstOrDefault();

                if (entity == null)
                    entity = new Dip_Rapporto_Giustificativi_Maturazione
                    {
                        IdDip_RapportoLavoro = src.IdDip_RapportoLavoro,
                        IdPar_Giustificativi = src.IdPar_Giustificativi
                    };

                _mapper.Map(src, entity);
                entity.IdDip_RapportoLavoro = src.IdDip_RapportoLavoro;
                entity.IdPar_Giustificativi = src.IdPar_Giustificativi;

                var saved = await _maturazioneRepository.UpsertAsync(entity);
                retVal.Maturazione = _mapper.Map<Dip_Rapporto_Giustificativi_MaturazioneModel>(saved);

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // ?? Delete ????????????????????????????????????????????????????????????

        public virtual async Task<GenericResult<Dip_Rapporto_Giustificativi_Maturazione_Delete_OutModel>> Delete(
            GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_Delete_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _maturazioneRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _maturazioneRepository.DeleteAsync(entity);

                await Task.Delay(DelayAsyncMethod);
                return new Dip_Rapporto_Giustificativi_Maturazione_Delete_OutModel();
            }, isSubProcess);
        }

        // ?? Helpers ???????????????????????????????????????????????????????????

    }
}