using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Migrations.Public
{
    /*
     Tipi di richiesta iniziali dei ticket di assistenza (migration InitPublic).
     Le tabelle sono nuove: gli Id generati partono da 1 nell'ordine di inserimento.
     Nessun tipo ha la risoluzione AI attiva: il SuperUser la attiva dalla pagina dei tipi.
    */
    public static class PopolateTicketType
    {
        private static readonly TicketLevel[] Tutti =
        {
            TicketLevel.SuperUser, TicketLevel.Dev, TicketLevel.Dealer, TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User
        };

        private static readonly (ApplicationType? application, string codice, string descrizione, TicketLevel[] levels)[] Tipi =
        {
            (null, "ACCESSO",   "Accesso e credenziali",        new[] { TicketLevel.Company, TicketLevel.User }),
            (null, "UTENTI",    "Gestione utenti",              new[] { TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User }),
            (null, "CONTRATTO", "Contratto e fatturazione",     new[] { TicketLevel.Dealer, TicketLevel.FinancialAdvisor, TicketLevel.Company }),
            (null, "BUG",       "Malfunzionamento",             Tutti),
            (null, "NUOVA",     "Richiesta nuova funzionalità", Tutti),
            (null, "SVILUPPO",  "Attività di sviluppo",         new[] { TicketLevel.SuperUser, TicketLevel.Dev }),
            (ApplicationType.AttendanceTracking, "TIMBRATURE",     "Timbrature",                new[] { TicketLevel.Company, TicketLevel.User }),
            (ApplicationType.AttendanceTracking, "GIUSTIFICATIVI", "Giustificativi e causali",  new[] { TicketLevel.Company, TicketLevel.User }),
            (ApplicationType.AttendanceTracking, "NOTESPESE",      "Note spese",                new[] { TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User }),
            (ApplicationType.AttendanceTracking, "BUGPRESENZE",    "Malfunzionamento Presenze", Tutti),
        };

        public static void PopolateTicketType_UP(MigrationBuilder migrationBuilder)
        {
            int idTicketType = 0;
            foreach (var tipo in Tipi)
            {
                idTicketType++;

                migrationBuilder.InsertData(
                    table: "TicketType",
                    columns: new[] { "ApplicationType", "Codice", "Descrizione", "AiResolution", "Ordine", "Active" },
                    values: new object?[] { tipo.application.HasValue ? (int)tipo.application.Value : null, tipo.codice, tipo.descrizione, false, idTicketType * 10, true }
                    );

                foreach (var level in tipo.levels)
                {
                    migrationBuilder.InsertData(
                        table: "TicketTypeLevel",
                        columns: new[] { "IdTicketType", "Level" },
                        values: new object[] { idTicketType, (int)level }
                        );
                }
            }
        }
    }
}
