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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService
{
    public class Az_AttivitaService : ServiceBase, IAz_AttivitaService
    {
        private readonly IAz_AttivitaRepository _az_AttivitaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_AttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_AttivitaRepository az_AttivitaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_AttivitaRepository = az_AttivitaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Az_Attivita_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Attivita_GetAll_OutModel retVal = new Az_Attivita_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Attivita = _az_AttivitaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Az_Attivita = _mapper.Map<List<Az_AttivitaModel>>(az_Attivita);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_AttivitaGetOutModel>> Az_AttivitaGet(GenericRequest<Az_AttivitaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_AttivitaGetOutModel retVal = new Az_AttivitaGetOutModel();
                var az_Attivita = await _az_AttivitaRepository.FindByIdAsync(model.Data.Id);
                if (az_Attivita != null)
                {
                    retVal.Az_Attivita = _mapper.Map<Az_AttivitaModel>(az_Attivita);
                }
                else
                {
                    retVal.Az_Attivita = new Az_AttivitaModel();
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_AttivitaPutOutModel>> Az_AttivitaPut(GenericRequest<Az_AttivitaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_AttivitaPutOutModel retVal = new Az_AttivitaPutOutModel();
                retVal.Az_Attivita = model.Data.Az_Attivita;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Attivita = await _az_AttivitaRepository.FindByIdAsync(model.Data.Az_Attivita.Id);
                    if (az_Attivita == null)
                    {
                        az_Attivita = _mapper.Map<Az_Attivita>(model.Data.Az_Attivita);
                        az_Attivita.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Attivita = _mapper.Map<Az_Attivita>(model.Data.Az_Attivita);
                    }
                    az_Attivita = await _az_AttivitaRepository.UpsertAsync(az_Attivita);
                    retVal.Az_Attivita = _mapper.Map<Az_AttivitaModel>(az_Attivita);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_AttivitaService : IServiceBase
    {
        Task<GenericResult<Az_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Az_Attivita_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_AttivitaGetOutModel>> Az_AttivitaGet(GenericRequest<Az_AttivitaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_AttivitaPutOutModel>> Az_AttivitaPut(GenericRequest<Az_AttivitaPutInModel> model, bool isSubProcess);
    }
}
