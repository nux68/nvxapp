using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline
{
    /*
     Scadenzario comune (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.1-2, INF-SVC.5).
     I moduli registrano le scadenze dei propri oggetti con IDeadlineRegistry (es. Compliance: il corso 12 del
     dipendente scade il 30/06): la chiave e' applicativo + fonte + riferimento, quindi a ogni salvataggio
     dell'oggetto basta richiamare UpsertAsync con i dati aggiornati, e CloseAsync/RemoveAsync quando l'oggetto
     e' rinnovato o cancellato. Il promemoria notturno (IDeadlineReminder, DeadlineMaintenanceService) avvisa il
     responsabile, o gli amministratori dell'azienda, una volta al preavviso e una volta alla scadenza.
    */
    public class DeadlineRegistry : IDeadlineRegistry
    {
        private readonly IScadenzaRepository _scadenzaRepository;

        public DeadlineRegistry(IScadenzaRepository scadenzaRepository)
        {
            _scadenzaRepository = scadenzaRepository;
        }

        public async Task<Scadenza> UpsertAsync(DeadlineRegistration r)
        {
            if (string.IsNullOrWhiteSpace(r.Fonte) || string.IsNullOrWhiteSpace(r.Descrizione))
                throw new ArgumentException("Fonte e descrizione della scadenza sono obbligatorie");

            var fonte = r.Fonte.Trim();
            var riga = r.IdRiferimento == null
                       ? null
                       : await _scadenzaRepository.FindAll(x => x.IdCompany == r.IdCompany && x.Application == (int)r.Application
                                                              && x.Fonte == fonte && x.IdRiferimento == r.IdRiferimento)
                                                  .FirstOrDefaultAsync();
            bool nuova = riga == null;
            riga ??= new Scadenza { IdCompany = r.IdCompany, Application = (int)r.Application, Fonte = fonte, IdRiferimento = r.IdRiferimento };

            // nuova data: si riapre e i promemoria ripartono
            if (!nuova && riga.DataScadenza.Date != r.DataScadenza.Date)
            {
                riga.Stato = StatoScadenza.Aperta;
                riga.DataChiusura = null;
                riga.DataAvvisoPreavviso = null;
                riga.DataAvvisoScaduta = null;
            }
            riga.Descrizione = Taglia(r.Descrizione.Trim(), 200)!;
            riga.Note = Taglia(r.Note, 500);
            riga.DataScadenza = r.DataScadenza.Date;
            riga.GiorniPreavviso = Math.Clamp(r.GiorniPreavviso, 0, 365);
            riga.IdAspNetUsersResponsabile = string.IsNullOrEmpty(r.IdAspNetUsersResponsabile) ? null : r.IdAspNetUsersResponsabile;
            riga.Url = Taglia(r.Url, 200);
            riga.UrlState = r.UrlState == null ? null : Taglia(JsonSerializer.Serialize(r.UrlState), 500);

            return nuova ? await _scadenzaRepository.CreateAsync(riga) : await _scadenzaRepository.UpdateAsync(riga);
        }

        public async Task<bool> CloseAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento)
        {
            var ora = DateTime.Now;
            return await Chiave(idCompany, application, fonte, idRiferimento)
                       .Where(x => x.Stato == StatoScadenza.Aperta)
                       .ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, StatoScadenza.Chiusa).SetProperty(x => x.DataChiusura, ora)) > 0;
        }

        public async Task<bool> RemoveAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento)
            => await Chiave(idCompany, application, fonte, idRiferimento).ExecuteDeleteAsync() > 0;

        public async Task<int> RemoveAllAsync(int idCompany, ApplicationType application, string fonte)
            => await _scadenzaRepository.FindAll(x => x.IdCompany == idCompany && x.Application == (int)application && x.Fonte == fonte).ExecuteDeleteAsync();

        private IQueryable<Scadenza> Chiave(int idCompany, ApplicationType application, string fonte, int idRiferimento)
            => _scadenzaRepository.FindAll(x => x.IdCompany == idCompany && x.Application == (int)application && x.Fonte == fonte && x.IdRiferimento == idRiferimento);

        internal static string? Taglia(string? s, int max) => s == null ? null : (s.Length > max ? s[..max] : s);
    }

    public interface IDeadlineRegistry
    {
        /// <summary>Crea o aggiorna la scadenza dell'oggetto (stessa azienda, applicativo, fonte e riferimento).</summary>
        Task<Scadenza> UpsertAsync(DeadlineRegistration registration);
        /// <summary>Chiude la scadenza (es. corso rinnovato): resta nello storico, niente piu' promemoria.</summary>
        Task<bool> CloseAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento);
        /// <summary>Cancella la scadenza (es. oggetto eliminato).</summary>
        Task<bool> RemoveAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento);
        /// <summary>Cancella tutte le scadenze di una fonte dell'azienda (es. ricostruzione completa da parte del modulo).</summary>
        Task<int> RemoveAllAsync(int idCompany, ApplicationType application, string fonte);
    }

    /// <summary>Scadenza registrata da un modulo.</summary>
    public class DeadlineRegistration
    {
        public int IdCompany { get; set; }
        public ApplicationType Application { get; set; }
        /// <summary>tipo di oggetto nel modulo, con un prefisso del modulo (es. "cmp.corso")</summary>
        public string Fonte { get; set; } = string.Empty;
        /// <summary>id dell'oggetto: con null si crea sempre una nuova scadenza</summary>
        public int? IdRiferimento { get; set; }
        /// <summary>testo gia' tradotto (es. "Corso antincendio - Rossi Mario")</summary>
        public string Descrizione { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime DataScadenza { get; set; }
        public int GiorniPreavviso { get; set; } = 30;
        public string? IdAspNetUsersResponsabile { get; set; }
        public string? Url { get; set; }
        public object? UrlState { get; set; }
    }

    // -------------------------------------------------------------------------------------------------
    // Promemoria

    public class DeadlineReminder : IDeadlineReminder
    {
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly INotificationSender _notificationSender;
        private readonly ICompanyRecipients _companyRecipients;

        public DeadlineReminder(IScadenzaRepository scadenzaRepository,
                                ICompanyApplicationRepository companyApplicationRepository,
                                INotificationSender notificationSender,
                                ICompanyRecipients companyRecipients)
        {
            _scadenzaRepository = scadenzaRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _notificationSender = notificationSender;
            _companyRecipients = companyRecipients;
        }

        public async Task<int> SendRemindersAsync(DateTime oggi)
        {
            oggi = oggi.Date;
            // aperte, non ancora avvisate per la fase in cui si trovano (preavviso o scaduta)
            var daAvvisare = await _scadenzaRepository.FindAll(x => x.Stato == StatoScadenza.Aperta
                                                                    && ((x.DataScadenza <= oggi && x.DataAvvisoScaduta == null)
                                                                        || (x.DataScadenza > oggi && x.DataAvvisoPreavviso == null
                                                                            && x.DataScadenza <= oggi.AddDays(x.GiorniPreavviso))))
                                                      .OrderBy(x => x.IdCompany).ThenBy(x => x.DataScadenza)
                                                      .ToListAsync();
            if (daAvvisare.Count == 0)
                return 0;

            var aziende = daAvvisare.Select(x => x.IdCompany).Distinct().ToList();
            var attivi = (await _companyApplicationRepository.FindAll(x => aziende.Contains(x.IdCompany) && x.Active)
                                                             .Select(x => new { x.IdCompany, x.ApplicationType }).ToListAsync())
                         .Select(x => (x.IdCompany, (int)x.ApplicationType)).ToHashSet();
            var amministratori = new Dictionary<int, List<string>>();

            int inviati = 0;
            foreach (var s in daAvvisare)
            {
                try
                {
                    // scadenze di un applicativo disattivato: niente promemoria finche' non torna attivo
                    if (s.Application != (int)ApplicationType.Common && !attivi.Contains((s.IdCompany, s.Application)))
                        continue;

                    List<string> destinatari;
                    if (s.IdAspNetUsersResponsabile != null && await _companyRecipients.IsActiveUserAsync(s.IdCompany, s.IdAspNetUsersResponsabile))
                        destinatari = new List<string> { s.IdAspNetUsersResponsabile };
                    else
                    {
                        if (!amministratori.TryGetValue(s.IdCompany, out var admin))
                            amministratori[s.IdCompany] = admin = await _companyRecipients.AdminsAsync(s.IdCompany);
                        destinatari = admin;
                    }

                    bool scaduta = s.DataScadenza <= oggi;
                    string data = s.DataScadenza.ToString("dd/MM/yyyy");
                    await _notificationSender.SendAsync(new NotificationMessage
                    {
                        UserIds = destinatari,
                        IdCompany = s.IdCompany,
                        Categoria = NotificationCategory.Scadenze.Code,
                        Titolo = InfrastructureMessages.Get(scaduta ? "DeadlineExpiredTitle" : "DeadlineReminderTitle", s.Descrizione),
                        Messaggio = InfrastructureMessages.Get(scaduta ? "DeadlineExpiredText" : "DeadlineReminderText", s.Descrizione, data),
                        Url = s.Url ?? "/deadlines",
                        UrlState = Stato(s.UrlState)
                    });

                    var ora = DateTime.Now;
                    if (scaduta)
                        await _scadenzaRepository.FindAll(x => x.Id == s.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.DataAvvisoScaduta, ora)
                                                                                                     .SetProperty(x => x.DataAvvisoPreavviso, x => x.DataAvvisoPreavviso ?? ora));
                    else
                        await _scadenzaRepository.FindAll(x => x.Id == s.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.DataAvvisoPreavviso, ora));
                    inviati++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Promemoria della scadenza {Id} non inviato", s.Id);
                }
            }
            return inviati;
        }

        private static object? Stato(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try { return JsonSerializer.Deserialize<JsonElement>(json); }
            catch (JsonException) { return null; }
        }
    }

    public interface IDeadlineReminder
    {
        /// <summary>Manda i promemoria dovuti alla data indicata; restituisce quante scadenze ha avvisato.</summary>
        Task<int> SendRemindersAsync(DateTime oggi);
    }
}
