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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService
{

    public class Az_SediRepartoAttivitaService : ServiceBase, IAz_SediRepartoAttivitaService
    {
        private readonly IAz_SediRepartoAttivitaRepository _az_SediRepartoAttivitaRepository;

        public Az_SediRepartoAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepartoAttivitaRepository az_SediRepartoAttivitaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepartoAttivitaRepository = az_SediRepartoAttivitaRepository;
        }

        public virtual async Task<GenericResult<Az_SediRepartoAttivitaOutModel>> GetAll(GenericRequest<Az_SediRepartoAttivitaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoAttivitaOutModel retVal = new Az_SediRepartoAttivitaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediRepartoAttivita_Selected_GetOutModel>> GetSelected_On_Az_SediReparto(GenericRequest<Az_SediRepartoAttivita_Selected_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoAttivita_Selected_GetOutModel retVal = new Az_SediRepartoAttivita_Selected_GetOutModel();


                var az_SediRepartoAttivita = _az_SediRepartoAttivitaRepository.FindAll(x => x.IdAz_SediReparto == model.Data.IdAz_SediReparto);

                retVal.Az_SediRepartoAttivita = _mapper.Map<List<Az_SediRepartoAttivitaModel>>(az_SediRepartoAttivita);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediRepartoAttivita_Selected_PutOutModel>> PutSelected_On_Az_SediReparto(GenericRequest<Az_SediRepartoAttivita_Selected_PutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoAttivita_Selected_PutOutModel retVal = new Az_SediRepartoAttivita_Selected_PutOutModel();

                //cancellazione
                var az_SediRepartoAttivita = _az_SediRepartoAttivitaRepository.FindAll(x => x.IdAz_SediReparto == model.Data.IdAz_SediReparto).ToList();

                if (az_SediRepartoAttivita != null)
                {
                    foreach (var item in az_SediRepartoAttivita)
                    {
                        // ciclo i dati a db, se non presente nella lista tornata dal client, allora è stata cancellato
                        // e lo elimino dal db
                        var rec = model.Data.Az_SediRepartoAttivita.Where(x => x.IdAz_SediReparto == item.IdAz_SediReparto && 
                                                                               x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                        if (rec == null)
                        {
                            await _az_SediRepartoAttivitaRepository.DeleteAsync(item);
                        }
                    }
                }
                //aggiornamento
                foreach (var item in model.Data.Az_SediRepartoAttivita)
                {
                    var az_SediAttivita = _az_SediRepartoAttivitaRepository.FindAll(x => x.IdAz_SediReparto == item.IdAz_SediReparto && 
                                                                                    x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                    if (az_SediAttivita == null)
                    {
                        az_SediAttivita = new Az_SediRepartoAttivita() { IdAz_SediReparto = item.IdAz_SediReparto, IdPar_Attivita = item.IdPar_Attivita };
                    }
                    await _az_SediRepartoAttivitaRepository.UpsertAsync(az_SediAttivita);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SediRepartoAttivitaService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoAttivitaOutModel>> GetAll(GenericRequest<Az_SediRepartoAttivitaInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoAttivita_Selected_GetOutModel>> GetSelected_On_Az_SediReparto(GenericRequest<Az_SediRepartoAttivita_Selected_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoAttivita_Selected_PutOutModel>> PutSelected_On_Az_SediReparto(GenericRequest<Az_SediRepartoAttivita_Selected_PutInModel> model, Boolean isSubProcess);
    }
}
