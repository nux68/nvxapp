using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Registration
{
    /*
     Registrazione di una nuova azienda dallo showcase, senza login.
     L'azienda nasce nel ramo DEMO (studio DEMO del centro DEMO, creati da PopolateDB) con la stessa
     creazione del menu "Aziende" (AccountService.CompanyPut): stessi utenti principali e, nei rami
     applicativi, gli stessi applicativi attivati e inizializzati sulle nuove aziende.
     Gli errori di compilazione dei dati tornano come messaggi (MessageType.Error), mostrati dalla pagina.
    */
    public class RegistrationService : ServiceBase, IRegistrationService
    {
        // chiave del ramo in cui nascono le aziende registrate (Dealer e FinancialAdvisor)
        public const string RamoDemo = "DEMO";

        private const int PasswordMinLength = 8;
        private const int RagioneSocialeMinLength = 3;
        private const int RagioneSocialeMaxLength = 40;
        // suffissi degli utenti creati con l'azienda (CompanyPut; "_User" nei rami che creano anche il dipendente)
        private static readonly string[] SuffissiUtenti = { "_PowerAdmin", "_Admin", "_User" };

        private readonly IAccountService _accountService;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;

        public RegistrationService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   IAccountService accountService,
                                   ICompanyRepository companyRepository,
                                   IUserCompanyRepository userCompanyRepository,
                                   IDealerRepository dealerRepository,
                                   IFinancialAdvisorRepository financialAdvisorRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            _dealerRepository = dealerRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
        }

        public virtual async Task<GenericResult<CompanyRegisterOutModel>> CompanyRegister(GenericRequest<CompanyRegisterInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyRegisterOutModel retVal = new CompanyRegisterOutModel();

                var data = model.Data ?? new CompanyRegisterInModel();
                string descrizione = StringHelper.RemoveSpecialCharacters(data.RagioneSociale?.Trim()) ?? string.Empty;
                string mail = data.Mail?.Trim() ?? string.Empty;
                string pw = data.Pw ?? string.Empty;

                // dati della richiesta
                if (descrizione.Length < RagioneSocialeMinLength || descrizione.Length > RagioneSocialeMaxLength)
                    retVal.AddMessage($"La ragione sociale deve avere da {RagioneSocialeMinLength} a {RagioneSocialeMaxLength} lettere o cifre.", MessageType.Error);
                if (!MailValida(mail))
                    retVal.AddMessage("Indirizzo email non valido.", MessageType.Error);
                if (pw.Length < PasswordMinLength)
                    retVal.AddMessage($"La password deve avere almeno {PasswordMinLength} caratteri.", MessageType.Error);
                if (!data.AccettaCondizioni)
                    retVal.AddMessage("Per registrarti devi accettare le condizioni del servizio.", MessageType.Error);
                if (retVal.Messages.Any())
                    return retVal;

                // nome gia' usato da un'azienda o dagli utenti che si creerebbero
                string descrizioneNorm = descrizione.ToUpper();
                if (_companyRepository.FindAll(x => x.Descrizione != null && x.Descrizione.ToUpper() == descrizioneNorm).Any())
                    retVal.AddMessage($"Esiste gia' un'azienda registrata come '{descrizione}'.", MessageType.Error);
                else
                {
                    foreach (var suffisso in SuffissiUtenti)
                    {
                        if (await _userManager.FindByNameAsync(descrizione + suffisso) != null)
                        {
                            retVal.AddMessage($"Il nome '{descrizione}' non e' disponibile: scegli un'altra ragione sociale.", MessageType.Error);
                            break;
                        }
                    }
                }
                if (retVal.Messages.Any())
                    return retVal;

                int idFinancialAdvisor = StudioDemo();
                if (idFinancialAdvisor == 0)
                {
                    retVal.AddMessage("Registrazione non disponibile al momento.", MessageType.Error);
                    return retVal;
                }

                // creazione come dal menu "Aziende" di uno studio: lo studio corrente e' quello DEMO
                var reqPut = new GenericRequest<CompanyPutInModel>();
                reqPut.Data = new CompanyPutInModel();
                reqPut.Data.CompanyEdit = new CompanyEditModel()
                {
                    IdCompany = 0,
                    Descrizione = descrizione,
                    Mail = mail,
                    Pw = pw
                };

                ((_accountService as ServiceBase)!).tokenProperty = new TokenProperty()
                {
                    FinancialAdvisor = idFinancialAdvisor.ToString(),
                    Dealer = (_financialAdvisorRepository.FindById(idFinancialAdvisor)?.IdDealer ?? 0).ToString()
                };
                var resPut = await _accountService.CompanyPut(reqPut, true);

                if (!resPut.Success || resPut.Data == null || resPut.Data.CompanyEdit.IdCompany == 0)
                {
                    retVal.AddMessage("Registrazione non riuscita, riprova piu' tardi.", MessageType.Error);
                    return retVal;
                }

                retVal.IdCompany = resPut.Data.CompanyEdit.IdCompany;
                retVal.Descrizione = resPut.Data.CompanyEdit.Descrizione ?? descrizione;

                // utenti creati, per il primo accesso
                foreach (var userCompany in _userCompanyRepository.FindAll(x => x.IdCompany == retVal.IdCompany).ToList())
                {
                    var user = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    if (user?.UserName == null)
                        continue;
                    retVal.Users.Add(new CompanyRegisterUserModel()
                    {
                        UserName = user.UserName,
                        Roles = new List<string>(await _userManager.GetRolesAsync(user))
                    });
                }
                // prima il PowerAdmin, che gestisce l'azienda
                string[] ordineRuoli = { "CompanyPowerAdmin", "CompanyAdmin", "User" };
                retVal.Users = retVal.Users.OrderBy(u => u.Roles.Select(r => Array.IndexOf(ordineRuoli, r)).Where(i => i >= 0).DefaultIfEmpty(ordineRuoli.Length).Min()).ToList();

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // studio DEMO del centro DEMO (0 se manca)
        private int StudioDemo()
        {
            var dealer = _dealerRepository.FindAll(x => x.Descrizione == RamoDemo).FirstOrDefault();
            if (dealer == null)
                return 0;
            return _financialAdvisorRepository.FindAll(x => x.IdDealer == dealer.Id && x.Descrizione == RamoDemo).FirstOrDefault()?.Id ?? 0;
        }

        private static bool MailValida(string mail)
        {
            if (string.IsNullOrWhiteSpace(mail) || mail.Length > 256)
                return false;
            try
            {
                var address = new System.Net.Mail.MailAddress(mail);
                return address.Address == mail && mail.Contains('.');
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IRegistrationService : IServiceBase
    {
        Task<GenericResult<CompanyRegisterOutModel>> CompanyRegister(GenericRequest<CompanyRegisterInModel> model, Boolean isSubProcess);
    }
}
