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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService
{
    public class Az_SediAttivitaService : ServiceBase, IAz_SediAttivitaService
    {
        private readonly IAz_SediAttivitaRepository _az_SediAttivitaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SediAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediAttivitaRepository az_SediAttivitaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediAttivitaRepository = az_SediAttivitaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SediAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SediAttivita_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediAttivita_GetAll_OutModel retVal = new Az_SediAttivita_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SediAttivita = await _az_SediAttivitaRepository.FindAll();
                    retVal.Az_SediAttivita = _mapper.Map<List<Az_SediAttivitaModel>>(az_SediAttivita);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediAttivita_Selected_GetOutModel>> GetSelected_On_Az_Sedi(GenericRequest<Az_SediAttivita_Selected_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediAttivita_Selected_GetOutModel retVal = new Az_SediAttivita_Selected_GetOutModel();


                var az_SediAttivita = _az_SediAttivitaRepository.FindAll(x => x.IdAz_Sedi == model.Data.IdAz_Sedi);

                retVal.Az_SediAttivita = _mapper.Map<List<Az_SediAttivitaModel>>(az_SediAttivita);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediAttivita_Selected_PutOutModel>> PutSelected_On_Az_Sedi(GenericRequest<Az_SediAttivita_Selected_PutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediAttivita_Selected_PutOutModel retVal = new Az_SediAttivita_Selected_PutOutModel();

                //cancellazione
                var az_SediAttivitaList = _az_SediAttivitaRepository.FindAll(x => x.IdAz_Sedi == model.Data.IdAz_Sedi).ToList();

                if (az_SediAttivitaList != null)
                {
                    foreach (var item in az_SediAttivitaList)
                    {
                        // ciclo i dati a db, se non presente nella lista tornata dal client, allora è stata cancellato
                        // e lo elimino dal db
                        var rec = model.Data.Az_SediAttivita.Where(x => x.IdAz_Sedi == item.IdAz_Sedi &&
                                                                        x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                        if (rec == null)
                        {
                            await _az_SediAttivitaRepository.DeleteAsync(item);
                        }
                    }
                }
                //aggiornamento
                foreach (var item in model.Data.Az_SediAttivita)
                {
                    var az_SediAttivita = _az_SediAttivitaRepository.FindAll(x => x.IdAz_Sedi == item.IdAz_Sedi &&
                                                                                  x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                    if (az_SediAttivita == null)
                    {
                        az_SediAttivita = new Az_SediAttivita() { IdAz_Sedi = item.IdAz_Sedi, IdPar_Attivita = item.IdPar_Attivita };
                    }
                    await _az_SediAttivitaRepository.UpsertAsync(az_SediAttivita);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediAttivitaService : IServiceBase
    {
        Task<GenericResult<Az_SediAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SediAttivita_GetAll_InModel> model, bool isSubProcess);
        public Task<GenericResult<Az_SediAttivita_Selected_GetOutModel>> GetSelected_On_Az_Sedi(GenericRequest<Az_SediAttivita_Selected_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediAttivita_Selected_PutOutModel>> PutSelected_On_Az_Sedi(GenericRequest<Az_SediAttivita_Selected_PutInModel> model, Boolean isSubProcess);

    }
}
