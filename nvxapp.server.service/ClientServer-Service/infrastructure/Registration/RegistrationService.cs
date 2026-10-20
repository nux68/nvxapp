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
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Site;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Registration
{
    /*
     Registrazione di una nuova azienda dallo showcase, senza login, in due tempi:
      1. CompanyRegister: controlla i dati, salva la richiesta (CompanyRegistrationRequest) e invia
         all'email indicata il link di conferma, valido ScadenzaOre ore. Non crea ancora niente.
      2. CompanyRegisterConfirm: dal link, crea l'azienda nel ramo DEMO (studio DEMO del centro DEMO,
         creati da PopolateDB) con la stessa creazione del menu "Aziende" (AccountService.CompanyPut):
         stessi utenti principali e, nei rami applicativi, gli stessi applicativi attivati e
         inizializzati sulle nuove aziende. Gli utenti ricevono la password scelta alla registrazione;
         all'email arriva il riepilogo degli utenti creati con il link di accesso (senza password).
     Non si usa EmailConfirmed di Identity: gli utenti nascono anche dai menu, senza registrazione.
     Gli errori di compilazione dei dati tornano come messaggi (MessageType.Error), mostrati dalla pagina.
     Indirizzo del link: App:PublicUrl di appsettings (es. https://miosito.it), altrimenti l'origine
     della pagina che ha chiesto la registrazione.
    */
    public class RegistrationService : ServiceBase, IRegistrationService
    {
        // chiave del ramo in cui nascono le aziende registrate (Dealer e FinancialAdvisor)
        public const string RamoDemo = "DEMO";

        public const int ScadenzaOre = 24;
        private const int PasswordMinLength = 8;
        private const int RagioneSocialeMinLength = 3;
        private const int RagioneSocialeMaxLength = 40;
        // richieste in attesa ammesse per lo stesso indirizzo email (contro gli invii ripetuti)
        private const int RichiesteMaxPerMail = 3;
        // pagina dello showcase che riceve il link e pagina di accesso (nell'email di riepilogo)
        private const string PaginaConferma = "/confirm-email";
        private const string PaginaLogin = "/login";
        // suffissi degli utenti creati con l'azienda (CompanyPut; "_User" nei rami che creano anche il dipendente)
        private static readonly string[] SuffissiUtenti = { "_PowerAdmin", "_Admin", "_User" };

        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly ICompanyRegistrationRequestRepository _companyRegistrationRequestRepository;

        public RegistrationService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   IAccountService accountService,
                                   IEmailService emailService,
                                   ICompanyRepository companyRepository,
                                   IUserCompanyRepository userCompanyRepository,
                                   IDealerRepository dealerRepository,
                                   IFinancialAdvisorRepository financialAdvisorRepository,
                                   ICompanyRegistrationRequestRepository companyRegistrationRequestRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _emailService = emailService;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            _dealerRepository = dealerRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _companyRegistrationRequestRepository = companyRegistrationRequestRepository;
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

                if (!_emailService.Configured || StudioDemo() == 0)
                {
                    Log.Warning("Registrazione non disponibile: invio email non configurato o ramo DEMO mancante.");
                    retVal.AddMessage("Registrazione non disponibile al momento.", MessageType.Error);
                    return retVal;
                }

                await EliminaRichiesteScadute();

                // nome gia' usato da un'azienda, dagli utenti che si creerebbero o da un'altra richiesta in attesa
                string? nomeOccupato = await NomeNonDisponibile(descrizione);
                if (nomeOccupato != null)
                {
                    retVal.AddMessage(nomeOccupato, MessageType.Error);
                    return retVal;
                }
                var stessoNome = RichiesteInAttesa().Where(x => x.Descrizione.ToUpper() == descrizione.ToUpper()).ToList();
                if (stessoNome.Any(x => !string.Equals(x.Mail, mail, StringComparison.OrdinalIgnoreCase)))
                {
                    retVal.AddMessage($"Il nome '{descrizione}' non e' disponibile: scegli un'altra ragione sociale.", MessageType.Error);
                    return retVal;
                }
                if (RichiesteInAttesa().Count(x => x.Mail.ToUpper() == mail.ToUpper()) >= RichiesteMaxPerMail + stessoNome.Count)
                {
                    retVal.AddMessage("Troppe registrazioni in attesa per questo indirizzo email: conferma quelle ricevute o riprova piu' tardi.", MessageType.Error);
                    return retVal;
                }

                // stesso nome e stessa email: la nuova richiesta sostituisce la precedente (nuovo link)
                foreach (var vecchia in stessoNome)
                    await _companyRegistrationRequestRepository.DeleteAsync(vecchia);

                string token = NuovoToken();
                var richiesta = await _companyRegistrationRequestRepository.CreateAsync(new CompanyRegistrationRequest()
                {
                    Descrizione = descrizione,
                    Mail = mail,
                    PasswordHash = _userManager.PasswordHasher.HashPassword(new ApplicationUser(), pw),
                    TokenHash = HashToken(token),
                    ExpiresAt = DateTime.Now.AddHours(ScadenzaOre)
                });

                try
                {
                    await InviaEmailConferma(mail, descrizione, LinkConferma(token));
                }
                catch (Exception e)
                {
                    Log.Error(e, "Registrazione: invio dell'email di conferma a {mail} non riuscito.", mail);
                    await _companyRegistrationRequestRepository.DeleteAsync(richiesta);
                    retVal.AddMessage("Non e' stato possibile inviare l'email di conferma: controlla l'indirizzo o riprova piu' tardi.", MessageType.Error);
                    return retVal;
                }

                retVal.Mail = mail;
                retVal.ScadenzaOre = ScadenzaOre;

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyRegisterConfirmOutModel>> CompanyRegisterConfirm(GenericRequest<CompanyRegisterConfirmInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyRegisterConfirmOutModel retVal = new CompanyRegisterConfirmOutModel();

                string token = model.Data?.Token?.Trim() ?? string.Empty;
                var richiesta = string.IsNullOrEmpty(token) ? null : _companyRegistrationRequestRepository.FindByTokenHash(HashToken(token));
                if (richiesta == null)
                {
                    retVal.AddMessage("Link di conferma non valido: registrati di nuovo.", MessageType.Error);
                    return retVal;
                }

                // gia' confermata (es. link aperto due volte): si mostrano gli utenti creati
                if (richiesta.ConfirmedAt != null && richiesta.IdCompany != null)
                {
                    await RiempiAzienda(retVal, richiesta.IdCompany.Value);
                    return retVal;
                }

                if (richiesta.ExpiresAt < DateTime.Now)
                {
                    await _companyRegistrationRequestRepository.DeleteAsync(richiesta);
                    retVal.AddMessage($"Il link di conferma e' scaduto (vale {ScadenzaOre} ore): registrati di nuovo.", MessageType.Error);
                    return retVal;
                }

                // il nome puo' essere stato preso nel frattempo (es. azienda creata dal menu)
                string? nomeOccupato = await NomeNonDisponibile(richiesta.Descrizione);
                int idFinancialAdvisor = StudioDemo();
                if (nomeOccupato != null || idFinancialAdvisor == 0)
                {
                    retVal.AddMessage(nomeOccupato ?? "Registrazione non disponibile al momento.", MessageType.Error);
                    return retVal;
                }

                // creazione come dal menu "Aziende" di uno studio: lo studio corrente e' quello DEMO.
                // Gli utenti nascono con una password provvisoria, sostituita subito da quella scelta.
                var reqPut = new GenericRequest<CompanyPutInModel>();
                reqPut.Data = new CompanyPutInModel();
                reqPut.Data.CompanyEdit = new CompanyEditModel()
                {
                    IdCompany = 0,
                    Descrizione = richiesta.Descrizione,
                    Mail = richiesta.Mail,
                    Pw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
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
                int idCompany = resPut.Data.CompanyEdit.IdCompany;

                // password scelta alla registrazione per tutti gli utenti dell'azienda
                foreach (var userCompany in _userCompanyRepository.FindAll(x => x.IdCompany == idCompany).ToList())
                {
                    var user = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    if (user == null)
                        continue;
                    user.PasswordHash = richiesta.PasswordHash;
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                // richiesta confermata: la password cifrata non serve piu'
                richiesta.ConfirmedAt = DateTime.Now;
                richiesta.IdCompany = idCompany;
                richiesta.PasswordHash = string.Empty;
                await _companyRegistrationRequestRepository.UpdateAsync(richiesta);

                await RiempiAzienda(retVal, idCompany);

                // riepilogo delle utenze per email (solo alla prima conferma): se non parte,
                // l'azienda resta creata e gli utenti si vedono comunque nella pagina
                try
                {
                    await InviaEmailRiepilogo(richiesta.Mail, retVal.Descrizione, retVal.Users);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Registrazione: invio del riepilogo delle utenze a {mail} non riuscito.", richiesta.Mail);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // azienda creata e suoi utenti, prima il PowerAdmin che gestisce l'azienda
        private async Task RiempiAzienda(CompanyRegisterConfirmOutModel retVal, int idCompany)
        {
            retVal.IdCompany = idCompany;
            retVal.Descrizione = _companyRepository.FindById(idCompany)?.Descrizione ?? string.Empty;

            foreach (var userCompany in _userCompanyRepository.FindAll(x => x.IdCompany == idCompany).ToList())
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
            string[] ordineRuoli = { "CompanyPowerAdmin", "CompanyAdmin", "User" };
            retVal.Users = retVal.Users.OrderBy(u => u.Roles.Select(r => Array.IndexOf(ordineRuoli, r)).Where(i => i >= 0).DefaultIfEmpty(ordineRuoli.Length).Min()).ToList();
        }

        // messaggio se il nome e' gia' di un'azienda o degli utenti che si creerebbero, altrimenti null
        private async Task<string?> NomeNonDisponibile(string descrizione)
        {
            string descrizioneNorm = descrizione.ToUpper();
            if (_companyRepository.FindAll(x => x.Descrizione != null && x.Descrizione.ToUpper() == descrizioneNorm).Any())
                return $"Esiste gia' un'azienda registrata come '{descrizione}'.";

            foreach (var suffisso in SuffissiUtenti)
                if (await _userManager.FindByNameAsync(descrizione + suffisso) != null)
                    return $"Il nome '{descrizione}' non e' disponibile: scegli un'altra ragione sociale.";

            return null;
        }

        private List<CompanyRegistrationRequest> RichiesteInAttesa()
            => _companyRegistrationRequestRepository.FindAll(x => x.ConfirmedAt == null && x.ExpiresAt >= DateTime.Now).ToList();

        private async Task EliminaRichiesteScadute()
        {
            foreach (var scaduta in _companyRegistrationRequestRepository.FindAll(x => x.ConfirmedAt == null && x.ExpiresAt < DateTime.Now).ToList())
                await _companyRegistrationRequestRepository.DeleteAsync(scaduta);
        }

        private async Task InviaEmailConferma(string mail, string descrizione, string link)
        {
            string sito = SiteName.Value;
            string linkHtml = WebUtility.HtmlEncode(link);
            string subject = $"{sito}: conferma la registrazione di {descrizione}";
            string html = $@"<div style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222;max-width:560px"">
  <h2 style=""margin:0 0 16px 0"">{WebUtility.HtmlEncode(sito)}</h2>
  <p>Hai chiesto di registrare l'azienda <b>{WebUtility.HtmlEncode(descrizione)}</b>.</p>
  <p>Per attivarla apri questo link entro {ScadenzaOre} ore:</p>
  <p style=""margin:24px 0""><a href=""{linkHtml}"" style=""background:#ff7a00;color:#fff;padding:12px 22px;border-radius:22px;text-decoration:none;font-weight:bold"">Attiva l'account</a></p>
  <p style=""color:#666;font-size:13px"">Se il pulsante non funziona, copia questo indirizzo nel browser:<br>{linkHtml}</p>
  <p style=""color:#666;font-size:13px"">Se non hai chiesto tu la registrazione, ignora questa email: senza conferma non viene creato niente.</p>
</div>";
            string text = $"{sito}\n\nHai chiesto di registrare l'azienda {descrizione}.\n" +
                          $"Per attivarla apri questo link entro {ScadenzaOre} ore:\n{link}\n\n" +
                          "Se non hai chiesto tu la registrazione, ignora questa email: senza conferma non viene creato niente.";
            await _emailService.SendAsync(mail, subject, html, text);
        }

        // azienda attivata: utenti creati con i ruoli e link di accesso (la password e' quella
        // scelta alla registrazione e non si invia)
        private async Task InviaEmailRiepilogo(string mail, string descrizione, List<CompanyRegisterUserModel> users)
        {
            string sito = SiteName.Value;
            string linkLogin = BaseUrl() + PaginaLogin;
            string linkHtml = WebUtility.HtmlEncode(linkLogin);
            string righeHtml = string.Join("", users.Select(u =>
                $@"<tr><td style=""padding:8px 12px;border-bottom:1px solid #eee;font-weight:bold"">{WebUtility.HtmlEncode(u.UserName)}</td>" +
                $@"<td style=""padding:8px 12px;border-bottom:1px solid #eee;color:#666"">{WebUtility.HtmlEncode(string.Join(", ", u.Roles))}</td></tr>"));
            string righeTesto = string.Join("\n", users.Select(u => $"  - {u.UserName} ({string.Join(", ", u.Roles)})"));

            string subject = $"{sito}: l'azienda {descrizione} e' attiva";
            string html = $@"<div style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222;max-width:560px"">
  <h2 style=""margin:0 0 16px 0"">{WebUtility.HtmlEncode(sito)}</h2>
  <p>L'azienda <b>{WebUtility.HtmlEncode(descrizione)}</b> e' attiva. Questi sono gli utenti creati:</p>
  <table style=""border-collapse:collapse;margin:16px 0"">{righeHtml}</table>
  <p>Per tutti vale la password che hai scelto alla registrazione.</p>
  <p style=""margin:24px 0""><a href=""{linkHtml}"" style=""background:#ff7a00;color:#fff;padding:12px 22px;border-radius:22px;text-decoration:none;font-weight:bold"">Accedi</a></p>
  <p style=""color:#666;font-size:13px"">Conserva questa email: contiene i nomi degli utenti per accedere.</p>
</div>";
            string text = $"{sito}\n\nL'azienda {descrizione} e' attiva. Questi sono gli utenti creati:\n{righeTesto}\n\n" +
                          $"Per tutti vale la password che hai scelto alla registrazione.\nAccedi da: {linkLogin}";
            await _emailService.SendAsync(mail, subject, html, text);
        }

        // indirizzo della pagina di conferma
        private string LinkConferma(string token)
            => BaseUrl() + PaginaConferma + "?token=" + Uri.EscapeDataString(token);

        // indirizzo del sito: App:PublicUrl, altrimenti l'origine della pagina chiamante (SiteUrl)
        private string BaseUrl() => SiteUrl.Base(_configuration, _httpContextAccessor);

        // token del link (solo nell'email) e suo hash (nel database)
        private static string NuovoToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static string HashToken(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

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
        Task<GenericResult<CompanyRegisterConfirmOutModel>> CompanyRegisterConfirm(GenericRequest<CompanyRegisterConfirmInModel> model, Boolean isSubProcess);
    }
}
