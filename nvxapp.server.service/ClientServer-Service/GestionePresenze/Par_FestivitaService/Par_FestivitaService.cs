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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService
{
    // Calendario delle festivita' dell'azienda
    public class Par_FestivitaService : ServiceBase, IPar_FestivitaService
    {
        private readonly IPar_FestivitaRepository _par_FestivitaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_FestivitaService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,

                                    IPar_FestivitaRepository par_FestivitaRepository,
                                    IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_FestivitaRepository = par_FestivitaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        private async Task<int> IdAzienda()
        {
            int.TryParse(this.CurrentCompany, out int idCompany);
            var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
            return company_DATA?.az_Anagrafica?.Id ?? 0;
        }

        public virtual async Task<GenericResult<Par_Festivita_GetAllOutModel>> GetAll(GenericRequest<Par_Festivita_GetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Festivita_GetAllOutModel();
                int idAzienda = await IdAzienda();
                if (idAzienda > 0)
                {
                    var lista = _par_FestivitaRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda).ToList();
                    retVal.Par_Festivita = _mapper.Map<List<Par_FestivitaModel>>(lista);
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Festivita_GetOutModel>> Par_FestivitaGet(GenericRequest<Par_Festivita_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Festivita_GetOutModel();
                var entity = await _par_FestivitaRepository.FindByIdAsync(model.Data.Id);
                retVal.Par_Festivita = entity != null
                    ? _mapper.Map<Par_FestivitaModel>(entity)
                    : new Par_FestivitaModel { TipoFestivita = TipoFestivita.DataRicorrente, Data = DateTime.Today };
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Festivita_PutOutModel>> Par_FestivitaPut(GenericRequest<Par_Festivita_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Festivita_PutOutModel();
                int idAzienda = await IdAzienda();
                if (idAzienda > 0)
                {
                    var entity = _mapper.Map<Par_Festivita>(model.Data.Par_Festivita);
                    entity.IdAz_Anagrafica = idAzienda;
                    entity.Data = entity.Data.Date;
                    entity = await _par_FestivitaRepository.UpsertAsync(entity);
                    retVal.Par_Festivita = _mapper.Map<Par_FestivitaModel>(entity);
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Festivita_DeleteOutModel>> Par_FestivitaDelete(GenericRequest<Par_Festivita_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_FestivitaRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _par_FestivitaRepository.DeleteAsync(entity);
                return new Par_Festivita_DeleteOutModel();
            }, isSubProcess);
        }
    }

    public interface IPar_FestivitaService : IServiceBase
    {
        Task<GenericResult<Par_Festivita_GetAllOutModel>> GetAll(GenericRequest<Par_Festivita_GetAllInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Festivita_GetOutModel>> Par_FestivitaGet(GenericRequest<Par_Festivita_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Festivita_PutOutModel>> Par_FestivitaPut(GenericRequest<Par_Festivita_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Festivita_DeleteOutModel>> Par_FestivitaDelete(GenericRequest<Par_Festivita_DeleteInModel> model, bool isSubProcess);
    }
}
