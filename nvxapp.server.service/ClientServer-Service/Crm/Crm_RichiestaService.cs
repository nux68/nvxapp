using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Assistenza ai clienti dell'azienda (CRM-ASS, ANALISI_CRM_ASSISTENZA.md): richieste dei clienti (Az_Cliente comune),
     diverse dai ticket di Infrastructure verso la piattaforma.
        - stati Nuova / In carico / In attesa del cliente / Risolta / Chiusa / Annullata, con i tempi di presa in carico e di
          risoluzione (date registrate qui, ore calcolate negli elenchi)
        - assegnazione a un utente dell'azienda con notifica (INotificationSender) all'assegnatario
        - note interne ed eventi (assegnazione, cambio di stato) in Crm_RichiestaMessaggio: lo storico della richiesta
     Ruoli (§5): gli amministratori vedono e gestiscono tutto; il commerciale (User) vede le richieste dei clienti che vede e
     quelle assegnate a lui, lavora quelle libere o sue, si assegna o lascia libere le richieste, non le riassegna ad altri
     e non le cancella.
    */
    public class Crm_RichiestaService : ServiceBase, ICrm_RichiestaService
    {
        private readonly ICrm_RichiestaRepository _Crm_Richiesta;
        private readonly ICrm_RichiestaMessaggioRepository _Crm_RichiestaMessaggio;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrmUtility _utility;
        private readonly INotificationSender _notificationSender;

        public Crm_RichiestaService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    ICrm_RichiestaRepository crm_RichiestaRepository,
                                    ICrm_RichiestaMessaggioRepository crm_RichiestaMessaggioRepository,
                                    ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                                    ICrmUtility utility,
                                    INotificationSender notificationSender) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Richiesta = crm_RichiestaRepository;
            _Crm_RichiestaMessaggio = crm_RichiestaMessaggioRepository;
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _utility = utility;
            _notificationSender = notificationSender;
        }

        public static bool Aperta(Crm_StatoRichiesta s) => s is Crm_StatoRichiesta.Nuova or Crm_StatoRichiesta.InCarico or Crm_StatoRichiesta.InAttesaCliente;

        public virtual async Task<GenericResult<Crm_RichiestaListOutModel>> Crm_RichiestaList(GenericRequest<Crm_RichiestaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var f = model.Data;
                var q = _utility.RichiesteVisibili(ctx);
                if (f.IdAz_Cliente.HasValue)
                    q = q.Where(r => r.IdAz_Cliente == f.IdAz_Cliente);
                if (f.SoloAperte)
                    q = q.Where(r => r.Stato == Crm_StatoRichiesta.Nuova || r.Stato == Crm_StatoRichiesta.InCarico || r.Stato == Crm_StatoRichiesta.InAttesaCliente);
                if (f.SoloMie)
                {
                    var me = ctx.IdUtente;
                    q = q.Where(r => r.IdAspNetUsersAssegnatario == me);
                }
                return new Crm_RichiestaListOutModel { Richieste = await CrmLists.RichiesteAsync(q, _utility) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_RichiestaGetOutModel>> Crm_RichiestaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var r = await Visibile(ctx, model.Data.Id, false);
                return new Crm_RichiestaGetOutModel
                {
                    Richiesta = Modello(r),
                    Cliente = r.Az_ClienteNavigation?.Descrizione ?? string.Empty,
                    Contatti = await CrmLists.ContattiAsync(_Az_ClienteContatto, r.IdAz_Cliente),
                    OpportunitaCliente = await CrmLists.OpportunitaAsync(_utility.OpportunitaVisibili(ctx).Where(o => o.IdAz_Cliente == r.IdAz_Cliente), _utility),
                    Messaggi = await MessaggiAsync(r.Id),
                    Modificabile = Lavorabile(ctx, r),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_RichiestaPutOutModel>> Crm_RichiestaPut(GenericRequest<Crm_RichiestaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Richiesta;
                if (string.IsNullOrWhiteSpace(d.Oggetto))
                    throw new UserMessageException(CrmMessages.Get("RequestSubjectRequired"));
                if (!Enum.IsDefined(d.Stato) || !Enum.IsDefined(d.Priorita) || !Enum.IsDefined(d.Canale))
                    throw new UserMessageException(CrmMessages.Get("RequestDataInvalid"));
                var cliente = await _utility.ClienteAsync(ctx, d.IdAz_Cliente);
                var ora = DateTime.Now;

                Crm_Richiesta r;
                bool nuova = d.Id == 0;
                Crm_StatoRichiesta statoPrima = Crm_StatoRichiesta.Nuova;
                if (!nuova)
                {
                    r = await Visibile(ctx, d.Id, true);
                    if (!Lavorabile(ctx, r))
                        throw new UserMessageException(CrmMessages.Get("RequestNotAllowed"));
                    statoPrima = r.Stato;
                }
                else
                {
                    var apertura = d.DataApertura == default || d.DataApertura > ora ? ora : d.DataApertura;
                    var anno = apertura.Year;
                    var ultimo = await _Crm_Richiesta.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Anno == anno).MaxAsync(x => (int?)x.Numero) ?? 0;
                    r = new Crm_Richiesta
                    {
                        IdAz_Anagrafica = ctx.IdAz_Anagrafica, Anno = anno, Numero = ultimo + 1, DataApertura = apertura,
                        Stato = Crm_StatoRichiesta.Nuova, CreationDate = ora,
                        // il commerciale che apre una richiesta senza assegnatario la lascia libera
                    };
                    await _Crm_Richiesta.CreateAsync(r, false);
                }

                // assegnatario: gli amministratori a chiunque; il commerciale solo a se' o a nessuno
                var assegnatario = string.IsNullOrEmpty(d.IdAssegnatario) ? null : d.IdAssegnatario;
                bool cambioAssegnatario = assegnatario != r.IdAspNetUsersAssegnatario;
                if (cambioAssegnatario)
                {
                    if (assegnatario != null && !(await _utility.UtentiAsync(ctx)).Any(u => u.Id == assegnatario))
                        throw new UserMessageException(CrmMessages.Get("UserInvalid"));
                    if (!ctx.Amministratore && !(assegnatario == null || assegnatario == ctx.IdUtente))
                        throw new UserMessageException(CrmMessages.Get("RequestAssignNotAllowed"));
                    r.IdAspNetUsersAssegnatario = assegnatario;
                }

                r.IdAz_Cliente = cliente.Id;
                r.IdAz_ClienteContatto = d.IdAz_ClienteContatto is int c && await _Az_ClienteContatto.FindAll(_ => true).AnyAsync(x => x.Id == c && x.IdAz_Cliente == cliente.Id) ? c : null;
                r.IdCrm_Opportunita = d.IdCrm_Opportunita is int o && await _utility.OpportunitaVisibili(ctx).AnyAsync(x => x.Id == o && x.IdAz_Cliente == cliente.Id) ? o : null;
                r.Oggetto = d.Oggetto.Trim();
                r.Descrizione = Pulito(d.Descrizione);
                r.Canale = d.Canale;
                r.Priorita = d.Priorita;
                r.Soluzione = Pulito(d.Soluzione);
                if (!nuova && d.DataApertura != default && d.DataApertura <= ora)
                    r.DataApertura = d.DataApertura;
                ApplicaStato(r, d.Stato, ora);
                r.ModifiedDate = ora;
                r.ChangeUser = ctx.IdUtente;
                await _Crm_Richiesta.SaveChange();

                // storico: apertura, cambio di stato, assegnazione
                var eventi = new List<string>();
                if (nuova)
                    eventi.Add(CrmMessages.Get("RequestEventOpened"));
                if (r.Stato != statoPrima)
                    eventi.Add(CrmMessages.Get("RequestEventState", CrmMessages.Get("RequestState_" + (int)statoPrima), CrmMessages.Get("RequestState_" + (int)r.Stato)));
                string? nomeAssegnatario = null;
                if (cambioAssegnatario)
                {
                    nomeAssegnatario = assegnatario == null ? null : (await _utility.NomiAsync(new[] { assegnatario })).GetValueOrDefault(assegnatario);
                    eventi.Add(assegnatario == null ? CrmMessages.Get("RequestEventUnassigned") : CrmMessages.Get("RequestEventAssigned", nomeAssegnatario ?? assegnatario));
                }
                foreach (var e in eventi)
                    await _Crm_RichiestaMessaggio.CreateAsync(new Crm_RichiestaMessaggio
                    {
                        IdCrm_Richiesta = r.Id, Data = ora, IdAspNetUsersAutore = ctx.IdUtente, Testo = e, Sistema = true, CreationDate = ora
                    }, false);
                if (eventi.Count > 0)
                    await _Crm_RichiestaMessaggio.SaveChange();

                if (cambioAssegnatario && assegnatario != null && assegnatario != ctx.IdUtente)
                    await NotificaAsync(ctx, r, cliente.Descrizione, assegnatario);

                return new Crm_RichiestaPutOutModel { Richiesta = Modello(r) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_RichiestaMessaggioPutOutModel>> Crm_RichiestaMessaggioPut(GenericRequest<Crm_RichiestaMessaggioPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data;
                if (string.IsNullOrWhiteSpace(d.Testo))
                    throw new UserMessageException(CrmMessages.Get("RequestMessageRequired"));
                // nota interna: chi vede la richiesta puo' aggiungerla
                var r = await Visibile(ctx, d.IdCrm_Richiesta, false);
                var testo = d.Testo.Trim();
                await _Crm_RichiestaMessaggio.CreateAsync(new Crm_RichiestaMessaggio
                {
                    IdCrm_Richiesta = r.Id, Data = DateTime.Now, IdAspNetUsersAutore = ctx.IdUtente,
                    Testo = testo.Length > 4000 ? testo[..4000] : testo, CreationDate = DateTime.Now
                });
                return new Crm_RichiestaMessaggioPutOutModel { Messaggi = await MessaggiAsync(r.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_RichiestaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.Amministratore)
                    throw new UserMessageException(CrmMessages.Get("RequestNotAllowed"));
                var r = await Visibile(ctx, model.Data.Id, true);
                // i messaggi si cancellano con la richiesta
                await _Crm_Richiesta.DeleteAsync(r);
                await _Crm_Richiesta.SaveChange();
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'

        /*
         Tempi: la presa in carico e' la prima uscita da "Nuova" verso la lavorazione (resta anche se si torna indietro);
         la risoluzione e' il passaggio a Risolta (o a Chiusa se non era gia' risolta); Chiusa e Annullata chiudono.
         Riaprendo (stato aperto) risoluzione e chiusura si svuotano.
        */
        public static void ApplicaStato(Crm_Richiesta r, Crm_StatoRichiesta nuovo, DateTime ora)
        {
            r.Stato = nuovo;
            if (nuovo != Crm_StatoRichiesta.Nuova && nuovo != Crm_StatoRichiesta.Annullata)
                r.DataPresaInCarico ??= ora;
            switch (nuovo)
            {
                case Crm_StatoRichiesta.Nuova:
                case Crm_StatoRichiesta.InCarico:
                case Crm_StatoRichiesta.InAttesaCliente:
                    r.DataRisoluzione = null;
                    r.DataChiusura = null;
                    break;
                case Crm_StatoRichiesta.Risolta:
                    r.DataRisoluzione ??= ora;
                    r.DataChiusura = null;
                    break;
                case Crm_StatoRichiesta.Chiusa:
                    r.DataRisoluzione ??= ora;
                    r.DataChiusura ??= ora;
                    break;
                case Crm_StatoRichiesta.Annullata:
                    r.DataChiusura ??= ora;
                    break;
            }
        }

        private static bool Lavorabile(CrmContesto ctx, Crm_Richiesta r)
            => ctx.Amministratore || r.IdAspNetUsersAssegnatario == null || r.IdAspNetUsersAssegnatario == ctx.IdUtente;

        private async Task<Crm_Richiesta> Visibile(CrmContesto ctx, int id, bool tracking)
        {
            var q = _utility.RichiesteVisibili(ctx).Include(r => r.Az_ClienteNavigation);
            return await (tracking ? q.AsTracking() : q.AsNoTracking()).FirstOrDefaultAsync(r => r.Id == id)
                   ?? throw new UserMessageException(CrmMessages.Get("RequestNotFound"));
        }

        private async Task<List<Crm_RichiestaMessaggioModel>> MessaggiAsync(int idRichiesta)
        {
            var righe = await _Crm_RichiestaMessaggio.FindAll(m => m.IdCrm_Richiesta == idRichiesta).OrderByDescending(m => m.Data).ThenByDescending(m => m.Id).ToListAsync();
            var nomi = await _utility.NomiAsync(righe.Select(m => m.IdAspNetUsersAutore));
            return righe.Select(m => new Crm_RichiestaMessaggioModel
            {
                Id = m.Id, Data = m.Data, Testo = m.Testo, Sistema = m.Sistema,
                Autore = m.IdAspNetUsersAutore != null && nomi.TryGetValue(m.IdAspNetUsersAutore, out var n) ? n : null
            }).ToList();
        }

        private async Task NotificaAsync(CrmContesto ctx, Crm_Richiesta r, string cliente, string assegnatario)
        {
            try
            {
                var numero = CrmLists.NumeroRichiesta(r.Anno, r.Numero);
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = new[] { assegnatario },
                    IdCompany = ctx.IdCompany,
                    // categoria comune in attesa delle categorie dei moduli (INF-SVC.7, scelta provvisoria A3)
                    Categoria = NotificationCategory.Generale.Code,
                    Titolo = CrmMessages.Get("RequestAssignedTitle", numero),
                    Messaggio = CrmMessages.Get("RequestAssignedText", cliente, r.Oggetto, CrmMessages.Get("RequestPriority_" + (int)r.Priorita)),
                    Url = "/crmrequestedit",
                    UrlState = new { id = r.Id }
                });
            }
            catch (Exception ex)
            {
                // la richiesta e' salvata comunque: la notifica non e' essenziale
                Log.Warning(ex, "Notifica della richiesta di assistenza {Id} non inviata", r.Id);
            }
        }

        public static double? Ore(DateTime da, DateTime? a) => a is DateTime fine ? Math.Round(Math.Max(0, (fine - da).TotalHours), 1) : null;

        private static Crm_RichiestaModel Modello(Crm_Richiesta r) => new()
        {
            Id = r.Id, NumeroCompleto = CrmLists.NumeroRichiesta(r.Anno, r.Numero), IdAz_Cliente = r.IdAz_Cliente, IdAz_ClienteContatto = r.IdAz_ClienteContatto,
            Oggetto = r.Oggetto, Descrizione = r.Descrizione, Canale = r.Canale, Priorita = r.Priorita, Stato = r.Stato, IdAssegnatario = r.IdAspNetUsersAssegnatario,
            DataApertura = r.DataApertura, DataPresaInCarico = r.DataPresaInCarico, DataRisoluzione = r.DataRisoluzione, DataChiusura = r.DataChiusura,
            OrePresaInCarico = Ore(r.DataApertura, r.DataPresaInCarico), OreRisoluzione = Ore(r.DataApertura, r.DataRisoluzione),
            Soluzione = r.Soluzione, IdCrm_Opportunita = r.IdCrm_Opportunita
        };

        private static string? Pulito(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public interface ICrm_RichiestaService : IServiceBase
    {
        Task<GenericResult<Crm_RichiestaListOutModel>> Crm_RichiestaList(GenericRequest<Crm_RichiestaListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_RichiestaGetOutModel>> Crm_RichiestaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_RichiestaPutOutModel>> Crm_RichiestaPut(GenericRequest<Crm_RichiestaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_RichiestaMessaggioPutOutModel>> Crm_RichiestaMessaggioPut(GenericRequest<Crm_RichiestaMessaggioPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_RichiestaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
    }
}
