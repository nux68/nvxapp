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

        /*
         Indirizzo pubblico delle API per i link usati fuori dal browser (es. il feed .ics del calendario, INF-CAL.3):
         App:ApiPublicUrl (quando le API stanno su un indirizzo diverso dal sito), altrimenti App:PublicUrl (il server
         pubblica anche il client), altrimenti l'indirizzo con cui e' arrivata la richiesta (va bene in sviluppo, dove il
         client chiama le API su un'altra porta). Senza "/api" finale.
        */
        public static string ApiBase(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            string? baseUrl = configuration["App:ApiPublicUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = configuration["App:PublicUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var request = httpContextAccessor.HttpContext?.Request;
                if (request != null && request.Host.HasValue)
                    baseUrl = request.Scheme + "://" + request.Host.Value + request.PathBase.Value;
            }
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("Indirizzo delle API non configurato (App:ApiPublicUrl).");
            return baseUrl.TrimEnd('/');
        }
    }
}
