using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Payments
{
    /*
     Pagamenti delle licenze e loro ripartizione ai centri (ANALISI_PAGAMENTI_CENTRI.md §4).
        - SuperUser: tutti i centri; genera, conferma, annulla e liquida i rendiconti; impostazioni e dati di liquidazione di ogni centro
        - DealerPowerAdmin / DealerAdmin: solo il proprio centro (dal token, mai dal client), senza le bozze;
          il DealerPowerAdmin aggiorna i dati di liquidazione (non la trattenuta) e la fattura del centro sui rendiconti
        - gli altri ruoli: nessun accesso
    */
    public class PaymentReportService : ServiceBase, IPaymentReportService
    {
        private readonly IPaymentSettlementManager _manager;
        private readonly IPaymentSettingsProvider _settingsProvider;
        private readonly IPag_RendicontoRepository _rendicontoRepository;
        private readonly IPag_DealerConfigRepository _dealerConfigRepository;
        private readonly IPag_InvioEmailRepository _invioEmailRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly IApplicationActivationAuthorizer _authorizer;
        private readonly IPaymentGatewayResolver _gatewayResolver;
        private readonly IEmailService _emailService;

        public PaymentReportService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IPaymentSettlementManager manager,
                                    IPaymentSettingsProvider settingsProvider,
                                    IPag_RendicontoRepository rendicontoRepository,
                                    IPag_DealerConfigRepository dealerConfigRepository,
                                    IPag_InvioEmailRepository invioEmailRepository,
                                    IDealerRepository dealerRepository,
                                    IApplicationActivationAuthorizer authorizer,
                                    IPaymentGatewayResolver gatewayResolver,
                                    IEmailService emailService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _rendicontoRepository = rendicontoRepository;
            _dealerConfigRepository = dealerConfigRepository;
            _invioEmailRepository = invioEmailRepository;
            _dealerRepository = dealerRepository;
            _authorizer = authorizer;
            _gatewayResolver = gatewayResolver;
            _emailService = emailService;
        }

        // ---------------------------------------------------------------------------
        // Riepiloghi

        public virtual async Task<GenericResult<PaymentReportGetOutModel>> PaymentReportGet(GenericRequest<PaymentReportGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                ControllaPeriodo(model.Data.Anno, model.Data.Mese);
                var centri = await _manager.RiepilogoAsync(model.Data.Anno, model.Data.Mese, null);
                return new PaymentReportGetOutModel
                {
                    IsSuperUser = true,
                    Centro = centri,
                    DebugPagamenti = _gatewayResolver.DebugMode,
                    Totali = new PaymentTotalsModel
                    {
                        NumeroPagamenti = centri.Sum(x => x.NumeroPagamenti),
                        Incassato = centri.Sum(x => x.Incassato),
                        QuotaPiattaforma = centri.Sum(x => x.QuotaPiattaforma),
                        QuotaCentri = centri.Sum(x => x.QuotaCentro),
                        Liquidato = centri.Sum(x => x.Liquidato),
                        DaLiquidare = centri.Sum(x => x.DaLiquidare)
                    }
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentDealerGetOutModel>> PaymentDealerGet(GenericRequest<PaymentDealerGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                ControllaPeriodo(model.Data.Anno, model.Data.Mese);
                int idDealer = Centro(accesso, model.Data.IdDealer);
                int anno = model.Data.Anno;

                var riepilogo = (await _manager.RiepilogoAsync(anno, model.Data.Mese, idDealer)).FirstOrDefault(x => x.IdDealer == idDealer)
                                ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerNotFound"));
                if (!accesso.SuperUser && riepilogo.Rendiconto?.Stato == (int)StatoRendiconto.Bozza)
                    riepilogo.Rendiconto = null;

                var pagamenti = await _manager.PagamentiPeriodo(anno, model.Data.Mese, idDealer).OrderByDescending(x => x.DataPagamento).ToListAsync();

                var rendiconti = await _rendicontoRepository.FindAll(x => x.IdDealer == idDealer && x.Anno == anno && x.Stato != StatoRendiconto.Annullato
                                                                          && (accesso.SuperUser || x.Stato != StatoRendiconto.Bozza))
                                                            .OrderByDescending(x => x.Mese)
                                                            .ToListAsync();
                var idRendiconti = rendiconti.Select(r => r.Id).ToList();
                var invii = await _invioEmailRepository.FindAll(x => x.IdDealer == idDealer && x.Anno == anno).OrderByDescending(x => x.CreationDate).Take(30).ToListAsync();
                var statiAgganciati = await StatiRendicontoAsync(pagamenti);

                return new PaymentDealerGetOutModel
                {
                    IsSuperUser = accesso.SuperUser,
                    CanEditDealerData = accesso.SuperUser || accesso.PowerAdmin,
                    EmailConfigurata = _emailService.Configured,
                    Riepilogo = riepilogo,
                    Pagamento = pagamenti.Select(p => RowModel(p, statiAgganciati)).ToList(),
                    Rendiconto = rendiconti.Select(r =>
                    {
                        var m = PaymentSettlementManager.SettlementModel(r);
                        m.UltimoInvio = invii.Where(i => i.IdRendiconto == r.Id && i.Esito).Select(i => i.CreationDate).FirstOrDefault();
                        return m;
                    }).ToList(),
                    Invio = invii.Select(i => new PaymentEmailLogModel
                    {
                        Data = i.CreationDate, Anno = i.Anno, Mese = i.Mese, Tipo = (int)i.Tipo, Automatico = i.Automatico,
                        Destinatari = i.Destinatari, Esito = i.Esito, Errore = i.Errore
                    }).ToList(),
                    Dealer = accesso.SuperUser ? await DealersAsync() : new List<PaymentDealerItemModel>()
                };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Rendiconti

        public virtual async Task<GenericResult<PaymentSettlementGenerateOutModel>> PaymentSettlementGenerate(GenericRequest<PaymentSettlementGenerateInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var d = model.Data;
                int generati = d.IdDealer != null
                               ? (await _manager.GeneraAsync(d.IdDealer.Value, d.Anno, d.Mese) != null ? 1 : 0)
                               : await _manager.GeneraTuttiAsync(d.Anno, d.Mese);
                if (generati == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentNothingToSettle"));
                return new PaymentSettlementGenerateOutModel { Generati = generati };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettlementConfirmOutModel>> PaymentSettlementConfirm(GenericRequest<PaymentSettlementConfirmInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var r = await _manager.ConfermaAsync(model.Data.Id);
                bool inviata = await InviaSenzaBloccare(r, TipoRiepilogoPagamenti.Mensile);
                return new PaymentSettlementConfirmOutModel { Rendiconto = PaymentSettlementManager.SettlementModel(r), EmailInviata = inviata };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettlementCancelOutModel>> PaymentSettlementCancel(GenericRequest<PaymentSettlementCancelInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                await _manager.AnnullaAsync(model.Data.Id);
                return new PaymentSettlementCancelOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettlementPayOutModel>> PaymentSettlementPay(GenericRequest<PaymentSettlementPayInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var r = await _manager.LiquidaAsync(model.Data.Id, model.Data.DataLiquidazione, model.Data.RiferimentoBonifico, model.Data.Note);
                bool inviata = await InviaSenzaBloccare(r, TipoRiepilogoPagamenti.Liquidazione);
                return new PaymentSettlementPayOutModel { Rendiconto = PaymentSettlementManager.SettlementModel(r), EmailInviata = inviata };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettlementInvoicePutOutModel>> PaymentSettlementInvoicePut(GenericRequest<PaymentSettlementInvoicePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                var idDealer = await _rendicontoRepository.FindAll(x => x.Id == model.Data.Id).Select(x => (int?)x.IdDealer).FirstOrDefaultAsync()
                               ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementNotFound"));
                if (!accesso.SuperUser && !(accesso.PowerAdmin && accesso.IdDealer == idDealer))
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentNotAllowed"));
                var r = await _manager.FatturaAsync(model.Data.Id, model.Data.NumeroFatturaCentro, model.Data.DataFatturaCentro);
                return new PaymentSettlementInvoicePutOutModel { Rendiconto = PaymentSettlementManager.SettlementModel(r) };
            }, isSubProcess);
        }

        // la conferma o la liquidazione restano valide anche se l'email non parte: si vede nello storico degli invii
        private async Task<bool> InviaSenzaBloccare(Pag_Rendiconto r, TipoRiepilogoPagamenti tipo)
        {
            try
            {
                return (await _manager.InviaRendicontoAsync(r, tipo, false)).Esito;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Email del rendiconto {Id} non inviata", r.Id);
                return false;
            }
        }

        // ---------------------------------------------------------------------------
        // Email e documenti

        public virtual async Task<GenericResult<PaymentSummarySendOutModel>> PaymentSummarySend(GenericRequest<PaymentSummarySendInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                ControllaPeriodo(model.Data.Anno, model.Data.Mese);
                int idDealer = Centro(accesso, model.Data.IdDealer);
                var esito = await _manager.InviaRiepilogoAsync(idDealer, model.Data.Anno, model.Data.Mese, false);
                if (!esito.Esito)
                    throw new UserMessageException(esito.Errore ?? InfrastructureMessages.Get("PaymentSendFailed"));
                return new PaymentSummarySendOutModel { Destinatari = esito.Destinatari };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentExportOutModel>> PaymentExport(GenericRequest<PaymentExportInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                var d = model.Data;
                ControllaPeriodo(d.Anno, d.Mese);
                bool excel = string.Equals(d.Formato, "xlsx", StringComparison.OrdinalIgnoreCase);
                int? idDealer = accesso.SuperUser ? d.IdDealer ?? (excel ? null : accesso.IdDealer) : accesso.IdDealer;

                if (!excel)
                {
                    int id = idDealer ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerRequired"));
                    var doc = await _manager.DocumentoPeriodoAsync(id, d.Anno, d.Mese);
                    var nome = (await _dealerRepository.FindAll(x => x.Id == id).Select(x => x.Descrizione).FirstOrDefaultAsync()) ?? id.ToString();
                    return new PaymentExportOutModel
                    {
                        FileName = PaymentSettlementManager.NomeFile(nome, d.Anno, d.Mese),
                        ContentType = "application/pdf",
                        File = PaymentReportPdf.Crea(doc)
                    };
                }

                var riepilogo = await _manager.RiepilogoAsync(d.Anno, d.Mese, idDealer);
                if (idDealer != null)
                    riepilogo = riepilogo.Where(x => x.IdDealer == idDealer).ToList();
                var pagamenti = await _manager.PagamentiPeriodo(d.Anno, d.Mese, idDealer).OrderBy(x => x.DataPagamento).ToListAsync();
                var stati = await StatiRendicontoAsync(pagamenti);
                var titolo = InfrastructureMessages.Get(d.Mese == null ? "PaymentPdfTitleAnnual" : "PaymentPdfTitleMonthly", PaymentSettlementManager.NomePeriodo(d.Anno, d.Mese))
                             + (idDealer != null && riepilogo.Count > 0 ? " - " + riepilogo[0].Dealer : string.Empty);
                var file = PaymentSettlementManager.NomeFile(idDealer != null && riepilogo.Count > 0 ? riepilogo[0].Dealer ?? "centro" : InfrastructureMessages.Get("PaymentAllDealers"), d.Anno, d.Mese);
                return new PaymentExportOutModel
                {
                    FileName = Path.ChangeExtension(file, ".xlsx"),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    File = PaymentReportExcel.Crea(titolo, pagamenti.Select(p => RowModel(p, stati)).ToList(), riepilogo)
                };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Dati di liquidazione del centro e impostazioni

        public virtual async Task<GenericResult<PaymentDealerConfigGetOutModel>> PaymentDealerConfigGet(GenericRequest<PaymentDealerConfigGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                int idDealer = Centro(accesso, model.Data.IdDealer);
                return new PaymentDealerConfigGetOutModel { Config = await ConfigModelAsync(idDealer), CanEditPercentuale = accesso.SuperUser };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentDealerConfigPutOutModel>> PaymentDealerConfigPut(GenericRequest<PaymentDealerConfigPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoAsync();
                if (!accesso.SuperUser && !accesso.PowerAdmin)
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentNotAllowed"));
                var c = model.Data.Config;
                int idDealer = Centro(accesso, c.IdDealer);

                var iban = c.Iban?.Replace(" ", string.Empty).ToUpperInvariant();
                var email = c.EmailAmministrazione?.Trim();
                bool emailValide = string.IsNullOrEmpty(email)
                                   || email.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).All(e => new EmailAddressAttribute().IsValid(e));
                if ((c.PercentualePiattaforma is < 0 or > 100) || (iban != null && (iban.Length > 34 || !iban.All(char.IsLetterOrDigit)))
                    || (c.Intestatario?.Length > 100) || (email?.Length > 256) || !emailValide)
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerConfigInvalid"));

                var config = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).FirstOrDefaultAsync()
                             ?? new Pag_DealerConfig { IdDealer = idDealer };
                if (accesso.SuperUser)
                    config.PercentualePiattaforma = c.PercentualePiattaforma;
                config.Iban = string.IsNullOrEmpty(iban) ? null : iban;
                config.Intestatario = string.IsNullOrWhiteSpace(c.Intestatario) ? null : c.Intestatario.Trim();
                config.EmailAmministrazione = string.IsNullOrEmpty(email) ? null : email;
                if (config.Id == 0)
                    await _dealerConfigRepository.CreateAsync(config);
                else
                    await _dealerConfigRepository.UpdateAsync(config);

                return new PaymentDealerConfigPutOutModel { Config = await ConfigModelAsync(idDealer) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettingsGetOutModel>> PaymentSettingsGet(GenericRequest<PaymentSettingsGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                return new PaymentSettingsGetOutModel
                {
                    Settings = SettingsModel(await _settingsProvider.GetAsync()),
                    EmailConfigurata = _emailService.Configured,
                    DebugPagamenti = _gatewayResolver.DebugMode
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<PaymentSettingsPutOutModel>> PaymentSettingsPut(GenericRequest<PaymentSettingsPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var s = model.Data.Settings;
                if (s.PercentualePiattaforma is < 0 or > 100 || s.GiornoInvioMensile is < 1 or > 28 || s.MeseInvioAnnuale is < 1 or > 12)
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentSettingsInvalid"));
                await _settingsProvider.SaveAsync(new PaymentSettings
                {
                    PercentualePiattaforma = Math.Round(s.PercentualePiattaforma, 2),
                    RiepilogoAutomatico = s.RiepilogoAutomatico,
                    GiornoInvioMensile = s.GiornoInvioMensile,
                    MeseInvioAnnuale = s.MeseInvioAnnuale
                });
                return new PaymentSettingsPutOutModel { Settings = SettingsModel(await _settingsProvider.GetAsync()) };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        private sealed class Accesso
        {
            public bool SuperUser { get; set; }
            public int? IdDealer { get; set; }
            public bool PowerAdmin { get; set; }
        }

        private async Task<Accesso> AccessoAsync()
        {
            // il SuperUser che impersona un utente del centro ha nel token il centro impersonato: vale come centro predefinito
            if (await _authorizer.IsSuperUserAsync())
                return new Accesso { SuperUser = true, IdDealer = int.TryParse(CurrentDealer, out var impersonato) && impersonato > 0 ? impersonato : null };

            bool powerAdmin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerPowerAdmin));
            bool admin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerAdmin));
            if ((!powerAdmin && !admin) || !int.TryParse(CurrentDealer, out var idDealer) || idDealer <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentNotAllowed"));
            return new Accesso { IdDealer = idDealer, PowerAdmin = powerAdmin };
        }

        // il centro su cui lavorare: quello richiesto (o impersonato) per il SuperUser, quello del token per gli utenti del centro
        private static int Centro(Accesso accesso, int? richiesto)
            => accesso.SuperUser ? richiesto ?? accesso.IdDealer ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerRequired"))
                                 : accesso.IdDealer!.Value;

        private async Task SoloSuperUser()
        {
            if (!await _authorizer.IsSuperUserAsync())
                throw new UserMessageException(InfrastructureMessages.Get("PaymentNotAllowed"));
        }

        private static void ControllaPeriodo(int anno, int? mese)
        {
            if (anno < 2000 || anno > 2100 || mese is < 1 or > 12)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentPeriodInvalid"));
        }

        private async Task<Dictionary<int, StatoRendiconto>> StatiRendicontoAsync(List<Lic_Pagamento> pagamenti)
        {
            var ids = pagamenti.Where(p => p.IdRendiconto != null).Select(p => p.IdRendiconto!.Value).Distinct().ToList();
            return ids.Count == 0 ? new Dictionary<int, StatoRendiconto>()
                                  : await _rendicontoRepository.FindAll(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Stato);
        }

        private static PaymentRowModel RowModel(Lic_Pagamento p, Dictionary<int, StatoRendiconto> stati) => new()
        {
            Id = p.Id,
            DataPagamento = p.DataPagamento,
            IdDealer = p.IdDealer,
            Dealer = p.DealerDescrizione,
            Soggetto = p.Soggetto,
            TipoSoggetto = p.IdCompany != null ? (int)TipoSoggettoLicenza.Company : (int)TipoSoggettoLicenza.FinancialAdvisor,
            Ambito = (int)p.Ambito,
            Livello = (int)p.Livello,
            Periodicita = (int)p.Periodicita,
            DataInizioCoperta = p.DataInizioCoperta,
            DataFineCoperta = p.DataFineCoperta,
            Importo = p.Importo,
            Valuta = p.Valuta,
            Stato = (int)p.Stato,
            Debug = p.Debug,
            IdRendiconto = p.IdRendiconto,
            StatoRendiconto = p.IdRendiconto != null && stati.TryGetValue(p.IdRendiconto.Value, out var s) ? (int)s : null,
            IdRendicontoRettifica = p.IdRendicontoRettifica
        };

        private async Task<PaymentDealerConfigModel> ConfigModelAsync(int idDealer)
        {
            var dealer = await _dealerRepository.FindAll(x => x.Id == idDealer).Select(x => x.Descrizione).FirstOrDefaultAsync()
                         ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerNotFound"));
            var c = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).FirstOrDefaultAsync();
            return new PaymentDealerConfigModel
            {
                IdDealer = idDealer,
                Dealer = dealer,
                PercentualePiattaforma = c?.PercentualePiattaforma,
                PercentualeGenerale = (await _settingsProvider.GetAsync()).PercentualePiattaforma,
                Iban = c?.Iban,
                Intestatario = c?.Intestatario,
                EmailAmministrazione = c?.EmailAmministrazione
            };
        }

        private async Task<List<PaymentDealerItemModel>> DealersAsync()
            => await _dealerRepository.FindAll(x => true).OrderBy(x => x.Descrizione)
                                      .Select(x => new PaymentDealerItemModel { IdDealer = x.Id, Descrizione = x.Descrizione ?? string.Empty })
                                      .ToListAsync();

        private static PaymentSettingsModel SettingsModel(PaymentSettings s) => new()
        {
            PercentualePiattaforma = s.PercentualePiattaforma,
            RiepilogoAutomatico = s.RiepilogoAutomatico,
            GiornoInvioMensile = s.GiornoInvioMensile,
            MeseInvioAnnuale = s.MeseInvioAnnuale
        };
    }

    public interface IPaymentReportService : IServiceBase
    {
        Task<GenericResult<PaymentReportGetOutModel>> PaymentReportGet(GenericRequest<PaymentReportGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentDealerGetOutModel>> PaymentDealerGet(GenericRequest<PaymentDealerGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettlementGenerateOutModel>> PaymentSettlementGenerate(GenericRequest<PaymentSettlementGenerateInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettlementConfirmOutModel>> PaymentSettlementConfirm(GenericRequest<PaymentSettlementConfirmInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettlementCancelOutModel>> PaymentSettlementCancel(GenericRequest<PaymentSettlementCancelInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettlementPayOutModel>> PaymentSettlementPay(GenericRequest<PaymentSettlementPayInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettlementInvoicePutOutModel>> PaymentSettlementInvoicePut(GenericRequest<PaymentSettlementInvoicePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSummarySendOutModel>> PaymentSummarySend(GenericRequest<PaymentSummarySendInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentExportOutModel>> PaymentExport(GenericRequest<PaymentExportInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentDealerConfigGetOutModel>> PaymentDealerConfigGet(GenericRequest<PaymentDealerConfigGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentDealerConfigPutOutModel>> PaymentDealerConfigPut(GenericRequest<PaymentDealerConfigPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettingsGetOutModel>> PaymentSettingsGet(GenericRequest<PaymentSettingsGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<PaymentSettingsPutOutModel>> PaymentSettingsPut(GenericRequest<PaymentSettingsPutInModel> model, Boolean isSubProcess);
    }
}
