using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.PasswordRecovery.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Site;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Net;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.PasswordRecovery
{
    /*
     Recupero dell'accesso in caso di password dimenticata, senza login:
      1. PasswordRecoveryRequest: dal nome utente, invia all'email dell'utente il link per scegliere
         una nuova password (token di reset di Identity, valido ScadenzaOre ore e usabile una volta:
         cambiando la password cambia il security stamp). Si chiede il nome utente e non l'email
         perche' piu' utenti possono avere la stessa email (es. gli utenti di un'azienda).
         La risposta e' sempre la stessa, che l'utente esista o no, per non rivelare i nomi utente.
      2. PasswordRecoveryReset: dal link (pagina /reset-password), verifica il token e imposta la
         nuova password.
     Indirizzo del link: App:PublicUrl, altrimenti l'origine della pagina chiamante (SiteUrl).
    */
    public class PasswordRecoveryService : ServiceBase, IPasswordRecoveryService
    {
        // durata del token di reset di Identity (DataProtectorTokenProvider: 1 giorno se non configurato)
        public const int ScadenzaOre = 24;
        private const int PasswordMinLength = 8;
        // pagina dello showcase che riceve il link
        private const string PaginaReset = "/reset-password";

        private readonly IEmailService _emailService;

        public PasswordRecoveryService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration,

                                       IEmailService emailService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _emailService = emailService;
        }

        public virtual async Task<GenericResult<PasswordRecoveryRequestOutModel>> PasswordRecoveryRequest(GenericRequest<PasswordRecoveryRequestInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                PasswordRecoveryRequestOutModel retVal = new PasswordRecoveryRequestOutModel() { ScadenzaOre = ScadenzaOre };

                string userName = model.Data?.UserName?.Trim() ?? string.Empty;
                if (userName.Length == 0)
                {
                    retVal.AddMessage("Indica il nome utente.", MessageType.Error);
                    return retVal;
                }
                if (!_emailService.Configured)
                {
                    Log.Warning("Recupero password non disponibile: invio email non configurato.");
                    retVal.AddMessage("Recupero della password non disponibile al momento.", MessageType.Error);
                    return retVal;
                }

                // utente sconosciuto o senza email: nessun invio, ma la stessa risposta
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    Log.Information("Recupero password: utente {user} non trovato o senza email.", userName);
                    return retVal;
                }

                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string link = SiteUrl.Base(_configuration, _httpContextAccessor) + PaginaReset
                              + "?user=" + Uri.EscapeDataString(user.Id)
                              + "&token=" + WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                try
                {
                    await InviaEmailReset(user.Email, user.UserName ?? userName, link);
                }
                catch (Exception e)
                {
                    // non si rivela all'utente se l'invio e' partito: resta nel log
                    Log.Error(e, "Recupero password: invio dell'email a {mail} non riuscito.", user.Email);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PasswordRecoveryResetOutModel>> PasswordRecoveryReset(GenericRequest<PasswordRecoveryResetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                PasswordRecoveryResetOutModel retVal = new PasswordRecoveryResetOutModel();

                var data = model.Data ?? new PasswordRecoveryResetInModel();
                string pw = data.Pw ?? string.Empty;
                if (pw.Length < PasswordMinLength)
                {
                    retVal.AddMessage($"La password deve avere almeno {PasswordMinLength} caratteri.", MessageType.Error);
                    return retVal;
                }

                var user = string.IsNullOrEmpty(data.UserId) ? null : await _userManager.FindByIdAsync(data.UserId);
                string? token = null;
                try
                {
                    token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(data.Token ?? string.Empty));
                }
                catch (FormatException) { }

                if (user == null || string.IsNullOrEmpty(token))
                {
                    retVal.AddMessage("Link non valido: chiedi di nuovo il recupero della password.", MessageType.Error);
                    return retVal;
                }

                var result = await _userManager.ResetPasswordAsync(user, token, pw);
                if (!result.Succeeded)
                {
                    // token scaduto, gia' usato o password non accettata
                    bool tokenNonValido = result.Errors.Any(e => e.Code == "InvalidToken");
                    retVal.AddMessage(tokenNonValido
                        ? $"Il link non e' piu' valido (vale {ScadenzaOre} ore e una sola volta): chiedi di nuovo il recupero della password."
                        : string.Join(" ", result.Errors.Select(e => e.Description)), MessageType.Error);
                    return retVal;
                }

                retVal.UserName = user.UserName ?? string.Empty;

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        private async Task InviaEmailReset(string mail, string userName, string link)
        {
            string sito = SiteName.Value;
            string linkHtml = WebUtility.HtmlEncode(link);
            string subject = $"{sito}: reimposta la password di {userName}";
            string html = $@"<div style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222;max-width:560px"">
  <h2 style=""margin:0 0 16px 0"">{WebUtility.HtmlEncode(sito)}</h2>
  <p>E' stato chiesto di reimpostare la password dell'utente <b>{WebUtility.HtmlEncode(userName)}</b>.</p>
  <p>Per sceglierne una nuova apri questo link entro {ScadenzaOre} ore:</p>
  <p style=""margin:24px 0""><a href=""{linkHtml}"" style=""background:#ff7a00;color:#fff;padding:12px 22px;border-radius:22px;text-decoration:none;font-weight:bold"">Reimposta la password</a></p>
  <p style=""color:#666;font-size:13px"">Se il pulsante non funziona, copia questo indirizzo nel browser:<br>{linkHtml}</p>
  <p style=""color:#666;font-size:13px"">Se non l'hai chiesto tu, ignora questa email: la password attuale resta valida.</p>
</div>";
            string text = $"{sito}\n\nE' stato chiesto di reimpostare la password dell'utente {userName}.\n" +
                          $"Per sceglierne una nuova apri questo link entro {ScadenzaOre} ore:\n{link}\n\n" +
                          "Se non l'hai chiesto tu, ignora questa email: la password attuale resta valida.";
            await _emailService.SendAsync(mail, subject, html, text);
        }
    }

    public interface IPasswordRecoveryService : IServiceBase
    {
        Task<GenericResult<PasswordRecoveryRequestOutModel>> PasswordRecoveryRequest(GenericRequest<PasswordRecoveryRequestInModel> model, Boolean isSubProcess);
        Task<GenericResult<PasswordRecoveryResetOutModel>> PasswordRecoveryReset(GenericRequest<PasswordRecoveryResetInModel> model, Boolean isSubProcess);
    }
}
