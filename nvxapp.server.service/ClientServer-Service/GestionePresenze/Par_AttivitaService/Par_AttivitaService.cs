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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService
{
    public class Par_AttivitaService : ServiceBase, IPar_AttivitaService
    {
        private readonly IPar_AttivitaRepository _par_AttivitaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_CompetenzaRepository _par_CompetenzaRepository;
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;
        private readonly IPar_AttivitaCompetenzaService _par_AttivitaCompetenzaService;
        

        public Par_AttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IPar_AttivitaCompetenzaService par_AttivitaCompetenzaService,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_CompetenzaRepository par_CompetenzaRepository,
                                  IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository,
                                  IPar_AttivitaRepository par_AttivitaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_AttivitaRepository = par_AttivitaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_CompetenzaRepository = par_CompetenzaRepository;
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _par_AttivitaCompetenzaService = par_AttivitaCompetenzaService;
        }

        public virtual async Task<GenericResult<Par_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Par_Attivita_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_Attivita_GetAll_OutModel retVal = new Par_Attivita_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_Attivita = _par_AttivitaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Par_Attivita = _mapper.Map<List<Par_AttivitaModel>>(par_Attivita);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_AttivitaGetOutModel>> Par_AttivitaGet(GenericRequest<Par_AttivitaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaGetOutModel retVal = new Par_AttivitaGetOutModel();
                var par_Attivita = await _par_AttivitaRepository.FindByIdAsync(model.Data.Id);
                if (par_Attivita != null)
                {
                    retVal.Par_Attivita = _mapper.Map<Par_AttivitaModel>(par_Attivita);
                }
                else
                {
                    retVal.Par_Attivita = new Par_AttivitaModel(){ BackgroundColor  = "#ff0000",TextColor="#ff0000" };
                }


                GenericRequest<Par_AttivitaCompetenza_Selected_GetInModel> par_AttivitaCompetenza_Selected_GetInModel = new GenericRequest<Par_AttivitaCompetenza_Selected_GetInModel>();
                par_AttivitaCompetenza_Selected_GetInModel.Data.IdPar_Attivita = retVal.Par_Attivita.Id;

                var res2 = await _par_AttivitaCompetenzaService.GetSelected_On_Az_Par_Attivita(par_AttivitaCompetenza_Selected_GetInModel, true);
                if (res2.Success && res2.Data != null)
                {
                    res2.Data.Par_AttivitaCompetenza.ForEach(item =>
                    {
                        retVal.Par_Competenza.Add(new CheckObjOn_Id_Number()
                        {
                            Id = item.IdPar_Competenza,
                            Checked = true
                        });
                    });
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_AttivitaPutOutModel>> Par_AttivitaPut(GenericRequest<Par_AttivitaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaPutOutModel retVal = new Par_AttivitaPutOutModel();
                retVal.Par_Attivita = model.Data.Par_Attivita;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Attivita = await _par_AttivitaRepository.FindByIdAsync(model.Data.Par_Attivita.Id);
                    if (az_Attivita == null)
                    {
                        az_Attivita = _mapper.Map<Par_Attivita>(model.Data.Par_Attivita);
                        az_Attivita.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Attivita = _mapper.Map<Par_Attivita>(model.Data.Par_Attivita);
                    }
                    az_Attivita = await _par_AttivitaRepository.UpsertAsync(az_Attivita);
                    retVal.Par_Attivita = _mapper.Map<Par_AttivitaModel>(az_Attivita);


                    if(retVal.Par_Attivita.Default)
                    {
                        if(model.Data.Par_Competenza.Where(x=>x.Checked==true).ToList().Count==0)
                        {
                            //TODO GESTIRE COMUNICAZIONE SERVER CON ERRORI != EXCEPTION
                            retVal.Messages.Add(new Message("L'attivita di default deve avere almeno una competenza", MessageType.Exception));
                            return retVal;
                        }
                    }


                    GenericRequest<Par_AttivitaCompetenza_Selected_PutInModel> par_AttivitaCompetenza_Selected_PutInModel = new GenericRequest<Par_AttivitaCompetenza_Selected_PutInModel>();
                    par_AttivitaCompetenza_Selected_PutInModel.Data.IdPar_Attivita = retVal.Par_Attivita.Id;

                    foreach (var item in model.Data.Par_Competenza.Where(x=>x.Checked).ToList())
                    {
                        par_AttivitaCompetenza_Selected_PutInModel.Data.Par_AttivitaCompetenza.Add(new Par_AttivitaCompetenzaModel()
                        {
                             IdPar_Competenza= item.Id,
                             IdPar_Attivita = retVal.Par_Attivita.Id
                        });
                    }
                    var res2 = await _par_AttivitaCompetenzaService.PutSelected_On_Az_Par_Attivita(par_AttivitaCompetenza_Selected_PutInModel, true);
                    if (res2.Success && res2.Data != null){}


                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_AttivitaDeleteOutModel>> Par_AttivitaDelete(GenericRequest<Par_AttivitaDeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaDeleteOutModel retVal = new Par_AttivitaDeleteOutModel();
                var par_Attivita = await _par_AttivitaRepository.FindByIdAsync(model.Data.Id);
                if (par_Attivita != null)
                {
                    await _par_AttivitaRepository.DeleteAsync(par_Attivita);
                    retVal.Par_Attivita = _mapper.Map<Par_AttivitaModel>(par_Attivita);
                }
                else
                {
                    retVal.AddMessage($"Attività con Id {model.Data.Id} non trovata.", MessageType.Warning);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_AttivitaService : IServiceBase
    {
        Task<GenericResult<Par_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Par_Attivita_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_AttivitaGetOutModel>> Par_AttivitaGet(GenericRequest<Par_AttivitaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_AttivitaPutOutModel>> Par_AttivitaPut(GenericRequest<Par_AttivitaPutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_AttivitaDeleteOutModel>> Par_AttivitaDelete(GenericRequest<Par_AttivitaDeleteInModel> model, bool isSubProcess);
    }
}
