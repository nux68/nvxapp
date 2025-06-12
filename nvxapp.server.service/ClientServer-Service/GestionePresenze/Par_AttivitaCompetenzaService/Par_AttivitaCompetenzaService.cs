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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService
{
    public class Par_AttivitaCompetenzaService : ServiceBase, IPar_AttivitaCompetenzaService
    {
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_AttivitaCompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Par_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaCompetenza_GetAll_OutModel retVal = new Par_AttivitaCompetenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_AttivitaCompetenza = await _par_AttivitaCompetenzaRepository.FindAll();
                    retVal.Par_AttivitaCompetenza = _mapper.Map<List<Par_AttivitaCompetenzaModel>>(par_AttivitaCompetenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    
        public virtual async Task<GenericResult<Par_AttivitaCompetenza_Selected_GetOutModel>> GetSelected_On_Az_Par_Attivita(GenericRequest<Par_AttivitaCompetenza_Selected_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaCompetenza_Selected_GetOutModel retVal = new Par_AttivitaCompetenza_Selected_GetOutModel();


                var par_AttivitaCompetenza = _par_AttivitaCompetenzaRepository.FindAll(x => x.IdPar_Attivita == model.Data.IdPar_Attivita);

                retVal.Par_AttivitaCompetenza = _mapper.Map<List<Par_AttivitaCompetenzaModel>>(par_AttivitaCompetenza);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_AttivitaCompetenza_Selected_PutOutModel>> PutSelected_On_Az_Par_Attivita(GenericRequest<Par_AttivitaCompetenza_Selected_PutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaCompetenza_Selected_PutOutModel retVal = new Par_AttivitaCompetenza_Selected_PutOutModel();

                //cancellazione
                var Par_AttivitaCompetenza = _par_AttivitaCompetenzaRepository.FindAll(x => x.IdPar_Attivita == model.Data.IdPar_Attivita).ToList();

                if (Par_AttivitaCompetenza != null)
                {
                    foreach (var item in Par_AttivitaCompetenza)
                    {
                        // ciclo i dati a db, se non presente nella lista tornata dal client, allora è stata cancellato
                        // e lo elimino dal db
                        var rec = model.Data.Par_AttivitaCompetenza.Where(x => x.IdPar_Competenza == item.IdPar_Competenza && 
                                                                               x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                        if (rec == null)
                        {
                            await _par_AttivitaCompetenzaRepository.DeleteAsync(item);
                        }
                    }
                }
                //aggiornamento
                foreach (var item in model.Data.Par_AttivitaCompetenza)
                {
                    var az_SediAttivita = _par_AttivitaCompetenzaRepository.FindAll(x => x.IdPar_Competenza == item.IdPar_Competenza &&  
                                                                                    x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                    if (az_SediAttivita == null)
                    {
                        az_SediAttivita = new Par_AttivitaCompetenza() { IdPar_Competenza = item.IdPar_Competenza, IdPar_Attivita = item.IdPar_Attivita };
                    }
                    await _par_AttivitaCompetenzaRepository.UpsertAsync(az_SediAttivita);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    
        
        }

    public interface IPar_AttivitaCompetenzaService : IServiceBase
    {
        Task<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Par_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess);

        public Task<GenericResult<Par_AttivitaCompetenza_Selected_GetOutModel>> GetSelected_On_Az_Par_Attivita(GenericRequest<Par_AttivitaCompetenza_Selected_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Par_AttivitaCompetenza_Selected_PutOutModel>> PutSelected_On_Az_Par_Attivita(GenericRequest<Par_AttivitaCompetenza_Selected_PutInModel> model, Boolean isSubProcess);
    }
}
