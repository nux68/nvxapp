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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService
{
    public class Az_SubCommessaService : ServiceBase, IAz_SubCommessaService
    {
        private readonly IAz_SubCommessaRepository _az_SubCommessaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;

        private readonly IAz_SubCommessaUserService _az_SubCommessaUserService;
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;
        private readonly IAz_SubCommessaSediRepartoService _az_SubCommessaSediRepartoService;

        private readonly IAz_SediRepository _az_SediRepository;
        private readonly IAz_SediRepartoRepository _az_SediRepartoRepository;
        private readonly IAz_SediRepartoAttivitaRepository _az_SediRepartoAttivitaRepository;
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_CompetenzaRepository _dip_CompetenzaRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;

        public Az_SubCommessaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SubCommessaUserService az_SubCommessaUserService,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaUserRepository az_SubCommessaUserRepository,
                                  IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository,
                                  IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository,
                                  IAz_SubCommessaRepository az_SubCommessaRepository,
                                  IAz_SubCommessaSediRepartoService az_SubCommessaSediRepartoService,
                                  IAz_SediRepository az_SediRepository,
                                  IAz_SediRepartoRepository az_SediRepartoRepository,
                                  IAz_SediRepartoAttivitaRepository az_SediRepartoAttivitaRepository,
                                  IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IDip_CompetenzaRepository dip_CompetenzaRepository,
                                  IUserCompanyRepository userCompanyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaRepository = az_SubCommessaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;

            _az_SubCommessaUserService = az_SubCommessaUserService;
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
            _az_SubCommessaSediRepartoService = az_SubCommessaSediRepartoService;

            _az_SediRepository = az_SediRepository;
            _az_SediRepartoRepository = az_SediRepartoRepository;
            _az_SediRepartoAttivitaRepository = az_SediRepartoAttivitaRepository;
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_CompetenzaRepository = dip_CompetenzaRepository;
            _userCompanyRepository = userCompanyRepository;
        }

        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_OutModel retVal = new Az_SubCommessa_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = (await _az_SubCommessaRepository.FindAll()).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessaModel>>(az_SubCommessa);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_4Edit_OutModel retVal = new Az_SubCommessa_GetAll_4Edit_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == model.Data.Id).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessa_4EditModel>>(az_SubCommessa);
                    if (retVal.Az_SubCommessa != null)
                    {
                        foreach (var itemSubCommessa in retVal.Az_SubCommessa)
                        {
                            // Az_SubCommessaUser assignment
                            var req_SubCommessaUser = new GenericRequest<Az_SubCommessaUser_Get4SubCommessa_InModel>();
                            req_SubCommessaUser.Data.IdAz_SubCommessa = itemSubCommessa.Id;
                            var res_SubCommessaUser = await _az_SubCommessaUserService.Get4Commessa(req_SubCommessaUser, true);
                            if (res_SubCommessaUser.Success && res_SubCommessaUser.Data != null)
                                itemSubCommessa.Az_SubCommessaUser = res_SubCommessaUser.Data.Az_SubCommessaUser;

                            // Az_SubCommessaAttivita assignment
                            var req_SubCommessaAttivita = new GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel>();
                            req_SubCommessaAttivita.Data.IdAz_SubCommessa = itemSubCommessa.Id;
                            var res_SubCommessaAttivita = await _az_SubCommessaAttivitaService.Get4SubCommessa(req_SubCommessaAttivita, true);
                            if (res_SubCommessaAttivita.Success && res_SubCommessaAttivita.Data != null)
                                itemSubCommessa.Az_SubCommessaAttivita = res_SubCommessaAttivita.Data.Az_SubCommessaAttivita;

                            // Az_SubCommessaSediReparto assignment (chiamata service invece di repository)
                            var req_SubCommessaSediReparto = new GenericRequest<Az_SubCommessaSediReparto_Get4SubCommessa_InModel>();
                            req_SubCommessaSediReparto.Data.IdAz_SubCommessa = itemSubCommessa.Id;
                            var res_SubCommessaSediReparto = await _az_SubCommessaSediRepartoService.Get4SubCommessa(req_SubCommessaSediReparto, true);
                            if (res_SubCommessaSediReparto.Success && res_SubCommessaSediReparto.Data != null)
                                itemSubCommessa.Az_SubCommessaSediReparto = res_SubCommessaSediReparto.Data.Az_SubCommessaSediReparto;
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_PutAll_4Edit_OutModel retVal = new Az_SubCommessa_PutAll_4Edit_OutModel();
                retVal.Id = model.Data.Id;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    //rileggo i dati originali
                    var reqAz_Sub = new GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel>();
                    reqAz_Sub.Data.Id = model.Data.Id;

                    var resAz_Sub = await GetAll_4Edit(reqAz_Sub, true);
                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        //cancellazione sub commesse eliminate
                        foreach (var item in resAz_Sub.Data.Az_SubCommessa)
                        {   //ciclo le commesse originali

                            //ottengo il record orig del db
                            Az_SubCommessa? az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();

                            if (az_SubCommessa != null)
                            {
                                //cerco la commessa nei dati tornati dal client
                                var orig_TMP = model.Data.Az_SubCommessa.Where(x => x.Id == item.Id).FirstOrDefault();

                                //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                                if (orig_TMP == null)
                                {
                                    //procedo alla cancelazione
                                    await _az_SubCommessaRepository.DeleteAsync(az_SubCommessa);
                                }
                            }
                        }
                        //upsert commesse
                        foreach (var item in model.Data.Az_SubCommessa)
                        {
                            //ottengo il record orig del db
                            Az_SubCommessa? az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                            if (az_SubCommessa == null)
                            {
                                az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                                az_SubCommessa.IdAz_Commessa = model.Data.Id;
                                az_SubCommessa.Id = 0;
                            }
                            else
                            {
                                az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                            }
                            az_SubCommessa = await _az_SubCommessaRepository.UpsertAsync(az_SubCommessa);

                            var req_SubCommessaUser = new GenericRequest<Az_SubCommessaUser_Put4SubCommessa_InModel>();
                            req_SubCommessaUser.Data.IdAz_SubCommessa = az_SubCommessa.Id;
                            req_SubCommessaUser.Data.Az_SubCommessaUser = item.Az_SubCommessaUser;
                            var res_SubCommessaUser = await _az_SubCommessaUserService.Put4Commessa(req_SubCommessaUser, true);

                            var req_SubCommessaAttivita = new GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel>();
                            req_SubCommessaAttivita.Data.IdAz_SubCommessa = az_SubCommessa.Id;
                            req_SubCommessaAttivita.Data.Az_SubCommessaAttivita = item.Az_SubCommessaAttivita;
                            var res_SubCommessaAttivita = await _az_SubCommessaAttivitaService.Put4SubCommessa(req_SubCommessaAttivita, true);

                            var req_SubCommessaSediReparto = new GenericRequest<Az_SubCommessaSediReparto_Put4SubCommessa_InModel>();
                            req_SubCommessaSediReparto.Data.IdAz_SubCommessa = az_SubCommessa.Id;
                            req_SubCommessaSediReparto.Data.Az_SubCommessaSediReparto = item.Az_SubCommessaSediReparto;
                            await _az_SubCommessaSediRepartoService.Put4SubCommessa(req_SubCommessaSediReparto, true);
                        }

                        //rileggo i dati dopo le varizioni per ritornare il valore corrente
                        resAz_Sub = await GetAll_4Edit(reqAz_Sub, true);
                        if (resAz_Sub.Success && resAz_Sub.Data != null)
                        {
                            retVal.Az_SubCommessa = resAz_Sub.Data.Az_SubCommessa;
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // Reparti e utenti che si possono collegare a una sub commessa con le attivita' indicate:
        // - reparto: vi si svolge almeno una delle attivita'
        // - utente: ha almeno una delle competenze richieste da almeno una delle attivita'
        //   (un'attivita' senza competenze richieste puo' essere svolta da tutti)
        public virtual async Task<GenericResult<Az_SubCommessa_GetCompatibili_OutModel>> GetCompatibili(GenericRequest<Az_SubCommessa_GetCompatibili_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetCompatibili_OutModel retVal = new Az_SubCommessa_GetCompatibili_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var idPar_Attivita = model.Data.IdPar_Attivita.Distinct().ToList();

                    // reparti dell'azienda in cui si svolge almeno una delle attivita'
                    var idAz_Sedi = _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).Select(x => x.Id).ToList();
                    var idAz_SediReparto = _az_SediRepartoRepository.FindAll(x => idAz_Sedi.Contains(x.IdAz_Sedi)).Select(x => x.Id).ToList();
                    retVal.IdAz_SediReparto = _az_SediRepartoAttivitaRepository.FindAll(x => idAz_SediReparto.Contains(x.IdAz_SediReparto) &&
                                                                                            idPar_Attivita.Contains(x.IdPar_Attivita))
                                                                               .Select(x => x.IdAz_SediReparto)
                                                                               .Distinct()
                                                                               .ToList();

                    // competenze richieste da ciascuna attivita'
                    var attivitaCompetenza = _par_AttivitaCompetenzaRepository.FindAll(x => idPar_Attivita.Contains(x.IdPar_Attivita)).ToList();
                    var competenzeRichieste = idPar_Attivita.Select(id => attivitaCompetenza.Where(x => x.IdPar_Attivita == id)
                                                                                            .Select(x => x.IdPar_Competenza)
                                                                                            .ToList())
                                                            .ToList();

                    // utenti dell'azienda con almeno una competenza di almeno una attivita'
                    var idAspNetUsers = _userCompanyRepository.FindAll(x => x.IdCompany == IdCompany).Select(x => x.IdAspNetUsers).ToList();
                    var dip_Anagrafica = _dip_AnagraficaRepository.FindAll(x => idAspNetUsers.Contains(x.IdAspNetUsers)).ToList();
                    var idDip_Anagrafica = dip_Anagrafica.Select(x => x.Id).ToList();
                    var dip_Competenza = _dip_CompetenzaRepository.FindAll(x => idDip_Anagrafica.Contains(x.IdDip_Anagrafica)).ToList();

                    foreach (var dip in dip_Anagrafica)
                    {
                        var competenzeDip = dip_Competenza.Where(x => x.IdDip_Anagrafica == dip.Id).Select(x => x.IdPar_Competenza).ToList();
                        if (competenzeRichieste.Any(richieste => richieste.Count == 0 || richieste.Any(c => competenzeDip.Contains(c))))
                            retVal.IdAspNetUsers.Add(dip.IdAspNetUsers);
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_GetCompatibili_OutModel>> GetCompatibili(GenericRequest<Az_SubCommessa_GetCompatibili_InModel> model, bool isSubProcess);
    }
}
