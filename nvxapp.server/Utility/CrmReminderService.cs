using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using Serilog;
using System.Globalization;

namespace nvxapp.server.Utility
{
    /*
     Promemoria delle attivita' del CRM: ogni minuto, per le aziende con il CRM attivo, avvisa (SignalR, IUserNotifier)
     l'assegnatario delle attivita' da fare che iniziano entro Crm_Cfg.MinutiPreavviso (0 = nessun promemoria).
     Ogni attivita' si avvisa una volta (PromemoriaInviato; si riazzera se cambiano inizio o assegnatario).
     Chi non e' collegato non riceve l'avviso: le attivita' restano comunque nell'agenda.
     Una volta all'ora segna anche come Scadute le offerte inviate con la validita' passata (CRM-PREV, scadenza chiusa).
    */
    public class CrmReminderService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(1);
        // offerte inviate con la validita' passata -> Scadute (CRM-PREV, CrmOfferteCiclo): una volta all'ora
        private static readonly TimeSpan IntervalloOfferte = TimeSpan.FromHours(1);
        private DateTime _ultimeOfferte = DateTime.MinValue;
        private readonly IServiceScopeFactory _scopeFactory;

        public CrmReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                try
                {
                    await EseguiAsync(stoppingToken);
                }
                catch (OperationCanceledException) { return; }
                catch (Exception ex)
                {
                    Log.Error(ex, "Promemoria del CRM non riusciti");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var aziendeApp = sp.GetRequiredService<ICompanyApplicationRepository>();
            var http = sp.GetRequiredService<IHttpContextAccessor>();
            var tenancy = sp.GetRequiredService<TenancySettings>();
            var factory = sp.GetRequiredService<IApplicationDbContextFactory>();
            var notifier = sp.GetRequiredService<IUserNotifier>();
            var deadlineRegistry = sp.GetRequiredService<IDeadlineRegistry>();
            bool offerte = DateTime.Now - _ultimeOfferte >= IntervalloOfferte;
            if (offerte)
                _ultimeOfferte = DateTime.Now;

            var aziende = await aziendeApp.FindAll(x => x.ApplicationType == ApplicationType.Crm && x.Active)
                                        .Select(x => x.IdCompany).ToListAsync(cancellationToken);
            foreach (var idCompany in aziende)
            {
                try
                {
                    var schema = tenancy.MultiTenant ? TenantSchemaName.For(idCompany, ApplicationType.Crm) : TenantSchemaName.Public;
                    await using var db = (CrmDbContext)factory.Create(ApplicationType.Crm, schema);
                    var rep = new CrmRepositorySet(db, sp, http);

                    var cfg = await rep.Crm_Cfg.FindAll(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany).FirstOrDefaultAsync(cancellationToken);
                    if (cfg != null && offerte)
                        await CrmOfferteCiclo.ScadiAsync(rep.Crm_Offerta, deadlineRegistry, idCompany, cfg.IdAz_Anagrafica);
                    if (cfg == null || cfg.MinutiPreavviso <= 0)
                        continue;

                    var adesso = DateTime.Now;
                    var limite = adesso.AddMinutes(cfg.MinutiPreavviso);
                    var attivita = await rep.Crm_Attivita.FindAll(_ => true).AsTracking()
                                           .Include(a => a.Az_ClienteNavigation)
                                           .Where(a => a.IdAz_Anagrafica == cfg.IdAz_Anagrafica && a.Stato == Crm_StatoAttivita.DaFare && !a.PromemoriaInviato
                                                       && a.Inizio <= limite && a.Inizio >= adesso.AddHours(-1))
                                           .ToListAsync(cancellationToken);
                    foreach (var a in attivita)
                    {
                        var quando = a.Inizio.ToString("t", CultureInfo.CurrentCulture);
                        await notifier.NotifyAsync(new[] { a.IdAspNetUsersAssegnatario }, new UserNotificationData
                        {
                            Title = CrmMessages.Get("ReminderTitle"),
                            Message = a.Az_ClienteNavigation != null
                                ? CrmMessages.Get("ReminderMessageCustomer", a.Oggetto, quando, a.Az_ClienteNavigation.Descrizione)
                                : CrmMessages.Get("ReminderMessage", a.Oggetto, quando),
                            Url = "/crmactivityedit",
                            State = new { id = a.Id }
                        });
                        a.PromemoriaInviato = true;
                    }
                    if (attivita.Count > 0)
                        await rep.Crm_Attivita.SaveChange();
                }
                catch (Exception ex)
                {
                    // un'azienda con problemi non blocca le altre
                    Log.Error(ex, "Promemoria del CRM non riusciti per l'azienda {IdCompany}", idCompany);
                }
            }
        }
    }
}
