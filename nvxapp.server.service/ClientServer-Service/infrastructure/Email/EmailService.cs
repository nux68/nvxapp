using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Email
{
    /*
     Invio di email via SMTP (es. Brevo: smtp-relay.brevo.com, porta 587, StartTls).
     Parametri nella sezione "Email" di appsettings:
        Host, Port, User, FromAddress, FromName,
        Security: StartTls (predefinito) | SslOnConnect | None (es. server di prova locale come Mailpit)
     Password (chiave SMTP di Brevo): Email:Password, segreto non in appsettings ma nella variabile
     d'ambiente NVXAPP_Email__Password.
    */
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Configured => !string.IsNullOrWhiteSpace(_configuration["Email:Host"])
                                  && !string.IsNullOrWhiteSpace(_configuration["Email:FromAddress"]);

        public Task SendAsync(string to, string subject, string html, string text)
            => SendAsync(to, subject, html, text, Array.Empty<EmailAttachment>());

        public async Task SendAsync(string to, string subject, string html, string text, IEnumerable<EmailAttachment> attachments)
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
            var body = new BodyBuilder { HtmlBody = html, TextBody = text };
            foreach (var a in attachments)
                body.Attachments.Add(a.FileName, a.Content, ContentType.Parse(a.ContentType));
            message.Body = body.ToMessageBody();

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
            {
                string password = _configuration["Email:Password"]?.Trim() ?? string.Empty;
                if (password.Length == 0)
                    throw new Exception("Password SMTP non configurata: impostare la variabile d'ambiente NVXAPP_Email__Password.");
                await client.AuthenticateAsync(user, password);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public interface IEmailService
    {
        /// <summary>true se la sezione Email di appsettings ha almeno server e mittente.</summary>
        bool Configured { get; }

        /// <summary>Invia un'email in HTML con la versione in testo semplice.</summary>
        Task SendAsync(string to, string subject, string html, string text);

        /// <summary>Come sopra, con file allegati (es. il PDF di un'offerta del CRM).</summary>
        Task SendAsync(string to, string subject, string html, string text, IEnumerable<EmailAttachment> attachments);
    }

    public sealed class EmailAttachment
    {
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = "application/octet-stream";
        public byte[] Content { get; init; } = Array.Empty<byte>();
    }
}
