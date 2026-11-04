using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteService
{
    /*
     Clienti dell'azienda (tabella comune Az_Cliente, schema Common): disponibili con qualsiasi
     applicativo attivo. Ogni operazione e' limitata ai clienti dell'azienda della richiesta.
    */
    public class Az_ClienteService : ServiceBase, IAz_ClienteService
    {
        private readonly IAz_ClienteRepository _az_ClienteRepository;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IEnumerable<ICommonEntityUsage> _commonEntityUsages;
        private readonly IApplicationActivationAuthorizer _authorizer;

        public Az_ClienteService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 ICommonCompanyUtility commonCompanyUtility,
                                 ICompanyApplicationRepository companyApplicationRepository,
                                 IEnumerable<ICommonEntityUsage> commonEntityUsages,
                                 IApplicationActivationAuthorizer authorizer,
                                 IAz_ClienteRepository az_ClienteRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _authorizer = authorizer;
            _az_ClienteRepository = az_ClienteRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _companyApplicationRepository = companyApplicationRepository;
            _commonEntityUsages = commonEntityUsages;
        }

        public virtual async Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Cliente_GetAll_OutModel retVal = new Az_Cliente_GetAll_OutModel();

                var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
                var az_Cliente = _az_ClienteRepository.FindAll(x => x.IdAz_Anagrafica == az_Anagrafica.Id).ToList();
                retVal.Az_Cliente = _mapper.Map<List<Az_ClienteModel>>(az_Cliente);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_ClienteGetOutModel retVal = new Az_ClienteGetOutModel();

                var az_Cliente = await ClienteDellAzienda(model.Data.Id);
                retVal.Az_Cliente = az_Cliente != null
                    ? _mapper.Map<Az_ClienteModel>(az_Cliente)
                    : new Az_ClienteModel { Id = 0, IdAz_Anagrafica = 0 };

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_ClientePutOutModel retVal = new Az_ClientePutOutModel();
                await SoloAmministratori();

                var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);

                // un cliente esistente deve essere dell'azienda corrente
                if (model.Data.Az_Cliente.Id != 0 && await ClienteDellAzienda(model.Data.Az_Cliente.Id) == null)
                    model.Data.Az_Cliente.Id = 0;

                var az_Cliente = _mapper.Map<nvxapp.server.data.Entities.Tenant.Az_Cliente>(model.Data.Az_Cliente);
                az_Cliente.IdAz_Anagrafica = az_Anagrafica.Id;
                az_Cliente = await _az_ClienteRepository.UpsertAsync(az_Cliente);

                retVal.Az_Cliente = _mapper.Map<Az_ClienteModel>(az_Cliente);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClienteDeleteOutModel>> Az_ClienteDelete(GenericRequest<Az_ClienteDeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_ClienteDeleteOutModel retVal = new Az_ClienteDeleteOutModel();
                await SoloAmministratori();

                var az_Cliente = await ClienteDellAzienda(model.Data.Id);
                if (az_Cliente != null)
                {
                    // usato da un applicativo (es. commesse delle presenze): non si cancella
                    int idCompany = int.Parse(this.CurrentCompany);
                    var applicativi = _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany).Select(x => x.ApplicationType).ToList();
                    var usi = new List<string>();
                    foreach (var usage in _commonEntityUsages.Where(x => applicativi.Contains(x.Application)))
                        usi.AddRange(await usage.Az_ClienteUsageAsync(idCompany, az_Cliente.Id));
                    if (usi.Count > 0)
                        throw new UserMessageException(InfrastructureMessages.Get("CustomerInUse", az_Cliente.Descrizione, string.Join(", ", usi)));

                    retVal.Az_Cliente = _mapper.Map<Az_ClienteModel>(az_Cliente);
                    await _az_ClienteRepository.DeleteAsync(az_Cliente);
                }

                return retVal;
            }, isSubProcess);
        }

        // modifiche all'anagrafica comune dei clienti: amministratori dell'azienda (le pagine degli applicativi,
        // es. il CRM per i commerciali, hanno servizi propri con le loro regole)
        private async Task SoloAmministratori()
        {
            if (!await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.CompanyPowerAdmin)) && !await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.CompanyAdmin)))
                throw new UserMessageException(InfrastructureMessages.Get("CustomerEditNotAllowed"));
        }

        // cliente solo se appartiene all'azienda della richiesta (in modalita' singola le aziende condividono la tabella)
        private async Task<nvxapp.server.data.Entities.Tenant.Az_Cliente?> ClienteDellAzienda(int id)
        {
            if (id <= 0)
                return null;

            var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
            var az_Cliente = await _az_ClienteRepository.FindByIdAsync(id);
            return az_Cliente != null && az_Cliente.IdAz_Anagrafica == az_Anagrafica.Id ? az_Cliente : null;
        }
    }

    public interface IAz_ClienteService : IServiceBase
    {
        Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteDeleteOutModel>> Az_ClienteDelete(GenericRequest<Az_ClienteDeleteInModel> model, bool isSubProcess);
    }
}
