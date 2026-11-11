using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /*
     Attivita' del CRM sul calendario personale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, CRM-CAL.1; feed di INF-CAL).
     Il feed arriva SENZA utente collegato: niente CrmUtility.ContestoAsync (legge l'utente della richiesta), lo schema
     dell'azienda si apre con la factory (CrmSchema.ContextAsync) e i dati si leggono con i repository (CrmRepositorySet).
        - solo se il CRM e' attivo per l'azienda e l'utente ha un ruolo dell'azienda (come ContestoAsync)
        - solo le attivita' ASSEGNATE all'utente, da fare o fatte (le annullate no), con inizio nell'intervallo:
          sono sempre tra quelle che l'utente vede nel CRM (CrmUtility.AttivitaVisibili), anche con la visibilita' "solo i propri"
        - descrizione con gli stessi dati dell'agenda e della scheda: cliente, contatto, opportunita', lead e descrizione
        - senza orario (inizio a mezzanotte e fine assente o a mezzanotte) = tutto il giorno; la fine a mezzanotte e' esclusa
        - Uid "crm-attivita-<IdAzienda>-<Id>": gli Id sono per schema, l'azienda li rende unici
        - link alla scheda dell'attivita' (/crmactivityedit?id=..., assoluto solo con App:PublicUrl)
    */
    public class CrmCalendarEventProvider : ICalendarEventProvider
    {
        private static readonly string[] RuoliAzienda = { nameof(RoleCode.CompanyPowerAdmin), nameof(RoleCode.CompanyAdmin), nameof(RoleCode.User) };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public CrmCalendarEventProvider(UserManager<ApplicationUser> userManager,
                                        ICompanyApplicationRepository companyApplicationRepository,
                                        TenancySettings tenancy,
                                        IApplicationDbContextFactory factory,
                                        ITenantProvisioningService provisioning,
                                        IServiceProvider sp,
                                        IHttpContextAccessor http)
        {
            _userManager = userManager;
            _companyApplicationRepository = companyApplicationRepository;
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
        }

        public async Task<IEnumerable<CalendarEvent>> EventsAsync(int idCompany, string idAspNetUsers, DateTime dal, DateTime al)
        {
            var nessuno = Enumerable.Empty<CalendarEvent>();

            if (!await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.ApplicationType == ApplicationType.Crm && x.Active).AnyAsync())
                return nessuno;

            var user = await _userManager.FindByIdAsync(idAspNetUsers);
            if (user == null || !(await _userManager.GetRolesAsync(user)).Any(r => RuoliAzienda.Contains(r)))
                return nessuno;

            await using var db = await CrmSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, CancellationToken.None);
            if (db == null)
                return nessuno;
            var rep = new CrmRepositorySet(db, _sp, _http);

            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync();
            if (idAz == 0)
                return nessuno;

            var righe = await rep.Crm_Attivita.FindAll(a => a.IdAz_Anagrafica == idAz && a.IdAspNetUsersAssegnatario == idAspNetUsers
                                                            && a.Stato != Crm_StatoAttivita.Annullata
                                                            && a.Inizio >= dal && a.Inizio <= al)
                                     .AsNoTracking()
                                     .OrderBy(a => a.Inizio)
                                     .Take(2000)
                                     .Select(a => new
                                     {
                                         a.Id, a.Oggetto, a.Descrizione, a.Inizio, a.Fine, a.Stato, a.ModifiedDate, a.CreationDate,
                                         Tipo = a.Crm_Par_TipoAttivitaNavigation != null ? a.Crm_Par_TipoAttivitaNavigation.Descrizione : null,
                                         Cliente = a.Az_ClienteNavigation != null ? a.Az_ClienteNavigation.Descrizione : null,
                                         ContattoCognome = a.Az_ClienteContattoNavigation != null ? a.Az_ClienteContattoNavigation.Cognome : null,
                                         ContattoNome = a.Az_ClienteContattoNavigation != null ? a.Az_ClienteContattoNavigation.Nome : null,
                                         Opportunita = a.Crm_OpportunitaNavigation != null ? a.Crm_OpportunitaNavigation.Titolo : null,
                                         LeadCognome = a.Crm_LeadNavigation != null ? a.Crm_LeadNavigation.Cognome : null,
                                         LeadNome = a.Crm_LeadNavigation != null ? a.Crm_LeadNavigation.Nome : null,
                                         LeadAzienda = a.Crm_LeadNavigation != null ? a.Crm_LeadNavigation.Azienda : null,
                                         a.IdCrm_Lead,
                                     })
                                     .ToListAsync();

            return righe.Select(a =>
            {
                var oggetto = string.IsNullOrWhiteSpace(a.Tipo) ? a.Oggetto : CrmMessages.Get("CalendarActivityTitle", a.Tipo, a.Oggetto);
                if (a.Stato == Crm_StatoAttivita.Fatta)
                    oggetto = CrmMessages.Get("CalendarActivityDone", oggetto);

                var righeDescrizione = new List<string>();
                if (!string.IsNullOrWhiteSpace(a.Cliente))
                    righeDescrizione.Add(CrmMessages.Get("CalendarActivityCustomer", a.Cliente));
                var contatto = Nome(a.ContattoCognome, a.ContattoNome);
                if (contatto != null)
                    righeDescrizione.Add(CrmMessages.Get("CalendarActivityContact", contatto));
                if (!string.IsNullOrWhiteSpace(a.Opportunita))
                    righeDescrizione.Add(CrmMessages.Get("CalendarActivityOpportunity", a.Opportunita));
                if (a.IdCrm_Lead != null)
                {
                    var lead = Nome(a.LeadCognome, a.LeadNome);
                    if (!string.IsNullOrWhiteSpace(a.LeadAzienda))
                        lead = lead == null ? a.LeadAzienda : lead + " - " + a.LeadAzienda;
                    if (lead != null)
                        righeDescrizione.Add(CrmMessages.Get("CalendarActivityLead", lead));
                }
                if (!string.IsNullOrWhiteSpace(a.Descrizione))
                {
                    if (righeDescrizione.Count > 0)
                        righeDescrizione.Add(string.Empty);
                    righeDescrizione.Add(a.Descrizione);
                }

                var evento = new CalendarEvent
                {
                    Uid = $"crm-attivita-{idCompany}-{a.Id}",
                    Titolo = oggetto,
                    Descrizione = righeDescrizione.Count == 0 ? null : string.Join("\n", righeDescrizione),
                    Inizio = a.Inizio,
                    Url = "/crmactivityedit?id=" + a.Id,
                    UltimaModifica = a.ModifiedDate ?? a.CreationDate,
                };

                // senza orario: tutto il giorno; l'ultimo giorno e' quello prima della mezzanotte di fine
                if (a.Inizio.TimeOfDay == TimeSpan.Zero && (a.Fine == null || a.Fine.Value.TimeOfDay == TimeSpan.Zero))
                {
                    evento.TuttoIlGiorno = true;
                    evento.Inizio = a.Inizio.Date;
                    evento.Fine = a.Fine != null && a.Fine.Value.Date > a.Inizio.Date ? a.Fine.Value.Date.AddDays(-1) : a.Inizio.Date;
                }
                else
                    evento.Fine = a.Fine;
                return evento;
            }).ToList();
        }

        private static string? Nome(string? cognome, string? nome)
        {
            var s = ((cognome ?? string.Empty) + " " + (nome ?? string.Empty)).Trim();
            return s.Length == 0 ? null : s;
        }
    }
}
