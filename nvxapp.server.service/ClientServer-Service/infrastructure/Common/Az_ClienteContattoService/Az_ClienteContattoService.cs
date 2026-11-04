using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService
{
    /*
     Contatti (persone di riferimento) dei clienti dell'azienda: tabella comune Az_ClienteContatto.
     Ogni operazione e' limitata ai clienti dell'azienda della richiesta; al massimo un contatto
     principale per cliente (salvandone uno principale gli altri smettono di esserlo).
    */
    public class Az_ClienteContattoService : ServiceBase, IAz_ClienteContattoService
    {
        private readonly IAz_ClienteContattoRepository _az_ClienteContattoRepository;
        private readonly IAz_ClienteRepository _az_ClienteRepository;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IApplicationActivationAuthorizer _authorizer;

        public Az_ClienteContattoService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,
                                         ICommonCompanyUtility commonCompanyUtility,
                                         IApplicationActivationAuthorizer authorizer,
                                         IAz_ClienteRepository az_ClienteRepository,
                                         IAz_ClienteContattoRepository az_ClienteContattoRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _commonCompanyUtility = commonCompanyUtility;
            _authorizer = authorizer;
            _az_ClienteRepository = az_ClienteRepository;
            _az_ClienteContattoRepository = az_ClienteContattoRepository;
        }

        public virtual async Task<GenericResult<Az_ClienteContatto_GetAll_OutModel>> GetAll(GenericRequest<Az_ClienteContatto_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Az_ClienteContatto_GetAll_OutModel();

                var cliente = await ClienteDellAzienda(model.Data.IdAz_Cliente);
                var contatti = await _az_ClienteContattoRepository.FindAll(x => x.IdAz_Cliente == cliente.Id)
                                                                  .OrderByDescending(x => x.Principale).ThenBy(x => x.Cognome).ThenBy(x => x.Nome)
                                                                  .ToListAsync();
                retVal.Az_ClienteContatto = _mapper.Map<List<Az_ClienteContattoModel>>(contatti);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClienteContattoGetOutModel>> Az_ClienteContattoGet(GenericRequest<Az_ClienteContattoGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Az_ClienteContattoGetOutModel();
                var contatto = await ContattoDellAzienda(model.Data.Id);
                if (contatto != null)
                    retVal.Az_ClienteContatto = _mapper.Map<Az_ClienteContattoModel>(contatto);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClienteContattoPutOutModel>> Az_ClienteContattoPut(GenericRequest<Az_ClienteContattoPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Az_ClienteContattoPutOutModel();
                await SoloAmministratori();
                var dati = model.Data.Az_ClienteContatto;

                if (string.IsNullOrWhiteSpace(dati.Cognome) && string.IsNullOrWhiteSpace(dati.Nome))
                    throw new UserMessageException(InfrastructureMessages.Get("CustomerContactNameRequired"));

                if (dati.Id != 0)
                {
                    var esistente = await ContattoDellAzienda(dati.Id)
                                    ?? throw new UserMessageException(InfrastructureMessages.Get("CustomerContactNotFound"));
                    dati.IdAz_Cliente = esistente.IdAz_Cliente; // il contatto non cambia cliente
                }
                var cliente = await ClienteDellAzienda(dati.IdAz_Cliente);

                var contatto = _mapper.Map<Az_ClienteContatto>(dati);
                contatto.IdAz_Cliente = cliente.Id;
                contatto = await _az_ClienteContattoRepository.UpsertAsync(contatto);

                if (contatto.Principale)
                {
                    var altri = await _az_ClienteContattoRepository.FindAll(x => x.IdAz_Cliente == cliente.Id && x.Principale && x.Id != contatto.Id).ToListAsync();
                    foreach (var altro in altri)
                    {
                        altro.Principale = false;
                        await _az_ClienteContattoRepository.UpdateAsync(altro);
                    }
                }

                retVal.Az_ClienteContatto = _mapper.Map<Az_ClienteContattoModel>(contatto);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClienteContattoDeleteOutModel>> Az_ClienteContattoDelete(GenericRequest<Az_ClienteContattoDeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloAmministratori();
                var retVal = new Az_ClienteContattoDeleteOutModel();
                var contatto = await ContattoDellAzienda(model.Data.Id);
                if (contatto != null)
                {
                    retVal.Az_ClienteContatto = _mapper.Map<Az_ClienteContattoModel>(contatto);
                    await _az_ClienteContattoRepository.DeleteAsync(contatto);
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

        private async Task<Az_Cliente> ClienteDellAzienda(int idAz_Cliente)
        {
            var az_Anagrafica = await _commonCompanyUtility.CurrentAz_AnagraficaAsync(this.CurrentCompany);
            var cliente = idAz_Cliente > 0 ? await _az_ClienteRepository.FindByIdAsync(idAz_Cliente) : null;
            if (cliente == null || cliente.IdAz_Anagrafica != az_Anagrafica.Id)
                throw new UserMessageException(InfrastructureMessages.Get("CustomerNotFound"));
            return cliente;
        }

        private async Task<Az_ClienteContatto?> ContattoDellAzienda(int id)
        {
            if (id <= 0)
                return null;
            var contatto = await _az_ClienteContattoRepository.FindByIdAsync(id);
            if (contatto == null)
                return null;
            await ClienteDellAzienda(contatto.IdAz_Cliente); // eccezione se il cliente non e' dell'azienda
            return contatto;
        }
    }

    public interface IAz_ClienteContattoService : IServiceBase
    {
        Task<GenericResult<Az_ClienteContatto_GetAll_OutModel>> GetAll(GenericRequest<Az_ClienteContatto_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteContattoGetOutModel>> Az_ClienteContattoGet(GenericRequest<Az_ClienteContattoGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteContattoPutOutModel>> Az_ClienteContattoPut(GenericRequest<Az_ClienteContattoPutInModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteContattoDeleteOutModel>> Az_ClienteContattoDelete(GenericRequest<Az_ClienteContattoDeleteInModel> model, bool isSubProcess);
    }
}
