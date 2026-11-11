using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement
{
    /*
     Prese visione (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6): un modulo chiede a uno o piu' utenti di confermare
     di aver letto qualcosa (cedolino, consegna dei DPI, accordo di smart working...) senza conoscere il documentale.
        - IAcknowledgementService : interfaccia per i moduli (richiesta, stato, annullamento, conferma da un canale)
        - IAcknowledgementChannel : come si conferma. Il canale base "conferma" (ConfirmationChannel) e' sempre disponibile:
                                    l'utente conferma nella pagina "Prese visione". Il documentale aggiunge il proprio
                                    canale con la firma elettronica (DM-FIRMA), usato quando il documentale e' attivo;
                                    quando il destinatario firma, il canale chiama ConfirmFromChannelAsync.
     Si sceglie il canale richiesto, se disponibile per l'azienda, altrimenti quello disponibile con priorita' piu' alta.
     A ogni richiesta il destinatario riceve una notifica (categoria "presevisione").
    */
    public class AcknowledgementService : IAcknowledgementService
    {
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly IEnumerable<IAcknowledgementChannel> _channels;
        private readonly INotificationSender _notificationSender;

        public AcknowledgementService(IPresaVisioneRepository presaVisioneRepository,
                                      IEnumerable<IAcknowledgementChannel> channels,
                                      INotificationSender notificationSender)
        {
            _presaVisioneRepository = presaVisioneRepository;
            _channels = channels;
            _notificationSender = notificationSender;
        }

        public async Task<List<PresaVisione>> RequestAsync(AcknowledgementRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Fonte) || string.IsNullOrWhiteSpace(r.Titolo))
                throw new ArgumentException("Fonte e titolo della presa visione sono obbligatori");

            var canale = await ScegliCanaleAsync(r.IdCompany, r.Canale);
            var fonte = r.Fonte.Trim();
            string? urlState = r.UrlState == null ? null : Taglia(JsonSerializer.Serialize(r.UrlState), 500);
            var risultato = new List<PresaVisione>();
            var notificare = new List<string>();

            foreach (var idUtente in r.UserIds.Where(x => !string.IsNullOrEmpty(x)).Distinct())
            {
                // stessa richiesta gia' fatta (stesso oggetto e destinatario): si aggiornano i testi, la conferma resta
                var riga = r.IdRiferimento == null
                           ? null
                           : await _presaVisioneRepository.FindAll(x => x.IdCompany == r.IdCompany && x.Application == (int)r.Application && x.Fonte == fonte
                                                                      && x.IdRiferimento == r.IdRiferimento && x.IdAspNetUsers == idUtente)
                                                          .FirstOrDefaultAsync();
                bool nuova = riga == null;
                riga ??= new PresaVisione
                {
                    IdCompany = r.IdCompany,
                    Application = (int)r.Application,
                    Fonte = fonte,
                    IdRiferimento = r.IdRiferimento,
                    IdAspNetUsers = idUtente,
                    Canale = canale.Code
                };
                riga.Titolo = Taglia(r.Titolo.Trim(), 200)!;
                riga.Testo = Taglia(r.Testo, 4000);
                riga.Url = Taglia(r.Url, 200);
                riga.UrlState = urlState;
                bool riaperta = riga.Annullata || (r.RichiediDiNuovo && riga.DataConferma != null);
                if (riaperta)
                {
                    riga.Annullata = false;
                    riga.DataConferma = null;
                    riga.Ip = null;
                    riga.UserAgent = null;
                    riga.Canale = canale.Code;
                    riga.IdCanale = null;
                }

                riga = nuova ? await _presaVisioneRepository.CreateAsync(riga) : await _presaVisioneRepository.UpdateAsync(riga);

                if (nuova || riaperta)
                {
                    var idCanale = await canale.StartAsync(riga, r);
                    if (idCanale != null)
                        await _presaVisioneRepository.FindAll(x => x.Id == riga.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.IdCanale, Taglia(idCanale, 100)));
                    riga.IdCanale = idCanale;
                    notificare.Add(idUtente);
                }
                risultato.Add(riga);
            }

            if (notificare.Count > 0 && r.Notifica)
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = notificare,
                    IdCompany = r.IdCompany,
                    Categoria = NotificationCategory.PreseVisione.Code,
                    Titolo = InfrastructureMessages.Get("AcknowledgementRequestTitle", r.Titolo),
                    Messaggio = InfrastructureMessages.Get("AcknowledgementRequestText", r.Titolo),
                    Url = canale.UrlPerUtente ?? "/acknowledgements"
                });
            return risultato;
        }

        public async Task<List<PresaVisione>> StatusAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento)
            => await _presaVisioneRepository.FindAll(x => x.IdCompany == idCompany && x.Application == (int)application && x.Fonte == fonte
                                                        && x.IdRiferimento == idRiferimento && !x.Annullata)
                                            .ToListAsync();

        public async Task<int> CancelAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento, string? idAspNetUsers = null)
            => await _presaVisioneRepository.FindAll(x => x.IdCompany == idCompany && x.Application == (int)application && x.Fonte == fonte
                                                        && x.IdRiferimento == idRiferimento && x.DataConferma == null && !x.Annullata
                                                        && (idAspNetUsers == null || x.IdAspNetUsers == idAspNetUsers))
                                            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Annullata, true));

        public async Task<PresaVisione?> ConfirmFromChannelAsync(string canale, string idCanale, DateTime? data = null, string? ip = null, string? userAgent = null)
        {
            var riga = await _presaVisioneRepository.FindAll(x => x.Canale == canale && x.IdCanale == idCanale && !x.Annullata).FirstOrDefaultAsync();
            if (riga == null || riga.DataConferma != null)
                return riga;
            await ConfermaAsync(riga, data ?? DateTime.Now, ip, userAgent);
            return riga;
        }

        private async Task ConfermaAsync(PresaVisione riga, DateTime data, string? ip, string? userAgent)
        {
            riga.DataConferma = data;
            riga.Ip = Taglia(ip, 64);
            riga.UserAgent = Taglia(userAgent, 500);
            await _presaVisioneRepository.UpdateAsync(riga);
        }

        private async Task<IAcknowledgementChannel> ScegliCanaleAsync(int idCompany, string? richiesto)
        {
            var disponibili = new List<IAcknowledgementChannel>();
            foreach (var c in _channels.OrderByDescending(c => c.Priority))
            {
                try
                {
                    if (await c.AvailableAsync(idCompany))
                        disponibili.Add(c);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Canale di presa visione {Canale} non verificabile per l'azienda {IdCompany}", c.Code, idCompany);
                }
            }
            return disponibili.FirstOrDefault(c => c.Code == richiesto)
                   ?? disponibili.FirstOrDefault()
                   ?? _channels.First(c => c.Code == ConfirmationChannel.CodeConferma);
        }

        internal static string? Taglia(string? s, int max) => s == null ? null : (s.Length > max ? s[..max] : s);
    }

    public interface IAcknowledgementService
    {
        /// <summary>Chiede la presa visione a ogni utente indicato (una richiesta per oggetto e destinatario) e lo avvisa.</summary>
        Task<List<PresaVisione>> RequestAsync(AcknowledgementRequest request);
        /// <summary>Richieste dell'oggetto, con la data di conferma di ogni destinatario.</summary>
        Task<List<PresaVisione>> StatusAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento);
        /// <summary>Ritira le richieste non ancora confermate dell'oggetto (di tutti o di un destinatario).</summary>
        Task<int> CancelAsync(int idCompany, ApplicationType application, string fonte, int idRiferimento, string? idAspNetUsers = null);
        /// <summary>Usata dai canali: il destinatario ha confermato (es. ha firmato il documento).</summary>
        Task<PresaVisione?> ConfirmFromChannelAsync(string canale, string idCanale, DateTime? data = null, string? ip = null, string? userAgent = null);
    }

    /// <summary>Richiesta di presa visione da parte di un modulo.</summary>
    public class AcknowledgementRequest
    {
        public int IdCompany { get; set; }
        public ApplicationType Application { get; set; }
        /// <summary>tipo di oggetto nel modulo, con un prefisso del modulo (es. "hr.cedolino")</summary>
        public string Fonte { get; set; } = string.Empty;
        public int? IdRiferimento { get; set; }
        public IEnumerable<string> UserIds { get; set; } = Array.Empty<string>();
        /// <summary>testi gia' tradotti: titolo (es. "Cedolino ottobre 2026") e testo da leggere (facoltativo)</summary>
        public string Titolo { get; set; } = string.Empty;
        public string? Testo { get; set; }
        /// <summary>pagina del client con l'oggetto da leggere (es. il documento) e stato da passarle</summary>
        public string? Url { get; set; }
        public object? UrlState { get; set; }
        /// <summary>canale preferito (es. "doc.fes"); null o non disponibile = il migliore disponibile</summary>
        public string? Canale { get; set; }
        /// <summary>documento da far firmare, per i canali che lo usano (il canale base lo ignora)</summary>
        public string? NomeFile { get; set; }
        public byte[]? Contenuto { get; set; }
        /// <summary>chiede di nuovo la conferma anche a chi l'aveva gia' data (es. nuova versione del documento)</summary>
        public bool RichiediDiNuovo { get; set; }
        /// <summary>manda la notifica ai destinatari</summary>
        public bool Notifica { get; set; } = true;
    }

    /// <summary>Modo in cui il destinatario conferma (registrato dallo scanner dei servizi).</summary>
    public interface IAcknowledgementChannel
    {
        string Code { get; }
        /// <summary>il piu' alto vince tra i canali disponibili</summary>
        int Priority { get; }
        /// <summary>pagina in cui il destinatario conferma (null = pagina delle prese visione)</summary>
        string? UrlPerUtente { get; }
        Task<bool> AvailableAsync(int idCompany);
        /// <summary>avvia la richiesta nel canale (es. crea il documento da firmare); restituisce il riferimento nel canale</summary>
        Task<string?> StartAsync(PresaVisione presaVisione, AcknowledgementRequest request);
    }

    /// <summary>Canale base: l'utente conferma di aver letto nella pagina "Prese visione".</summary>
    public class ConfirmationChannel : IAcknowledgementChannel
    {
        public const string CodeConferma = "conferma";

        public string Code => CodeConferma;
        public int Priority => 0;
        public string? UrlPerUtente => null;
        public Task<bool> AvailableAsync(int idCompany) => Task.FromResult(true);
        public Task<string?> StartAsync(PresaVisione presaVisione, AcknowledgementRequest request) => Task.FromResult<string?>(null);
    }
}
