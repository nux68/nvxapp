using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MimeKit;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Email
{
    /*
     Invio di email via SMTP (es. Brevo: smtp-relay.brevo.com, porta 587, StartTls).
     Parametri nella sezione "Email" di appsettings:
        Host, Port, User, FromAddress, FromName,
        Security: StartTls (predefinito) | SslOnConnect | None (es. server di prova locale come Mailpit)
     Password (chiave SMTP di Brevo): dal file brevo.key nella root del sito (ContentRootPath, come
     openrouter.key e groq.key, escluso da git); senza il file vale Email:Password (es. variabile
     d'ambiente Email__Password).
    */
    public class EmailService : IEmailService
    {
        private const string PasswordFileName = "brevo.key";

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public EmailService(IConfiguration configuration, IHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public bool Configured => !string.IsNullOrWhiteSpace(_configuration["Email:Host"])
                                  && !string.IsNullOrWhiteSpace(_configuration["Email:FromAddress"]);

        public async Task SendAsync(string to, string subject, string html, string text)
        {
            string host = _configuration["Email:Host"] ?? string.Empty;
            string from = _configuration["Email:FromAddress"] ?? string.Empty;
            string user = _configuration["Email:User"] ?? string.Empty;
            if (host.Length == 0 || from.Length == 0)
                throw new Exception("Invio email non configurato (sezione Email di appsettings).");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["Email:FromName"] ?? string.Empty, from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = html, TextBody = text }.ToMessageBody();

            int.TryParse(_configuration["Email:Port"], out int port);
            var security = (_configuration["Email:Security"] ?? "StartTls") switch
            {
                "SslOnConnect" => SecureSocketOptions.SslOnConnect,
                "None" => SecureSocketOptions.None,
                _ => SecureSocketOptions.StartTls
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port > 0 ? port : 587, security);
            if (user.Length > 0)
                await client.AuthenticateAsync(user, Password());
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // password SMTP: file brevo.key nella root del sito, altrimenti Email:Password
        // (il file deve contenere solo la chiave, gli spazi e i ritorni a capo si ignorano)
        private string Password()
        {
            var path = Path.Combine(_env.ContentRootPath, PasswordFileName);
            if (File.Exists(path))
            {
                var key = File.ReadAllText(path).Trim();
                if (key.Length > 0)
                    return key;
            }
            return _configuration["Email:Password"] ?? string.Empty;
        }
    }

    public interface IEmailService
    {
        /// <summary>true se la sezione Email di appsettings ha almeno server e mittente.</summary>
        bool Configured { get; }

        /// <summary>Invia un'email in HTML con la versione in testo semplice.</summary>
        Task SendAsync(string to, string subject, string html, string text);
    }
}
