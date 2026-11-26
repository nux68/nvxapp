using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /*
     Tipi di richiesta dei ticket per gli applicativi che non ne avevano (la migration iniziale ha solo Generale e Presenze):
     CRM, Documentale, Risorse Umane, Compliance, Risorse aziendali. Solo dati: il modello non cambia.
     Inseriti solo se il codice non c'e' gia' (il SuperUser puo' averli creati a mano), con i livelli abilitati.
     Nessun tipo ha la risposta automatica AI attiva.
    */
    [DbContext(typeof(PublicDbContext))]
    [Migration("20261126130000_AddTicketTypeApplicativi")]
    public partial class AddTicketTypeApplicativi : Migration
    {
        private static readonly TicketLevel[] Tutti =
        {
            TicketLevel.SuperUser, TicketLevel.Dev, TicketLevel.Dealer, TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User
        };
        private static readonly TicketLevel[] AziendaUtente = { TicketLevel.Company, TicketLevel.User };
        private static readonly TicketLevel[] StudioAziendaUtente = { TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User };

        private static readonly (int application, string codice, string descrizione, TicketLevel[] levels)[] Tipi =
        {
            (3, "CLIENTI", "Clienti e contatti", AziendaUtente),
            (3, "OPPORTUNITA", "Opportunità e preventivi", AziendaUtente),
            (3, "BUGCRM", "Malfunzionamento CRM", Tutti),
            (4, "DOCUMENTI", "Documenti e cartelle", AziendaUtente),
            (4, "FIRMA", "Firma e presa visione", AziendaUtente),
            (4, "BUGDOC", "Malfunzionamento Documentale", Tutti),
            (5, "DIPENDENTI", "Scheda dipendente e contratti", StudioAziendaUtente),
            (5, "CEDOLINI", "Cedolini", StudioAziendaUtente),
            (5, "FORMAZIONE", "Formazione", AziendaUtente),
            (5, "BUGHR", "Malfunzionamento Risorse Umane", Tutti),
            (6, "GDPR", "Privacy e GDPR", AziendaUtente),
            (6, "SICUREZZA", "Sicurezza sul lavoro e DPI", AziendaUtente),
            (6, "BUGCMP", "Malfunzionamento Compliance", Tutti),
            (7, "FLOTTA", "Flotta e veicoli", AziendaUtente),
            (7, "PRENOTAZIONI", "Prenotazioni di sale e postazioni", AziendaUtente),
            (7, "BUGAST", "Malfunzionamento Risorse aziendali", Tutti),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            int ordine = 200;
            foreach (var tipo in Tipi)
            {
                ordine += 10;
                string codice = tipo.codice.Replace("'", "''");
                string descrizione = tipo.descrizione.Replace("'", "''");
                migrationBuilder.Sql($@"INSERT INTO public.""TicketType"" (""ApplicationType"", ""Codice"", ""Descrizione"", ""Ordine"", ""Active"")
SELECT {tipo.application}, '{codice}', '{descrizione}', {ordine}, true
WHERE NOT EXISTS (SELECT 1 FROM public.""TicketType"" WHERE ""Codice"" = '{codice}');");
                foreach (var level in tipo.levels)
                    migrationBuilder.Sql($@"INSERT INTO public.""TicketTypeLevel"" (""IdTicketType"", ""Level"")
SELECT t.""Id"", {(int)level} FROM public.""TicketType"" t
WHERE t.""Codice"" = '{codice}'
  AND NOT EXISTS (SELECT 1 FROM public.""TicketTypeLevel"" l WHERE l.""IdTicketType"" = t.""Id"" AND l.""Level"" = {(int)level});");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // si tolgono solo i tipi mai usati (nessun ticket, documento o impostazione AI collegati)
            foreach (var tipo in Tipi)
            {
                string codice = tipo.codice.Replace("'", "''");
                migrationBuilder.Sql($@"DELETE FROM public.""TicketTypeLevel"" WHERE ""IdTicketType"" IN (SELECT ""Id"" FROM public.""TicketType"" WHERE ""Codice"" = '{codice}')
  AND NOT EXISTS (SELECT 1 FROM public.""Ticket"" k JOIN public.""TicketType"" t ON t.""Id"" = k.""IdTicketType"" WHERE t.""Codice"" = '{codice}');");
                migrationBuilder.Sql($@"DELETE FROM public.""TicketType"" t WHERE t.""Codice"" = '{codice}'
  AND NOT EXISTS (SELECT 1 FROM public.""TicketTypeLevel"" l WHERE l.""IdTicketType"" = t.""Id"")
  AND NOT EXISTS (SELECT 1 FROM public.""Ticket"" k WHERE k.""IdTicketType"" = t.""Id"");");
            }
        }
    }
}
