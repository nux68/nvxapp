using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar
{
    /*
     Scrittura di un calendario iCalendar (RFC 5545) per il feed personale (INF-CAL.3):
        - righe terminate da CRLF, piegate a 75 ottetti (UTF-8, senza spezzare un carattere) con lo spazio iniziale
        - testo con escape di \ ; , e degli a capo
        - DTSTAMP e orari in UTC (suffisso Z); eventi tutto il giorno con VALUE=DATE e DTEND escluso (giorno dopo)
     Classe statica: lo scanner dei servizi non la registra.
    */
    public static class IcsWriter
    {
        public const string ProdId = "-//nvxapp//Calendario//IT";
        public const string UidDominio = "nvxapp";

        public static string Scrivi(string nomeCalendario, IEnumerable<CalendarEvent> eventi, string? baseUrl, DateTime adessoUtc)
        {
            var sb = new StringBuilder();
            Riga(sb, "BEGIN:VCALENDAR");
            Riga(sb, "VERSION:2.0");
            Riga(sb, "PRODID:" + ProdId);
            Riga(sb, "CALSCALE:GREGORIAN");
            Riga(sb, "METHOD:PUBLISH");
            Riga(sb, "X-WR-CALNAME:" + Testo(nomeCalendario));
            // frequenza di aggiornamento suggerita ai client (Outlook e Apple la rispettano, Google decide da se')
            Riga(sb, "REFRESH-INTERVAL;VALUE=DURATION:PT1H");
            Riga(sb, "X-PUBLISHED-TTL:PT1H");

            string dtstamp = Utc(adessoUtc);
            foreach (var e in eventi)
            {
                Riga(sb, "BEGIN:VEVENT");
                Riga(sb, "UID:" + Testo(e.Uid + "@" + UidDominio));
                Riga(sb, "DTSTAMP:" + dtstamp);
                if (e.TuttoIlGiorno)
                {
                    var inizio = e.Inizio.Date;
                    var ultimo = e.Fine?.Date ?? inizio;
                    if (ultimo < inizio)
                        ultimo = inizio;
                    Riga(sb, "DTSTART;VALUE=DATE:" + Data(inizio));
                    Riga(sb, "DTEND;VALUE=DATE:" + Data(ultimo.AddDays(1)));
                    // non occupa il tempo (libero/occupato)
                    Riga(sb, "TRANSP:TRANSPARENT");
                }
                else
                {
                    var fine = e.Fine ?? e.Inizio.AddHours(1);
                    if (fine < e.Inizio)
                        fine = e.Inizio;
                    Riga(sb, "DTSTART:" + Utc(e.Inizio));
                    Riga(sb, "DTEND:" + Utc(fine));
                }
                Riga(sb, "SUMMARY:" + Testo(e.Titolo));
                if (!string.IsNullOrWhiteSpace(e.Descrizione))
                    Riga(sb, "DESCRIPTION:" + Testo(e.Descrizione));
                var url = UrlAssoluto(e.Url, baseUrl);
                if (url != null)
                    Riga(sb, "URL:" + url);
                if (e.UltimaModifica != null)
                    Riga(sb, "LAST-MODIFIED:" + Utc(e.UltimaModifica.Value));
                Riga(sb, "END:VEVENT");
            }

            Riga(sb, "END:VCALENDAR");
            return sb.ToString();
        }

        /// <summary>escape del tipo TEXT (RFC 5545 §3.3.11)</summary>
        public static string Testo(string? valore)
        {
            if (string.IsNullOrEmpty(valore))
                return string.Empty;
            var sb = new StringBuilder(valore.Length + 8);
            for (int i = 0; i < valore.Length; i++)
            {
                char c = valore[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case ';': sb.Append("\\;"); break;
                    case ',': sb.Append("\\,"); break;
                    case '\r':
                        if (i + 1 < valore.Length && valore[i + 1] == '\n')
                            i++;
                        sb.Append("\\n");
                        break;
                    case '\n': sb.Append("\\n"); break;
                    default:
                        // altri caratteri di controllo non ammessi
                        if (!char.IsControl(c) || c == '\t')
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>aggiunge la riga piegata a 75 ottetti e terminata da CRLF (RFC 5545 §3.1)</summary>
        public static void Riga(StringBuilder sb, string riga)
        {
            const int massimo = 75;
            int ottetti = 0;
            var enumeratore = StringInfo.GetTextElementEnumerator(riga);
            while (enumeratore.MoveNext())
            {
                string elemento = enumeratore.GetTextElement();
                int lunghezza = Encoding.UTF8.GetByteCount(elemento);
                if (ottetti + lunghezza > massimo)
                {
                    sb.Append("\r\n ");
                    // la riga di continuazione comincia con uno spazio, che conta
                    ottetti = 1;
                }
                sb.Append(elemento);
                ottetti += lunghezza;
            }
            sb.Append("\r\n");
        }

        private static string Data(DateTime d) => d.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        private static string Utc(DateTime d)
        {
            var utc = d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Local).ToUniversalTime();
            return utc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        }

        private static string? UrlAssoluto(string? url, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            if (Uri.TryCreate(url, UriKind.Absolute, out var assoluto) && (assoluto.Scheme == Uri.UriSchemeHttp || assoluto.Scheme == Uri.UriSchemeHttps))
                return assoluto.AbsoluteUri;
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;
            return baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
        }
    }
}
