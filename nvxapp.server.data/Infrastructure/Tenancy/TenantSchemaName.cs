using System.Text.RegularExpressions;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Regole sui nomi degli schemi PostgreSQL degli applicativi delle aziende.
     Il nome finisce in comandi SQL (SET search_path, CREATE SCHEMA): viene sempre validato
     e usato tra doppi apici, mai concatenato senza controllo.
    */
    public static class TenantSchemaName
    {
        public const string Public = "public";

        // minuscole, cifre e underscore; inizia con lettera o underscore; max 63 caratteri (limite PostgreSQL)
        private static readonly Regex _valid = new Regex("^[a-z_][a-z0-9_]{0,62}$", RegexOptions.Compiled);

        /// <summary>Normalizza (minuscolo) e valida il nome; eccezione se non valido.</summary>
        public static string Validate(string? schema)
        {
            var value = (schema ?? string.Empty).Trim().ToLowerInvariant();
            if (!_valid.IsMatch(value))
                throw new InvalidOperationException($"Nome schema non valido: '{schema}'.");
            return value;
        }

        /// <summary>Schema dei dati di un applicativo di un'azienda (multi-tenant): tenant_&lt;IdAzienda&gt;_&lt;IdApplicativo&gt;.</summary>
        public static string For(int idCompany, ApplicationType application) => $"tenant_{idCompany}_{(int)application}";

        /// <summary>
        /// Schema Common (dati comuni) della stessa azienda di uno schema applicativo: tenant_&lt;IdAzienda&gt;_&lt;n&gt; -> tenant_&lt;IdAzienda&gt;_0.
        /// "public" resta "public" (modalita' singola); null se il nome non e' uno schema azienda.
        /// </summary>
        public static string? CommonFor(string schema)
        {
            var value = Validate(schema);
            if (value == Public)
                return Public;

            var match = _tenant.Match(value);
            return match.Success ? For(int.Parse(match.Groups[1].Value), ApplicationType.Common) : null;
        }

        private static readonly Regex _tenant = new Regex("^tenant_([0-9]+)_[0-9]+$", RegexOptions.Compiled);

        /// <summary>Identificatore quotato per l'SQL.</summary>
        public static string Quote(string schema) => $"\"{Validate(schema)}\"";
    }
}
