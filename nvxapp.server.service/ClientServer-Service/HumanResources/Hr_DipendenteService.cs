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
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Dipendenti visti da Risorse Umane (tabella comune Dip_Anagrafica): il modulo funziona anche senza Presenze e Documentale,
     quindi l'ufficio del personale ne gestisce qui i dati che servono (codice fiscale e matricola per i cedolini, date di
     assunzione e cessazione per le checklist, D6). Le scritture passano dal repository di Common.
     Account del dipendente (HR-ONB.3): creazione e disattivazione con i servizi esistenti degli utenti dell'azienda
     (IAccountService.UserCompanyGet/UserCompanyPut: limite utenti della licenza, ruoli, inizializzatori), senza duplicarne la logica.
    */
    public class Hr_DipendenteService : ServiceBase, IHr_DipendenteService
    {
        private static readonly Regex _codiceFiscale = new(@"^[A-Z0-9]{16}$|^[0-9]{11}$", RegexOptions.Compiled);

        private readonly IHrUtility _utility;
        private readonly IHrOnboardingCore _onboarding;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IDip_AnagraficaCommonRepository _Dip_AnagraficaCommon;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IAccountService _accountService;

        public Hr_DipendenteService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IHrUtility utility,
                                    IHrOnboardingCore onboarding,
                                    IHr_Dip_AnagraficaReadRepository dip_Anagrafica,
                                    IDip_AnagraficaCommonRepository dip_AnagraficaCommon,
                                    IUserCompanyRepository userCompanyRepository,
                                    IAccountService accountService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _onboarding = onboarding;
            _Dip_Anagrafica = dip_Anagrafica;
            _Dip_AnagraficaCommon = dip_AnagraficaCommon;
            _userCompanyRepository = userCompanyRepository;
            _accountService = accountService;
        }

        public virtual async Task<GenericResult<Hr_DipendenteListOutModel>> Hr_DipendenteList(GenericRequest<Hr_DipendenteListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var dip = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (!model.Data.SoloAttivi || x.Attivo))
                                               .Include(x => x.AspNetUsersNavigation).OrderBy(x => x.Cognome).ThenBy(x => x.Nome).ToListAsync();
                return new Hr_DipendenteListOutModel { Dipendenti = await ModelliAsync(ctx, dip) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_DipendenteGetOutModel>> Hr_DipendenteGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                return new Hr_DipendenteGetOutModel { Dipendente = model.Data.Id == 0 ? new Hr_DipendenteModel() : await ModelloAsync(ctx, model.Data.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_DipendentePutOutModel>> Hr_DipendentePut(GenericRequest<Hr_DipendentePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data.Dipendente;
                if (string.IsNullOrWhiteSpace(d.Cognome))
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeSurnameRequired"));
                var cf = string.IsNullOrWhiteSpace(d.CodiceFiscale) ? null : Regex.Replace(d.CodiceFiscale, @"\s+", string.Empty).ToUpperInvariant();
                if (cf != null && !_codiceFiscale.IsMatch(cf))
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeTaxCodeInvalid"));
                var matricola = string.IsNullOrWhiteSpace(d.Matricola) ? null : HrPdf.Taglia(d.Matricola.Trim(), 20);
                if (d.DataAssunzione != null && d.DataCessazione != null && d.DataCessazione.Value.Date < d.DataAssunzione.Value.Date)
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeDatesInvalid"));
                // codice fiscale e matricola unici tra i dipendenti dell'azienda (regola di Dip_Anagrafica)
                if (cf != null && await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Id != d.Id && x.CodiceFiscale == cf).AnyAsync())
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeTaxCodeDuplicated"));
                if (matricola != null && await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Id != d.Id && x.Matricola == matricola).AnyAsync())
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNumberDuplicated"));

                Dip_Anagrafica dip;
                if (d.Id != 0)
                    dip = await _Dip_AnagraficaCommon.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                          ?? throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));
                else
                {
                    dip = new Dip_Anagrafica { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                    await _Dip_AnagraficaCommon.CreateAsync(dip, false);
                }
                dip.Cognome = HrPdf.Taglia(d.Cognome.Trim(), 100);
                dip.Nome = string.IsNullOrWhiteSpace(d.Nome) ? null : HrPdf.Taglia(d.Nome.Trim(), 100);
                dip.CodiceFiscale = cf;
                dip.Matricola = matricola;
                dip.Email = string.IsNullOrWhiteSpace(d.Email) ? null : HrPdf.Taglia(d.Email.Trim(), 100);
                dip.DataAssunzione = d.DataAssunzione?.Date;
                dip.DataCessazione = d.DataCessazione?.Date;
                dip.Attivo = d.Attivo;
                dip.ModifiedDate = DateTime.Now;
                dip.ChangeUser = ctx.IdUtente;
                await _Dip_AnagraficaCommon.SaveChange();

                // date cambiate: checklist di assunzione o cessazione (D6)
                await _onboarding.SincronizzaAsync(ctx, dip.Id);
                return new Hr_DipendentePutOutModel { Dipendente = await ModelloAsync(ctx, dip.Id) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ account (HR-ONB.3)
        public virtual async Task<GenericResult<Hr_AccountOutModel>> Hr_AccountCrea(GenericRequest<Hr_AccountCreaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data;
                var dip = await _Dip_AnagraficaCommon.FindAll(x => x.Id == d.IdDip_Anagrafica && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                          ?? throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));
                if (!string.IsNullOrEmpty(dip.IdAspNetUsers))
                    throw new UserMessageException(HumanResourcesMessages.Get("AccountAlreadyPresent"));
                var userName = (d.UserName ?? string.Empty).Trim();
                if (userName.Length == 0 || string.IsNullOrWhiteSpace(d.Password))
                    throw new UserMessageException(HumanResourcesMessages.Get("AccountDataRequired"));
                if (await _userManager.FindByNameAsync(userName) != null)
                    throw new UserMessageException(HumanResourcesMessages.Get("AccountUserNameUsed"));

                // servizio esistente degli utenti dell'azienda: ruolo User, limite utenti della licenza, inizializzatori
                var put = await _accountService.UserCompanyPut(new GenericRequest<UserCompanyPutInModel>
                {
                    Data = new UserCompanyPutInModel
                    {
                        UserCompanyEdit = new UserCompanyEditModel
                        {
                            IdUserCompany = 0, Descrizione = userName, Mail = string.IsNullOrWhiteSpace(d.Mail) ? dip.Email : d.Mail.Trim(), Pw = d.Password,
                            Roles = new List<string> { nameof(RoleCode.User) }
                        }
                    }
                }, true);
                var idUtente = put.Data?.UserCompanyEdit?.IdAspNetUsers;
                if (!put.Success || string.IsNullOrEmpty(idUtente))
                    throw new UserMessageException(put.Messages?.FirstOrDefault()?.Text ?? HumanResourcesMessages.Get("AccountNotCreated"));

                // l'inizializzatore comune ha creato un'anagrafica per il nuovo utente: l'utente va invece sul dipendente esistente
                var doppia = await _Dip_AnagraficaCommon.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers == idUtente && x.Id != dip.Id)
                                                        .AsTracking().FirstOrDefaultAsync();
                if (doppia != null)
                {
                    doppia.IdAspNetUsers = null;
                    doppia.Attivo = false;
                    await _Dip_AnagraficaCommon.SaveChange();
                }
                dip.IdAspNetUsers = idUtente;
                dip.ModifiedDate = DateTime.Now;
                await _Dip_AnagraficaCommon.SaveChange();
                if (doppia != null)
                {
                    try
                    {
                        await _Dip_AnagraficaCommon.DeleteAsync(doppia);
                    }
                    catch (Exception ex)
                    {
                        // gia' usata da un altro applicativo (es. dati iniziali delle presenze): resta disattivata e senza utente
                        Log.Warning(ex, "[HR] Anagrafica {Id} creata per l'utente {User} non eliminata", doppia.Id, idUtente);
                    }
                }

                if (d.IdHr_ChecklistVoce is int idVoce)
                    await _onboarding.CompletaVoceAsync(ctx, idVoce);
                return new Hr_AccountOutModel { Account = (await ModelloAsync(ctx, dip.Id)).Account };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_AccountOutModel>> Hr_AccountDisattiva(GenericRequest<Hr_AccountDisattivaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var dip = await _utility.DipendenteAsync(ctx, model.Data.IdDip_Anagrafica);
                var uc = string.IsNullOrEmpty(dip.IdAspNetUsers) ? null
                         : await _userCompanyRepository.FindAll(x => x.IdAspNetUsers == dip.IdAspNetUsers && x.IdCompany == ctx.IdCompany).FirstOrDefaultAsync();
                if (uc == null)
                    throw new UserMessageException(HumanResourcesMessages.Get("AccountNotPresent"));
                if (dip.IdAspNetUsers == ctx.IdUtente)
                    throw new UserMessageException(HumanResourcesMessages.Get("AccountSelf"));

                if (!uc.Disabled)
                {
                    // servizio esistente: si rilegge l'utente (ruoli compresi) e si salva disabilitato
                    var get = await _accountService.UserCompanyGet(new GenericRequest<UserCompanyGetInModel> { Data = new UserCompanyGetInModel { Id = uc.Id } }, true);
                    var edit = get.Data?.UserCompanyEdit ?? throw new UserMessageException(HumanResourcesMessages.Get("AccountNotPresent"));
                    edit.Disabled = true;
                    edit.Mail = null;
                    await _accountService.UserCompanyPut(new GenericRequest<UserCompanyPutInModel> { Data = new UserCompanyPutInModel { UserCompanyEdit = edit } }, true);
                }

                if (model.Data.IdHr_ChecklistVoce is int idVoce)
                    await _onboarding.CompletaVoceAsync(ctx, idVoce);
                return new Hr_AccountOutModel { Account = (await ModelloAsync(ctx, dip.Id)).Account };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<Hr_DipendenteModel> ModelloAsync(HrContesto ctx, int id)
        {
            var dip = await _Dip_Anagrafica.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Include(x => x.AspNetUsersNavigation).FirstOrDefaultAsync()
                      ?? throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));
            return (await ModelliAsync(ctx, new List<Dip_Anagrafica> { dip })).First();
        }

        private async Task<List<Hr_DipendenteModel>> ModelliAsync(HrContesto ctx, List<Dip_Anagrafica> dip)
        {
            var utenti = dip.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).ToList();
            var disabilitati = await _userCompanyRepository.FindAll(x => x.IdCompany == ctx.IdCompany && utenti.Contains(x.IdAspNetUsers) && x.Disabled)
                                                           .Select(x => x.IdAspNetUsers).ToListAsync();
            return dip.Select(x => new Hr_DipendenteModel
            {
                Id = x.Id, Cognome = x.Cognome, Nome = x.Nome, Matricola = x.Matricola, CodiceFiscale = x.CodiceFiscale, Email = x.Email,
                DataAssunzione = x.DataAssunzione, DataCessazione = x.DataCessazione, Attivo = x.Attivo,
                Account = new Hr_AccountModel
                {
                    Presente = x.IdAspNetUsers != null, UserName = x.AspNetUsersNavigation?.UserName, Mail = x.AspNetUsersNavigation?.Email,
                    Disabilitato = x.IdAspNetUsers != null && disabilitati.Contains(x.IdAspNetUsers)
                }
            }).ToList();
        }
    }

    public interface IHr_DipendenteService : IServiceBase
    {
        Task<GenericResult<Hr_DipendenteListOutModel>> Hr_DipendenteList(GenericRequest<Hr_DipendenteListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_DipendenteGetOutModel>> Hr_DipendenteGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_DipendentePutOutModel>> Hr_DipendentePut(GenericRequest<Hr_DipendentePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_AccountOutModel>> Hr_AccountCrea(GenericRequest<Hr_AccountCreaInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_AccountOutModel>> Hr_AccountDisattiva(GenericRequest<Hr_AccountDisattivaInModel> model, Boolean isSubProcess);
    }
}
