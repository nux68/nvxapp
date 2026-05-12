using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.Mappers.Public;
using nvxapp.server.service.ServerModels;
using System.Data;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService
{

    public class Dip_AnagraficaService : ServiceBase, IDip_AnagraficaService
    {
        private readonly IAccountService _accountService;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;

        private readonly IDip_RapportoLavoroService                              _dip_RapportoLavoroService;
        private readonly IDip_ProfiloOrarioService                               _dip_ProfiloOrarioService;
        private readonly IContatoriService                                       _contatoriService;
        private readonly IDip_Rapporto_Giustificativi_MaturazioneService         _maturazioneService;
        
        

        private readonly IUserCompanyRepository _userCompanyRepository;

        public Dip_AnagraficaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_RapportoLavoroService dip_RapportoLavoroService,
                                  IDip_ProfiloOrarioService dip_ProfiloOrarioService,
                                  IContatoriService contatoriService,
                                  IDip_Rapporto_Giustificativi_MaturazioneService maturazioneService,
                                  IUserCompanyRepository userCompanyRepository,
                                  IAspNetUserRolesRepository aspNetUserRolesRepository,
                                  IAspNetRolesRepository aspNetRolesRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAccountService accountService,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _accountService = accountService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _userCompanyRepository = userCompanyRepository;
            _dip_RapportoLavoroService = dip_RapportoLavoroService;
            _dip_ProfiloOrarioService  = dip_ProfiloOrarioService;
            _contatoriService          = contatoriService;
            _maturazioneService        = maturazioneService;
        }

        public virtual async Task<GenericResult<Dip_Anagrafica_GetAll_OutModel>> GetAll(GenericRequest<Dip_Anagrafica_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_Anagrafica_GetAll_OutModel retVal = new Dip_Anagrafica_GetAll_OutModel();

                GenericRequest<UserCompanyListInModel> req = new GenericRequest<UserCompanyListInModel>();
                var res = await _accountService.UserCompanyList(req, true);
                if (res.Success && res.Data != null)
                {
                    var UserCompanyList = res.Data.UserCompanyList;
                    List<string?> idAspNetUsers = UserCompanyList.Select(x => x.IdAspNetUsers).ToList();

                    // TODO eliminare a regime
                    foreach (var item in idAspNetUsers)
                    {
                        if (item != null)
                            await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(item, true);
                    }

                    var aspNetRoles = _aspNetRolesRepository.FindAll( x=>x.Id != null   ).ToList();


                    List<Dip_Anagrafica> dip_Anagrafica = _dip_AnagraficaRepository.FindAll(x => idAspNetUsers.Contains(x.IdAspNetUsers)).ToList();
                    List<Dip_RapportoLavoro> dip_RapportoLavoro = _dip_RapportoLavoroRepository.FindAll(x => dip_Anagrafica.Select(x => x.Id).ToList().Contains(x.IdDip_Anagrafica)).ToList();
                    foreach (var item in dip_Anagrafica)
                    {


                        var applicationUser = await _userManager.FindByIdAsync(item.IdAspNetUsers);
                        if (applicationUser != null)
                        {
                            var usrRoles = new List<string>(await _userManager.GetRolesAsync(applicationUser));
                            retVal.Dip_Anagrafica.Add(new Dip_AnagraficaModel()
                            {
                                UserName = applicationUser.UserName != null ? applicationUser.UserName : string.Empty,
                                IdAspNetUsers = item.IdAspNetUsers,
                                Cognome = item.Cognome,
                                Nome = item.Nome,
                                Id = item.Id,
                                Dip_RapportoLavoro = _mapper.Map<List<Dip_RapportoLavoroModel>>(dip_RapportoLavoro.Where(x => x.IdDip_Anagrafica == item.Id).ToList()),
                                RoleCode = aspNetRoles.Where(x=> x.Name!= null && usrRoles.Contains(x.Name)).Select(x=> x.Code).ToList()
                                
                            });

                            retVal.Dip_Anagrafica = retVal.Dip_Anagrafica.OrderBy(x => x.Cognome ?? string.Empty)
                                                                         .ThenBy(x => x.Nome ?? string.Empty)
                                                                         .ToList();
                                                                    }

                    }
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<Dip_Anagrafica_Get_OutModel>> Dip_AnagraficaGet(GenericRequest<Dip_Anagrafica_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Anagrafica_Get_OutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var applicationUser = await _userManager.FindByIdAsync(model.Data.Id);
                    if( applicationUser != null )
                    {

                        

                        var aspNetRoles = _aspNetRolesRepository.FindAll( x=>x.Id != null ).ToList();
                        var usrRoles = new List<string>(await _userManager.GetRolesAsync(applicationUser));

                        var dip_Anagrafica = _dip_AnagraficaRepository.FindAll( x=> x.IdAspNetUsers == model.Data.Id).FirstOrDefault();
                        if( dip_Anagrafica != null )
                        {
                            retVal.Dip_Anagrafica = _mapper.Map<Dip_Anagrafica4EditModel>(dip_Anagrafica);
                            retVal.Dip_Anagrafica.RoleCode = aspNetRoles.Where(x=> x.Name!= null && usrRoles.Contains(x.Name)).Select(x=> x.Code).ToList();
                            retVal.Dip_Anagrafica.UserName = applicationUser.UserName != null ? applicationUser.UserName : "";
                            retVal.Dip_Anagrafica.Mail = applicationUser.Email != null ? applicationUser.Email : "";

                            var usrC =  _userCompanyRepository.FindAll(x => x.IdAspNetUsers == model.Data.Id && x.IdCompany == IdCompany).FirstOrDefault();

                            if ( usrC != null )
                            {
                                retVal.Dip_Anagrafica.MainUser = usrC.MainUser;

                                var req_2 = new GenericRequest<UserCompanyGetInModel>();
                                req_2.Data =  new UserCompanyGetInModel() { Id= usrC.Id };

                                var res_2 = await _accountService.UserCompanyGet(req_2, true);
                                if(res_2.Success && res_2.Data != null)
                                {
                                    retVal.Dip_Anagrafica.Descrizione = !string.IsNullOrEmpty(applicationUser.UserName)? applicationUser.UserName:"";
                                    retVal.Dip_Anagrafica.IdUserCompany = IdCompany;
                                    retVal.Dip_Anagrafica.RoleId =res_2.Data.UserCompanyEdit.RoleId;
                                    retVal.Dip_Anagrafica.Roles = res_2.Data.UserCompanyEdit.Roles;  
                                }

                                var req_1 = new GenericRequest<Dip_RapportoLavoro_Get_InModel>();
                                req_1.Data.Id = retVal.Dip_Anagrafica.Id; 
                                var res_1 = await _dip_RapportoLavoroService.Dip_RapportoLavoroGet(req_1, true);
                                if(res_1.Success && res_1.Data != null)
                                {
                                    retVal.Dip_Anagrafica.Dip_RapportoLavoro = res_1.Data.Dip_RapportoLavoro;
                                }

                                foreach(var item in retVal.Dip_Anagrafica.Dip_RapportoLavoro)
                                {
                                    var req_3 = new GenericRequest<Dip_ProfiloOrario_Get_InModel>();
                                        req_3.Data.Id = item.Id; 
                                        var res_3 = await _dip_ProfiloOrarioService.Dip_ProfiloOrarioGet(req_3, true);
                                        if(res_3.Success && res_3.Data != null)
                                        {
                                            retVal.Dip_Anagrafica.Dip_ProfiloOrario.AddRange( res_3.Data.Dip_ProfiloOrario);
                                        }
                                    }

                                    // ── Riporti (mese 0) per 5 anni ──────────────────────────────────
                                    int annoCorrente = DateTime.Now.Year;
                                    foreach (var rappLav in retVal.Dip_Anagrafica.Dip_RapportoLavoro)
                                    {
                                        var reqRip = new GenericRequest<Contatori_Riporto_GetAll_InModel>();
                                        for (int a = annoCorrente - 4; a <= annoCorrente; a++)
                                        {
                                            reqRip.Data.IdDip_RapportoLavoro = rappLav.Id;
                                            reqRip.Data.Anno                 = a;
                                            var resRip = await _contatoriService.Riporto_GetAll(reqRip, true);
                                            if (resRip.Success && resRip.Data != null)
                                                retVal.Dip_Anagrafica.Dip_Contatori_Riporto.AddRange(resRip.Data.Riporti);
                                        }

                                        // ── Maturazione per rapporto ─────────────────────────────────
                                        var reqMat = new GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_GetAll_InModel>();
                                        reqMat.Data.IdDip_RapportoLavoro = rappLav.Id;
                                        var resMat = await _maturazioneService.GetAll(reqMat, true);
                                        if (resMat.Success && resMat.Data != null)
                                            retVal.Dip_Anagrafica.Dip_Maturazione.AddRange(resMat.Data.Maturazioni);
                                    }
                                

                            }
                        }
                    }
                }
                await Task.Delay(DelayAsyncMethod);
                
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_Anagrafica_Put_OutModel>> Dip_AnagraficaPut(GenericRequest<Dip_Anagrafica_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_Anagrafica_Put_OutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {


                    var req_UsrComp = new GenericRequest<UserCompanyPutInModel>();
                    req_UsrComp.Data = new UserCompanyPutInModel()
                    {
                        UserCompanyEdit = new UserCompanyEditModel()
                        {
                            IdUserCompany = model.Data.Dip_Anagrafica.IdUserCompany,
                            Descrizione = !string.IsNullOrEmpty(model.Data.Dip_Anagrafica.Descrizione) ? model.Data.Dip_Anagrafica.Descrizione : "",
                            MainUser = model.Data.Dip_Anagrafica.MainUser,
                            Mail = model.Data.Dip_Anagrafica.Mail,
                            Pw = model.Data.Dip_Anagrafica.Pw,
                            RoleId = model.Data.Dip_Anagrafica.RoleId,
                            Roles = model.Data.Dip_Anagrafica.Roles
                        }
                    };
    
                    var res_UsrComp = await _accountService.UserCompanyPut(req_UsrComp, true);
                    if(res_UsrComp.Success && res_UsrComp.Data != null)
                    {
                        var usrC =  await _userCompanyRepository.FindByIdAsync(res_UsrComp.Data.UserCompanyEdit.IdUserCompany);

                        if(usrC != null )
                        {
                            var applicationUser = await _userManager.FindByIdAsync(usrC.IdAspNetUsers);
                            if( applicationUser != null )
                            {

                                var aspNetRoles = _aspNetRolesRepository.FindAll(x => x.Id != null).ToList();
                                var usrRoles = new List<string>(await _userManager.GetRolesAsync(applicationUser));
                                var entity = _dip_AnagraficaRepository.FindAll(x => x.IdAspNetUsers == applicationUser.Id).FirstOrDefault();

                                if(entity == null)
                                {
                                    entity = _mapper.Map<Dip_Anagrafica>(model.Data.Dip_Anagrafica);
                                    entity.IdAspNetUsers = applicationUser.Id;
                                    entity.Id = 0 ;
                                    entity.Dip_RapportoLavoro = null;
                                    entity = await _dip_AnagraficaRepository.UpsertAsync(entity);

                                    foreach(var item in model.Data.Dip_Anagrafica.Dip_RapportoLavoro)
                                        item.IdDip_Anagrafica = entity.Id;
                                }
                                else
                                {
                                    entity = _mapper.Map<Dip_Anagrafica>(model.Data.Dip_Anagrafica);
                                    entity = await _dip_AnagraficaRepository.UpsertAsync(entity);
                                }


                                var req_1 = new GenericRequest<Dip_RapportoLavoro_Put_InModel>();
                                req_1.Data.Id = entity.Id;
                                req_1.Data.Dip_RapportoLavoro = model.Data.Dip_Anagrafica.Dip_RapportoLavoro;
                                var res_1 = await _dip_RapportoLavoroService.Dip_RapportoLavoroPut(req_1, true);
                                if (res_1.Success && res_1.Data != null)
                                {
                                    retVal.Dip_Anagrafica.Dip_RapportoLavoro = res_1.Data.Dip_RapportoLavoro;

                                    


                                    foreach (var itemRapp in retVal.Dip_Anagrafica.Dip_RapportoLavoro)
                                    {
                                        var req_3 = new GenericRequest<Dip_ProfiloOrario_Put_InModel>();
                                        req_3.Data.Id = itemRapp.Id;

                                        //se è un nuovo rapporto lavoro, i nuovi profili orari verranno agganciati a quello
                                        if( model.Data.Dip_Anagrafica.Dip_RapportoLavoro.Where(x=> x.Id<0).Any()   )
                                        {
                                            foreach(var itemProfHH in model.Data.Dip_Anagrafica.Dip_ProfiloOrario.Where(x=> x.IdDip_RapportoLavoro<0))
                                                itemProfHH.IdDip_RapportoLavoro = itemRapp.Id;
                                        }


                                        req_3.Data.Dip_ProfiloOrario = model.Data.Dip_Anagrafica.Dip_ProfiloOrario.Where(x => x.IdDip_RapportoLavoro == itemRapp.Id).ToList();
                                        var res_3 = await _dip_ProfiloOrarioService.Dip_ProfiloOrarioPut(req_3, true);
                                        if (res_3.Success && res_3.Data != null)
                                        {
                                            retVal.Dip_Anagrafica.Dip_ProfiloOrario = res_3.Data.Dip_ProfiloOrario;
                                        }

                                        // ── Salvataggio riporti (mese 0) ─────────────────────────────
                                        var riportiInInput = model.Data.Dip_Anagrafica.Dip_Contatori_Riporto
                                            .Where(r => r.IdDip_RapportoLavoro == itemRapp.Id)
                                            .ToList();

                                        foreach (var rip in riportiInInput)
                                        {
                                            rip.IdDip_RapportoLavoro = itemRapp.Id;
                                            var reqUpsert = new GenericRequest<Contatori_Riporto_Upsert_InModel>();
                                            reqUpsert.Data.Riporto = rip;
                                            await _contatoriService.Riporto_Upsert(reqUpsert, true);
                                        }

                                        // ── Salvataggio maturazione ──────────────────────────────────
                                        var maturazioniInInput = model.Data.Dip_Anagrafica.Dip_Maturazione
                                            .Where(m => m.IdDip_RapportoLavoro == itemRapp.Id)
                                            .ToList();

                                        foreach (var mat in maturazioniInInput)
                                        {
                                            mat.IdDip_RapportoLavoro = itemRapp.Id;
                                            var reqMat = new GenericRequest<Dip_Rapporto_Giustificativi_Maturazione_Upsert_InModel>();
                                            reqMat.Data.Maturazione = mat;
                                            await _maturazioneService.Upsert(reqMat, true);
                                        }
                                    }
                                } // end foreach itemRapp / end if res_1.Success

                            }
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }


    }

    public interface IDip_AnagraficaService : IServiceBase
    {
        public Task<GenericResult<Dip_Anagrafica_GetAll_OutModel>> GetAll(GenericRequest<Dip_Anagrafica_GetAll_InModel> model, Boolean isSubProcess);

        public Task<GenericResult<Dip_Anagrafica_Get_OutModel>> Dip_AnagraficaGet(GenericRequest<Dip_Anagrafica_Get_InModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_Anagrafica_Put_OutModel>> Dip_AnagraficaPut(GenericRequest<Dip_Anagrafica_Put_InModel> model, bool isSubProcess);

    }

  

}
