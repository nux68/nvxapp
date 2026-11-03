using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Ad ogni apertura di connessione del contesto di un applicativo imposta il search_path della sessione
     PostgreSQL sullo schema di QUEL contesto:  SET search_path TO "schema_x", public

     - lo schema e' una proprieta' del contesto (quindi della richiesta), non uno stato globale:
       richieste concorrenti di aziende diverse usano connessioni diverse con search_path diversi
     - quando la connessione torna nel pool Npgsql ne azzera lo stato (DISCARD ALL), quindi
       chi la riusa non eredita lo schema
     - "public" resta nel percorso per le tabelle condivise referenziate dalle tabelle tenant
     - per i contesti degli applicativi (non Common) c'e' anche lo schema Common della stessa azienda
       (tenant_<IdAzienda>_0), che contiene le tabelle comuni (Az_Anagrafica, Az_Cliente, ...):
           SET search_path TO "tenant_5_2", "tenant_5_0", public

     Senza stato: una sola istanza per tutta l'applicazione.
    */
    public sealed class TenantSearchPathInterceptor : DbConnectionInterceptor
    {
        public static readonly TenantSearchPathInterceptor Instance = new TenantSearchPathInterceptor();

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            using var command = CreateCommand(connection, eventData);
            command.ExecuteNonQuery();
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            await using var command = CreateCommand(connection, eventData);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static DbCommand CreateCommand(DbConnection connection, ConnectionEndEventData eventData)
        {
            if (eventData.Context is not ApplicationDbContextBase applicationContext)
                throw new InvalidOperationException($"{nameof(TenantSearchPathInterceptor)} va registrato solo sui contesti degli applicativi ({nameof(ApplicationDbContextBase)}).");

            var schema = applicationContext.Schema; // gia' validato
            var command = connection.CreateCommand();
            command.CommandText = SearchPath(applicationContext.Application, schema);
            return command;
        }

        public static string SearchPath(ApplicationType application, string schema)
        {
            if (schema == TenantSchemaName.Public)
                return "SET search_path TO public";

            var common = application == ApplicationType.Common ? null : TenantSchemaName.CommonFor(schema);
            return common == null || common == schema
                ? $"SET search_path TO {TenantSchemaName.Quote(schema)}, public"
                : $"SET search_path TO {TenantSchemaName.Quote(schema)}, {TenantSchemaName.Quote(common)}, public";
        }
    }
}
