using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest
{
    /*
     Sezioni del riepilogo delle funzioni base (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.1): scadenzario, prese
     visione in attesa, notifiche non lette. Le tabelle sono in public: si leggono con i repository dallo scope.
     Regole di visibilita' come nelle pagine: gli amministratori dell'azienda (e lo studio, idAspNetUsers null) vedono
     tutta l'azienda, gli altri utenti solo cio' che li riguarda. Solo applicativi attivi.
    */

    /// <summary>Utilita' comuni ai provider delle funzioni base.</summary>
    internal static class DigestUtil
    {
        public const int MaxDettagli = 10;

        /// <summary>(vista dell'azienda, utente esistente): idAspNetUsers null = azienda; amministratore = azienda</summary>
        public static async Task<(bool Azienda, bool Valido)> VistaAsync(UserManager<ApplicationUser> userManager, string? idAspNetUsers)
        {
            if (idAspNetUsers == null)
                return (true, true);
            var user = await userManager.FindByIdAsync(idAspNetUsers);
            if (user == null)
                return (false, false);
            return (await DeadlineService.AmministratoreAsync(userManager, user), true);
        }

        /// <summary>applicativi attivi dell'azienda piu' le funzioni base (valori di ApplicationType)</summary>
        public static async Task<List<int>> AttiviAsync(ICompanyApplicationRepository repository, int idCompany)
            => (await repository.FindAll(x => x.IdCompany == idCompany && x.Active).Select(x => x.ApplicationType).ToListAsync())
               .Select(x => (int)x).Append((int)ApplicationType.Common).ToList();

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy");
    }

    /// <summary>Scadenzario: superate e ancora aperte, nei prossimi 14 giorni, chiuse nel periodo.</summary>
    public class DeadlineDigestProvider : IDigestProvider
    {
        public const int GiorniInScadenza = 14;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public DeadlineDigestProvider(UserManager<ApplicationUser> userManager,
                                      IScadenzaRepository scadenzaRepository,
                                      ICompanyApplicationRepository companyApplicationRepository)
        {
            _userManager = userManager;
            _scadenzaRepository = scadenzaRepository;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public ApplicationType Application => ApplicationType.Common;

        public async Task<DigestSection?> SectionAsync(int idCompany, string? idAspNetUsers, DateTime dal, DateTime al)
        {
            var (azienda, valido) = await DigestUtil.VistaAsync(_userManager, idAspNetUsers);
            if (!valido)
                return null;

            var oggi = al.Date;
            var entro = oggi.AddDays(GiorniInScadenza);
            var dalGiorno = dal.Date;
            var dopo = oggi.AddDays(1);
            var visibili = await DeadlineService.VisibiliPerUtente(_scadenzaRepository, _companyApplicationRepository, idCompany,
                                                                   idAspNetUsers ?? string.Empty, azienda, false);

            var section = new DigestSection
            {
                Codice = "scadenze",
                Application = ApplicationType.Common,
                Titolo = InfrastructureMessages.Get("DigestSectionDeadlines"),
                Ordine = 10,
            };

            var scadute = visibili.Where(x => x.Stato == StatoScadenza.Aperta && x.DataScadenza < oggi);
            int nScadute = await scadute.CountAsync();
            if (nScadute > 0)
            {
                section.Critica = true;
                section.Righe.Add(new DigestFact
                {
                    Testo = InfrastructureMessages.Get("DigestDeadlinesExpired", nScadute),
                    Numero = nScadute,
                    Critico = true,
                    Dettagli = await Dettagli(scadute),
                });
            }

            var inScadenza = visibili.Where(x => x.Stato == StatoScadenza.Aperta && x.DataScadenza >= oggi && x.DataScadenza <= entro);
            int nInScadenza = await inScadenza.CountAsync();
            if (nInScadenza > 0)
                section.Righe.Add(new DigestFact
                {
                    Testo = InfrastructureMessages.Get("DigestDeadlinesExpiring", nInScadenza, GiorniInScadenza),
                    Numero = nInScadenza,
                    Dettagli = await Dettagli(inScadenza),
                });

            var chiuse = visibili.Where(x => x.Stato == StatoScadenza.Chiusa && x.DataChiusura >= dalGiorno && x.DataChiusura < dopo);
            int nChiuse = await chiuse.CountAsync();
            if (nChiuse > 0)
                section.Righe.Add(new DigestFact
                {
                    Testo = InfrastructureMessages.Get("DigestDeadlinesClosed", nChiuse),
                    Numero = nChiuse,
                    Dettagli = await Dettagli(chiuse),
                });

            return section.Righe.Count == 0 ? null : section;
        }

        private static async Task<List<string>> Dettagli(IQueryable<Scadenza> query)
            => (await query.OrderBy(x => x.DataScadenza).Take(DigestUtil.MaxDettagli)
                           .Select(x => new { x.DataScadenza, x.Descrizione }).ToListAsync())
               .Select(x => DigestUtil.Data(x.DataScadenza) + " - " + x.Descrizione).ToList();
    }

    /// <summary>Prese visione in attesa di conferma (dell'azienda per gli amministratori e lo studio, proprie per gli altri).</summary>
    public class AcknowledgementDigestProvider : IDigestProvider
    {
        /// <summary>oltre questi giorni di attesa la sezione e' critica</summary>
        public const int GiorniAttesa = 7;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public AcknowledgementDigestProvider(UserManager<ApplicationUser> userManager,
                                             IPresaVisioneRepository presaVisioneRepository,
                                             ICompanyApplicationRepository companyApplicationRepository)
        {
            _userManager = userManager;
            _presaVisioneRepository = presaVisioneRepository;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public ApplicationType Application => ApplicationType.Common;

        public async Task<DigestSection?> SectionAsync(int idCompany, string? idAspNetUsers, DateTime dal, DateTime al)
        {
            var (azienda, valido) = await DigestUtil.VistaAsync(_userManager, idAspNetUsers);
            if (!valido)
                return null;

            var attivi = await DigestUtil.AttiviAsync(_companyApplicationRepository, idCompany);
            var inAttesa = _presaVisioneRepository.FindAll(x => x.IdCompany == idCompany && !x.Annullata && x.DataConferma == null
                                                                && attivi.Contains(x.Application));
            var limite = al.Date.AddDays(-GiorniAttesa);

            var section = new DigestSection
            {
                Codice = "presevisione",
                Application = ApplicationType.Common,
                Titolo = InfrastructureMessages.Get("DigestSectionAcknowledgements"),
                Ordine = 20,
            };

            if (azienda)
            {
                int n = await inAttesa.CountAsync();
                if (n > 0)
                {
                    var gruppi = await inAttesa.GroupBy(x => x.Titolo)
                                               .Select(g => new { Titolo = g.Key, N = g.Count() })
                                               .OrderByDescending(x => x.N).ThenBy(x => x.Titolo)
                                               .Take(DigestUtil.MaxDettagli).ToListAsync();
                    section.Righe.Add(new DigestFact
                    {
                        Testo = InfrastructureMessages.Get("DigestAcknowledgementsPending", n),
                        Numero = n,
                        Dettagli = gruppi.Select(g => InfrastructureMessages.Get("DigestAcknowledgementItem", g.Titolo, g.N)).ToList(),
                    });

                    int vecchie = await inAttesa.CountAsync(x => x.CreationDate < limite);
                    if (vecchie > 0)
                    {
                        section.Critica = true;
                        section.Righe.Add(new DigestFact
                        {
                            Testo = InfrastructureMessages.Get("DigestAcknowledgementsOld", vecchie, GiorniAttesa),
                            Numero = vecchie,
                            Critico = true,
                        });
                    }
                }
            }

            if (idAspNetUsers != null)
            {
                var mie = inAttesa.Where(x => x.IdAspNetUsers == idAspNetUsers);
                int n = await mie.CountAsync();
                if (n > 0)
                    section.Righe.Add(new DigestFact
                    {
                        Testo = InfrastructureMessages.Get("DigestAcknowledgementsMine", n),
                        Numero = n,
                        Dettagli = await mie.OrderBy(x => x.Id).Take(DigestUtil.MaxDettagli).Select(x => x.Titolo).ToListAsync(),
                    });
            }

            return section.Righe.Count == 0 ? null : section;
        }
    }

    /// <summary>Notifiche non lette dell'utente (solo nei riepiloghi personali; escluse quelle dei riepiloghi).</summary>
    public class NotificationDigestProvider : IDigestProvider
    {
        private readonly INotificaRepository _notificaRepository;
        private readonly INotificationCategories _categories;

        public NotificationDigestProvider(INotificaRepository notificaRepository, INotificationCategories categories)
        {
            _notificaRepository = notificaRepository;
            _categories = categories;
        }

        public ApplicationType Application => ApplicationType.Common;

        public async Task<DigestSection?> SectionAsync(int idCompany, string? idAspNetUsers, DateTime dal, DateTime al)
        {
            if (idAspNetUsers == null)
                return null;

            string riepilogo = NotificationCategory.Riepilogo.Code;
            var gruppi = await _notificaRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && !x.Letta
                                                                && (x.IdCompany == idCompany || x.IdCompany == null)
                                                                && x.Categoria != riepilogo)
                                                  .GroupBy(x => x.Categoria)
                                                  .Select(g => new { Categoria = g.Key, N = g.Count() })
                                                  .ToListAsync();
            int n = gruppi.Sum(x => x.N);
            if (n == 0)
                return null;

            return new DigestSection
            {
                Codice = "notifiche",
                Application = ApplicationType.Common,
                Titolo = InfrastructureMessages.Get("DigestSectionNotifications"),
                Ordine = 90,
                Righe = new List<DigestFact>
                {
                    new DigestFact
                    {
                        Testo = InfrastructureMessages.Get("DigestNotificationsUnread", n),
                        Numero = n,
                        Dettagli = gruppi.OrderByDescending(x => x.N)
                                         .Select(x => InfrastructureMessages.Get("DigestNotificationsCategory",
                                                                                 _categories.Find(x.Categoria)?.Titolo ?? x.Categoria, x.N))
                                         .ToList(),
                    }
                }
            };
        }
    }
}
