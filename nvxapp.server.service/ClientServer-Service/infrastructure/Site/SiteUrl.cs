using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Site
{
    /*
     Indirizzo pubblico del sito per i link nelle email (conferma della registrazione, recupero
     della password, accesso): App:PublicUrl di appsettings (es. https://miosito.it), altrimenti
     l'origine della pagina che ha fatto la richiesta (va bene in sviluppo).
    */
    public static class SiteUrl
    {
        public static string Base(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            string? baseUrl = configuration["App:PublicUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = httpContextAccessor.HttpContext?.Request.Headers["Origin"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("Indirizzo del sito non configurato (App:PublicUrl).");
            return baseUrl.TrimEnd('/');
        }
    }
}
