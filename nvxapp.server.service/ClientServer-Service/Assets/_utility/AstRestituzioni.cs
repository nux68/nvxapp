using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Beni e veicoli da restituire alla cessazione (ANALISI_ASSETS_APPLICATIVO.md, AST-BENI.3).
     La data di cessazione sta in Common (Dip_Anagrafica.DataCessazione, la tengono allineata Presenze e Risorse Umane):
     sono da restituire le consegne di beni ancora aperte e le assegnazioni di veicoli che vanno oltre la cessazione.
     Verifica (ogni giorno da AstRestituzioniMaintenanceService, o a richiesta dalla pagina dei beni): le assegnazioni non
     ancora avvisate con la cessazione entro il preavviso (Ast_Cfg.GiorniPreavviso)
        - vanno nello scadenzario comune (fonti "ast.restituzione" per i beni e "ast.restituzione.veicolo" per i veicoli,
          scadenza alla data di cessazione, senza responsabile: avvisa gli amministratori); si chiudono alla restituzione
        - una notifica per azienda agli amministratori con l'elenco (se le notifiche del modulo sono attive)
     Lavora sul contesto dello schema creato dalla factory (anche fuori da una richiesta), con i repository (AstRepositorySet).
    */
    public interface IAstRestituzioni
    {
        /// <summary>Assegnazioni aperte di dipendenti con data di cessazione (tutte, anche gia' avvisate).</summary>
        Task<List<Ast_DaRestituireModel>> ElencoAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>Avvisa le assegnazioni nuove dell'azienda; restituisce quante ne ha avvisate.</summary>
        Task<int> VerificaAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>Verifica tutte le aziende con Risorse aziendali attivo.</summary>
        Task<int> VerificaTutteAsync(CancellationToken cancellationToken = default);
    }

    public class AstRestituzioni : IAstRestituzioni
    {
        public const string FonteBene = "ast.restituzione";
        public const string FonteVeicolo = "ast.restituzione.veicolo";
        public const string Url = "/astgoods";

        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly INotificationSender _notificationSender;
        private readonly ICompanyRecipients _companyRecipients;

        public AstRestituzioni(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                               IServiceProvider sp, IHttpContextAccessor http, ICompanyApplicationRepository companyApplicationRepository,
                               IDeadlineRegistry deadlineRegistry, INotificationSender notificationSender, ICompanyRecipients companyRecipients)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
            _companyApplicationRepository = companyApplicationRepository;
            _deadlineRegistry = deadlineRegistry;
            _notificationSender = notificationSender;
            _companyRecipients = companyRecipients;
        }

        public async Task<List<Ast_DaRestituireModel>> ElencoAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await AstSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return new List<Ast_DaRestituireModel>();
            var rep = new AstRepositorySet(db, _sp, _http);
            var idAz = await IdAzAsync(rep, idCompany, cancellationToken);
            return idAz == 0 ? new List<Ast_DaRestituireModel>() : await LeggiAsync(rep, idAz, null, cancellationToken);
        }

        public async Task<int> VerificaAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            await using var db = await AstSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, cancellationToken);
            if (db == null)
                return 0;
            var rep = new AstRepositorySet(db, _sp, _http);
            var idAz = await IdAzAsync(rep, idCompany, cancellationToken);
            if (idAz == 0)
                return 0;
            var cfg = await rep.Ast_Cfg.FindAll(x => x.IdAz_Anagrafica == idAz).FirstOrDefaultAsync(cancellationToken);
            int preavviso = cfg?.GiorniPreavviso ?? 30;
            var entro = DateTime.Today.AddDays(preavviso);
            var nuove = (await LeggiAsync(rep, idAz, entro, cancellationToken)).Where(x => x.DataAvviso == null).ToList();
            if (nuove.Count == 0)
                return 0;

            foreach (var x in nuove)
            {
                await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
                {
                    IdCompany = idCompany,
                    Application = ApplicationType.Assets,
                    Fonte = x.Tipo == "veicolo" ? FonteVeicolo : FonteBene,
                    IdRiferimento = x.IdAssegnazione,
                    Descrizione = AssetsMessages.Get("ReturnDeadlineDescription", x.Oggetto, x.Dipendente),
                    DataScadenza = x.DataCessazione,
                    GiorniPreavviso = preavviso,
                    Url = Url,
                    UrlState = new { sezione = "restituire" },
                });
            }

            var ora = DateTime.Now;
            var beni = nuove.Where(x => x.Tipo == "bene").Select(x => x.IdAssegnazione).ToList();
            var veicoli = nuove.Where(x => x.Tipo == "veicolo").Select(x => x.IdAssegnazione).ToList();
            if (beni.Count > 0)
                await rep.Ast_BeneAssegnazione.FindAll(x => beni.Contains(x.Id)).ExecuteUpdateAsync(u => u.SetProperty(x => x.DataAvvisoCessazione, ora), cancellationToken);
            if (veicoli.Count > 0)
                await rep.Ast_VeicoloAssegnazione.FindAll(x => veicoli.Contains(x.Id)).ExecuteUpdateAsync(u => u.SetProperty(x => x.DataAvvisoCessazione, ora), cancellationToken);

            if (cfg?.NotificheAttive ?? true)
            {
                var admin = await _companyRecipients.AdminsAsync(idCompany);
                var righe = nuove.OrderBy(x => x.DataCessazione).ThenBy(x => x.Dipendente).Take(20)
                                 .Select(x => AssetsMessages.Get("ReturnNotificationLine", x.Dipendente, x.DataCessazione.ToString("dd/MM/yyyy"), x.Oggetto));
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = admin,
                    IdCompany = idCompany,
                    Categoria = AstNotificationCategoryProvider.Beni.Code,
                    Titolo = AssetsMessages.Get("ReturnNotificationTitle", nuove.Count),
                    Messaggio = string.Join("\n", righe) + (nuove.Count > 20 ? "\n..." : string.Empty),
                    Url = Url,
                    UrlState = new { sezione = "restituire" },
                });
            }
            return nuove.Count;
        }

        public async Task<int> VerificaTutteAsync(CancellationToken cancellationToken = default)
        {
            var aziende = await _companyApplicationRepository.FindAll(x => x.ApplicationType == ApplicationType.Assets && x.Active)
                                                             .Select(x => x.IdCompany).Distinct().ToListAsync(cancellationToken);
            int totale = 0;
            foreach (var idCompany in aziende)
            {
                try
                {
                    totale += await VerificaAsync(idCompany, cancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Risorse aziendali: verifica delle restituzioni non riuscita per l'azienda {IdCompany}", idCompany);
                }
            }
            return totale;
        }

        private static async Task<int> IdAzAsync(AstRepositorySet rep, int idCompany, CancellationToken cancellationToken)
            => await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        // assegnazioni aperte alla cessazione; entro = solo cessazioni fino a quella data
        private static async Task<List<Ast_DaRestituireModel>> LeggiAsync(AstRepositorySet rep, int idAz, DateTime? entro, CancellationToken cancellationToken)
        {
            var limite = entro ?? DateTime.MaxValue.Date;
            var beni = await rep.Ast_BeneAssegnazione.FindAll(x => x.DataRestituzione == null && x.Ast_BeneNavigation!.IdAz_Anagrafica == idAz
                                                                 && x.Dip_AnagraficaNavigation!.DataCessazione != null
                                                                 && x.Dip_AnagraficaNavigation.DataCessazione <= limite)
                                                    .Select(x => new
                                                    {
                                                        x.Id, x.IdAst_Bene, x.Ast_BeneNavigation!.CodiceInventario, x.Ast_BeneNavigation.Descrizione,
                                                        x.IdDip_Anagrafica, x.Dip_AnagraficaNavigation!.Cognome, x.Dip_AnagraficaNavigation.Nome,
                                                        DataCessazione = x.Dip_AnagraficaNavigation.DataCessazione!.Value, x.DataConsegna, x.DataAvvisoCessazione
                                                    })
                                                    .ToListAsync(cancellationToken);
            var veicoli = await rep.Ast_VeicoloAssegnazione.FindAll(x => x.Ast_VeicoloNavigation!.IdAz_Anagrafica == idAz
                                                                       && x.Dip_AnagraficaNavigation!.DataCessazione != null
                                                                       && x.Dip_AnagraficaNavigation.DataCessazione <= limite
                                                                       && (x.DataFine == null || x.DataFine > x.Dip_AnagraficaNavigation.DataCessazione))
                                                          .Select(x => new
                                                          {
                                                              x.Id, x.IdAst_Veicolo, x.Ast_VeicoloNavigation!.Targa, x.Ast_VeicoloNavigation.Marca, x.Ast_VeicoloNavigation.Modello,
                                                              x.IdDip_Anagrafica, x.Dip_AnagraficaNavigation!.Cognome, x.Dip_AnagraficaNavigation.Nome,
                                                              DataCessazione = x.Dip_AnagraficaNavigation.DataCessazione!.Value, x.DataInizio, x.DataAvvisoCessazione
                                                          })
                                                          .ToListAsync(cancellationToken);
            return beni.Select(x => new Ast_DaRestituireModel
                       {
                           Tipo = "bene", IdAssegnazione = x.Id, IdOggetto = x.IdAst_Bene, Oggetto = x.CodiceInventario + " - " + x.Descrizione,
                           IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = AstUtility.Nominativo(x.Cognome, x.Nome),
                           DataCessazione = x.DataCessazione, DataConsegna = x.DataConsegna, DataAvviso = x.DataAvvisoCessazione,
                       })
                       .Concat(veicoli.Select(x => new Ast_DaRestituireModel
                       {
                           Tipo = "veicolo", IdAssegnazione = x.Id, IdOggetto = x.IdAst_Veicolo, Oggetto = Ast_FlottaService.DescrizioneVeicolo(x.Marca, x.Modello, x.Targa),
                           IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = AstUtility.Nominativo(x.Cognome, x.Nome),
                           DataCessazione = x.DataCessazione, DataConsegna = x.DataInizio, DataAvviso = x.DataAvvisoCessazione,
                       }))
                       .OrderBy(x => x.DataCessazione).ThenBy(x => x.Dipendente).ToList();
        }
    }
}
