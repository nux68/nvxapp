using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    // fonti di Compliance nei servizi comuni (scadenzario, prese visione): prefisso "cmp."
    public static class CmpFonti
    {
        public const string Formazione = "cmp.formazione";
        public const string Visita = "cmp.visita";
        public const string Nomina = "cmp.nomina";
        public const string Documento = "cmp.documento";
        public const string Responsabile = "cmp.responsabile";
        public const string Violazione = "cmp.violazione";
        // presa visione della consegna dei DPI: con il documentale attivo la fonte con "dpi" genera il verbale di consegna
        public const string Dpi = "cmp.dpi";
    }

    // pagine del client aperte dalle scadenze e dalle prese visione (stato: sezione e id dell'oggetto)
    public static class CmpUrl
    {
        public const string Sicurezza = "/cmp81";
        public const string Gdpr = "/cmpgdpr";
        public const string Miei = "/cmpmine";
    }

    /*
     Scadenze di Compliance nello scadenzario comune (IDeadlineRegistry, CMP-81.5 e CMP-GDPR): a ogni salvataggio dell'oggetto
     si registra (o aggiorna) la scadenza, alla cancellazione si toglie. Senza data la scadenza si toglie; "chiusa" la chiude
     (es. corso rinnovato, violazione notificata) e una scadenza chiusa che torna valida si riapre.
    */
    public interface ICmpScadenze
    {
        Task RegistraAsync(CmpContesto ctx, string fonte, int id, string descrizione, DateTime? data, bool chiusa, string url, string sezione,
                           string? note = null, int? giorniPreavviso = null);
        Task RimuoviAsync(CmpContesto ctx, string fonte, int id);
    }

    public class CmpScadenze : ICmpScadenze
    {
        private readonly IDeadlineRegistry _registry;

        public CmpScadenze(IDeadlineRegistry registry)
        {
            _registry = registry;
        }

        public async Task RegistraAsync(CmpContesto ctx, string fonte, int id, string descrizione, DateTime? data, bool chiusa, string url, string sezione,
                                        string? note = null, int? giorniPreavviso = null)
        {
            if (data == null)
            {
                await RimuoviAsync(ctx, fonte, id);
                return;
            }

            var registrazione = new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany,
                Application = ApplicationType.Compliance,
                Fonte = fonte,
                IdRiferimento = id,
                Descrizione = descrizione,
                Note = note,
                DataScadenza = data.Value.Date,
                GiorniPreavviso = giorniPreavviso ?? ctx.GiorniPreavviso,
                Url = url,
                UrlState = new { sezione, id },
            };
            var riga = await _registry.UpsertAsync(registrazione);
            if (chiusa)
                await _registry.CloseAsync(ctx.IdCompany, ApplicationType.Compliance, fonte, id);
            else if (riga.Stato == StatoScadenza.Chiusa)
            {
                // di nuovo valida (es. cancellato il corso che l'aveva rinnovata): si ricrea aperta
                await _registry.RemoveAsync(ctx.IdCompany, ApplicationType.Compliance, fonte, id);
                await _registry.UpsertAsync(registrazione);
            }
        }

        public Task RimuoviAsync(CmpContesto ctx, string fonte, int id)
            => _registry.RemoveAsync(ctx.IdCompany, ApplicationType.Compliance, fonte, id);
    }
}
