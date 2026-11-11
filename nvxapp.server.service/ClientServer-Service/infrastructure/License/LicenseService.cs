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
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /*
     Gestione delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §4, §6, §10, §12).
        - impostazioni e abilitazione dei centri a gestire i listini: solo SuperUser
        - listini globali: SuperUser; listini del centro: SuperUser o DealerPowerAdmin del centro abilitato (PuoGestireListini);
          gli altri utenti del centro vedono in sola lettura i listini globali e quelli del proprio centro
        - licenze del soggetto: l'azienda (CompanyPowerAdmin sottoscrive e rinnova, CompanyAdmin vede) e lo studio
          (FinancialAdvisorPowerAdmin sottoscrive e rinnova, FinancialAdvisorAdmin vede)
     Ruoli, centro, studio e azienda sono quelli del token, mai dal client.
    */
    public class LicenseService : ServiceBase, ILicenseService
    {
        private static readonly AmbitoLicenza[] AmbitiOfferta =
            { AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement, AmbitoLicenza.Cumulativo };

        private readonly ILic_ListinoRepository _listinoRepository;
        private readonly ILic_PianoRepository _pianoRepository;
        private readonly ILic_AbbonamentoRepository _abbonamentoRepository;
        private readonly ILic_PagamentoRepository _pagamentoRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IApplicationActivationAuthorizer _authorizer;
        private readonly ILicenseSettingsProvider _settingsProvider;
        private readonly ILicenseGuard _licenseGuard;
        private readonly ILicenseCatalogResolver _resolver;
        private readonly ILicenseSubscriptionManager _subscriptionManager;
        private readonly IPaymentGatewayResolver _gatewayResolver;
        private readonly ICompanyApplicationService _companyApplicationService;
        private readonly IStorageQuotaService _storageQuotaService;

        public LicenseService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,
                              ILic_ListinoRepository listinoRepository,
                              ILic_PianoRepository pianoRepository,
                              ILic_AbbonamentoRepository abbonamentoRepository,
                              ILic_PagamentoRepository pagamentoRepository,
                              IDealerRepository dealerRepository,
                              ICompanyRepository companyRepository,
                              IApplicationActivationAuthorizer authorizer,
                              ILicenseSettingsProvider settingsProvider,
                              ILicenseGuard licenseGuard,
                              ILicenseCatalogResolver resolver,
                              ILicenseSubscriptionManager subscriptionManager,
                              IPaymentGatewayResolver gatewayResolver,
                              ICompanyApplicationService companyApplicationService,
                              IStorageQuotaService storageQuotaService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _listinoRepository = listinoRepository;
            _pianoRepository = pianoRepository;
            _abbonamentoRepository = abbonamentoRepository;
            _pagamentoRepository = pagamentoRepository;
            _dealerRepository = dealerRepository;
            _companyRepository = companyRepository;
            _authorizer = authorizer;
            _settingsProvider = settingsProvider;
            _licenseGuard = licenseGuard;
            _resolver = resolver;
            _subscriptionManager = subscriptionManager;
            _gatewayResolver = gatewayResolver;
            _companyApplicationService = companyApplicationService;
            _storageQuotaService = storageQuotaService;
        }

        // ---------------------------------------------------------------------------
        // Impostazioni (SuperUser)

        public virtual async Task<GenericResult<LicenseSettingsGetOutModel>> LicenseSettingsGet(GenericRequest<LicenseSettingsGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                return new LicenseSettingsGetOutModel
                {
                    Settings = SettingsModel(await _settingsProvider.GetAsync()),
                    Dealer = await DealersAsync(),
                    DebugPagamenti = _gatewayResolver.DebugMode
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseSettingsPutOutModel>> LicenseSettingsPut(GenericRequest<LicenseSettingsPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var s = model.Data.Settings;
                var limiti = new[] { s.MaxUtentiFree, s.MaxUtentiStandard, s.MaxUtentiPro, s.MaxClientiFree, s.MaxClientiStandard, s.MaxClientiPro,
                                     s.MaxSpazioMBFree, s.MaxSpazioMBStandard, s.MaxSpazioMBPro };
                if (limiti.Any(x => x.HasValue && x <= 0) || s.GraceGiorni < 0 || s.GraceGiorni > 365 || s.AvvisoGiorni < 0 || s.AvvisoGiorni > 365)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseSettingsInvalid"));

                await _settingsProvider.SaveAsync(new LicenseSettings
                {
                    Attive = s.Attive,
                    MaxUtentiFree = s.MaxUtentiFree,
                    MaxUtentiStandard = s.MaxUtentiStandard,
                    MaxUtentiPro = s.MaxUtentiPro,
                    MaxClientiFree = s.MaxClientiFree,
                    MaxClientiStandard = s.MaxClientiStandard,
                    MaxClientiPro = s.MaxClientiPro,
                    MaxSpazioMBFree = s.MaxSpazioMBFree,
                    MaxSpazioMBStandard = s.MaxSpazioMBStandard,
                    MaxSpazioMBPro = s.MaxSpazioMBPro,
                    GraceGiorni = s.GraceGiorni,
                    AvvisoGiorni = s.AvvisoGiorni
                });

                foreach (var d in model.Data.Dealer)
                {
                    var abilitato = d.PuoGestireListini;
                    await _dealerRepository.FindAll(x => x.Id == d.IdDealer && x.PuoGestireListini != abilitato)
                                           .ExecuteUpdateAsync(u => u.SetProperty(x => x.PuoGestireListini, abilitato));
                }

                return new LicenseSettingsPutOutModel
                {
                    Settings = SettingsModel(await _settingsProvider.GetAsync()),
                    Dealer = await DealersAsync(),
                    DebugPagamenti = _gatewayResolver.DebugMode
                };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Listini

        public virtual async Task<GenericResult<LicenseCatalogListOutModel>> LicenseCatalogList(GenericRequest<LicenseCatalogListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoListini();
                var query = accesso.SuperUser
                            ? _listinoRepository.FindAll(x => true)
                            : _listinoRepository.FindAll(x => x.IdDealer == null || x.IdDealer == accesso.IdDealer);
                var listini = await query.Include(x => x.DealerNavigation).Include(x => x.Lic_Piano)
                                         .OrderBy(x => x.IdDealer.HasValue).ThenBy(x => x.DealerNavigation!.Descrizione)
                                         .ThenBy(x => x.TipoSoggetto).ThenBy(x => x.Ambito)
                                         .ToListAsync();
                var usati = await AbbonamentiPerListino();

                var retVal = new LicenseCatalogListOutModel { IsSuperUser = accesso.SuperUser, CanCreate = accesso.SuperUser || accesso.PuoGestire };
                foreach (var l in listini)
                    retVal.Listino.Add(CatalogModel(l, usati, accesso));
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseCatalogGetOutModel>> LicenseCatalogGet(GenericRequest<LicenseCatalogGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoListini();
                var retVal = new LicenseCatalogGetOutModel { IsSuperUser = accesso.SuperUser };
                if (accesso.SuperUser)
                    retVal.Dealer = await DealersAsync();

                if (model.Data.Id <= 0)
                {
                    if (!accesso.SuperUser && !accesso.PuoGestire)
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                    retVal.Listino = await NuovoListinoAsync(accesso.SuperUser ? null : accesso.IdDealer);
                    return retVal;
                }

                var listino = await _listinoRepository.FindAll(x => x.Id == model.Data.Id).Include(x => x.DealerNavigation).Include(x => x.Lic_Piano).FirstOrDefaultAsync()
                              ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogNotFound"));
                if (!accesso.SuperUser && listino.IdDealer != null && listino.IdDealer != accesso.IdDealer)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

                retVal.Listino = CatalogModel(listino, await AbbonamentiPerListino(), accesso);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseCatalogPutOutModel>> LicenseCatalogPut(GenericRequest<LicenseCatalogPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoListini();
                var dati = model.Data.Listino;

                // validazione
                if (string.IsNullOrWhiteSpace(dati.Descrizione) || dati.Descrizione.Trim().Length > 100
                    || !Enum.IsDefined(typeof(TipoSoggettoLicenza), dati.TipoSoggetto) || !Enum.IsDefined(typeof(AmbitoLicenza), dati.Ambito))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogInvalid"));
                var livelli = Enum.GetValues<LivelloLicenza>().Select(x => (int)x).ToList();
                if (dati.Piano.Count != livelli.Count || livelli.Any(l => dati.Piano.Count(p => p.Livello == l) != 1))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogInvalid"));
                foreach (var p in dati.Piano)
                {
                    if (string.IsNullOrWhiteSpace(p.Codice) || p.Codice.Trim().Length > 20 || (p.Descrizione?.Length ?? 0) > 100
                        || p.PrezzoMensile < 0 || p.PrezzoAnnuale < 0 || (p.MaxUtenti.HasValue && p.MaxUtenti <= 0)
                        || (p.MaxClienti.HasValue && p.MaxClienti <= 0) || (p.MaxSpazioMB.HasValue && p.MaxSpazioMB <= 0))
                        throw new UserMessageException(InfrastructureMessages.Get("LicensePlanInvalid", p.Codice));
                    if (p.Livello == (int)LivelloLicenza.Free && (p.PrezzoMensile != 0 || p.PrezzoAnnuale != 0))
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseFreeNotZero"));
                }

                // proprietario e permessi
                Lic_Listino listino;
                if (dati.Id > 0)
                {
                    listino = await _listinoRepository.FindAll(x => x.Id == dati.Id).Include(x => x.DealerNavigation).FirstOrDefaultAsync()
                              ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogNotFound"));
                    if (!PuoModificare(listino.IdDealer, listino.DealerNavigation?.PuoGestireListini ?? false, accesso))
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

                    bool usato = await _abbonamentoRepository.FindAll(x => x.IdListino == listino.Id).AnyAsync();
                    if (usato && ((int)listino.TipoSoggetto != dati.TipoSoggetto || (int)listino.Ambito != dati.Ambito))
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogInUse"));
                    listino.DealerNavigation = null;
                }
                else
                {
                    int? idDealer = accesso.SuperUser ? dati.IdDealer : accesso.IdDealer;
                    if (!accesso.SuperUser && !accesso.PuoGestire)
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                    if (idDealer != null && !await _dealerRepository.FindAll(x => x.Id == idDealer).AnyAsync())
                        throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogInvalid"));
                    listino = new Lic_Listino { IdDealer = idDealer };
                }

                var tipo = (TipoSoggettoLicenza)dati.TipoSoggetto;
                var ambito = (AmbitoLicenza)dati.Ambito;
                if (dati.Attivo && await _listinoRepository.FindAll(x => x.Attivo && x.Id != listino.Id && x.IdDealer == listino.IdDealer
                                                                         && x.TipoSoggetto == tipo && x.Ambito == ambito).AnyAsync())
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogDuplicate"));

                listino.TipoSoggetto = tipo;
                listino.Ambito = ambito;
                listino.Descrizione = dati.Descrizione.Trim();
                listino.Valuta = "EUR";
                listino.DisponibileSuCompany = tipo == TipoSoggettoLicenza.FinancialAdvisor && dati.DisponibileSuCompany;
                listino.Attivo = dati.Attivo;

                var piani = listino.Id > 0 ? await _pianoRepository.FindAll(x => x.IdListino == listino.Id).ToListAsync() : new List<Lic_Piano>();
                await using (var transaction = await _listinoRepository.BeginTransactionAsync())
                {
                    listino = await _listinoRepository.UpsertAsync(listino);
                    foreach (var p in dati.Piano)
                    {
                        var piano = piani.FirstOrDefault(x => (int)x.Livello == p.Livello) ?? new Lic_Piano { IdListino = listino.Id, Livello = (LivelloLicenza)p.Livello };
                        piano.Codice = p.Codice.Trim();
                        piano.Descrizione = string.IsNullOrWhiteSpace(p.Descrizione) ? null : p.Descrizione.Trim();
                        piano.Ordine = p.Livello * 10;
                        piano.PrezzoMensile = Math.Round(p.PrezzoMensile, 2);
                        piano.PrezzoAnnuale = Math.Round(p.PrezzoAnnuale, 2);
                        // limiti: solo quelli della funzione del listino (il Cumulativo li ha tutti), gli altri non servono (§5.1)
                        piano.MaxUtenti = LicenseDefaults.UsaLimite(ambito, AmbitoLicenza.AttendanceTracking) ? p.MaxUtenti : null;
                        piano.MaxClienti = LicenseDefaults.UsaLimite(ambito, AmbitoLicenza.Crm) ? p.MaxClienti : null;
                        piano.MaxSpazioMB = LicenseDefaults.UsaLimite(ambito, AmbitoLicenza.DocumentManagement) ? p.MaxSpazioMB : null;
                        // il piano Free resta sempre disponibile
                        piano.Attivo = p.Livello == (int)LivelloLicenza.Free || p.Attivo;
                        await _pianoRepository.UpsertAsync(piano);
                    }
                    await transaction.CommitAsync();
                }

                // limiti alzati: valgono subito per gli abbonamenti attivi; abbassati: dal rinnovo (§5.1)
                foreach (var p in await _pianoRepository.FindAll(x => x.IdListino == listino.Id).ToListAsync())
                {
                    var idPiano = p.Id;
                    var attivi = _abbonamentoRepository.FindAll(x => x.IdPiano == idPiano && x.Stato == StatoAbbonamento.Attivo);
                    int? utenti = p.MaxUtenti, clienti = p.MaxClienti, spazio = p.MaxSpazioMB;
                    if (utenti == null)
                        await attivi.Where(x => x.MaxUtentiBloccato != null).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxUtentiBloccato, (int?)null));
                    else
                        await attivi.Where(x => x.MaxUtentiBloccato != null && x.MaxUtentiBloccato < utenti).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxUtentiBloccato, utenti));
                    if (clienti == null)
                        await attivi.Where(x => x.MaxClientiBloccato != null).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxClientiBloccato, (int?)null));
                    else
                        await attivi.Where(x => x.MaxClientiBloccato != null && x.MaxClientiBloccato < clienti).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxClientiBloccato, clienti));
                    if (spazio == null)
                        await attivi.Where(x => x.MaxSpazioMBBloccato != null).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxSpazioMBBloccato, (int?)null));
                    else
                        await attivi.Where(x => x.MaxSpazioMBBloccato != null && x.MaxSpazioMBBloccato < spazio).ExecuteUpdateAsync(u => u.SetProperty(x => x.MaxSpazioMBBloccato, spazio));
                }

                var salvato = await _listinoRepository.FindAll(x => x.Id == listino.Id).Include(x => x.DealerNavigation).Include(x => x.Lic_Piano).FirstAsync();
                return new LicenseCatalogPutOutModel { Listino = CatalogModel(salvato, await AbbonamentiPerListino(), accesso) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseCatalogDeleteOutModel>> LicenseCatalogDelete(GenericRequest<LicenseCatalogDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var accesso = await AccessoListini();
                var listino = await _listinoRepository.FindAll(x => x.Id == model.Data.Id).Include(x => x.DealerNavigation).FirstOrDefaultAsync()
                              ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseCatalogNotFound"));
                if (!PuoModificare(listino.IdDealer, listino.DealerNavigation?.PuoGestireListini ?? false, accesso))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                listino.DealerNavigation = null;

                var retVal = new LicenseCatalogDeleteOutModel();
                if (await _abbonamentoRepository.FindAll(x => x.IdListino == listino.Id).AnyAsync())
                {
                    // usato da abbonamenti: si disattiva (gli abbonamenti restano validi fino alla scadenza)
                    listino.Attivo = false;
                    await _listinoRepository.UpdateAsync(listino);
                    retVal.Deleted = false;
                }
                else
                {
                    await _listinoRepository.DeleteAsync(listino);
                    retVal.Deleted = true;
                }
                return retVal;
            }, isSubProcess);
        }

        // listini globali del SuperUser (IdDealer null) attivi, con i piani attivi: pagina prezzi dello showcase, senza login.
        // I listini dei centri non si espongono: valgono solo per i loro soggetti, dopo il login.
        public virtual async Task<GenericResult<LicensePublicPriceListOutModel>> LicensePublicPriceList(GenericRequest<LicensePublicPriceListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ambiti = (model.Data?.Ambito ?? new List<int>())
                             .Where(a => Enum.IsDefined(typeof(AmbitoLicenza), a))
                             .Select(a => (AmbitoLicenza)a).Distinct().ToList();
                if (ambiti.Count == 0)
                    ambiti = AmbitiOfferta.ToList();

                var listini = await _listinoRepository.FindAll(x => x.Attivo && x.IdDealer == null && ambiti.Contains(x.Ambito))
                                                      .Include(x => x.Lic_Piano)
                                                      .OrderBy(x => x.Ambito).ThenBy(x => x.TipoSoggetto)
                                                      .ToListAsync();

                var retVal = new LicensePublicPriceListOutModel();
                foreach (var l in listini)
                {
                    var piani = (l.Lic_Piano ?? new List<Lic_Piano>()).Where(p => p.Attivo).OrderBy(p => p.Ordine).ThenBy(p => p.Livello)
                                .Select(PlanModel).ToList();
                    if (piani.Count == 0)
                        continue;
                    piani.ForEach(p => p.Id = 0);
                    retVal.Listino.Add(new LicensePublicCatalogModel
                    {
                        TipoSoggetto = (int)l.TipoSoggetto,
                        Ambito = (int)l.Ambito,
                        Descrizione = l.Descrizione,
                        Valuta = l.Valuta,
                        DisponibileSuCompany = l.DisponibileSuCompany,
                        Piano = piani
                    });
                }
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Licenze del soggetto

        public virtual async Task<GenericResult<LicenseStatusGetOutModel>> LicenseStatusGet(GenericRequest<LicenseStatusGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (soggetto, canManage) = await SoggettoCorrente();
                var settings = await _settingsProvider.GetAsync();
                var oggi = DateTime.Today;

                var retVal = new LicenseStatusGetOutModel
                {
                    TipoSoggetto = (int)soggetto.Tipo,
                    Soggetto = soggetto.Descrizione,
                    Studio = soggetto.Studio,
                    Attive = settings.Attive,
                    CanManage = canManage,
                    DebugPagamenti = _gatewayResolver.DebugMode,
                    Metodo = _gatewayResolver.MetodiDisponibili().Select(x => (int)x).ToList(),
                    GraceGiorni = settings.GraceGiorni
                };

                // copertura effettiva per funzione, con il limite della funzione e l'uso attuale (§5.1):
                // Presenze -> utenti, CRM -> clienti attivi, Documentale -> spazio occupato (solo per le aziende)
                bool azienda = soggetto.Tipo == TipoSoggettoLicenza.Company;
                retVal.UtentiAttivi = azienda ? await _licenseGuard.ActiveUsersAsync(soggetto.Id) : await _licenseGuard.StudioActiveUsersAsync(soggetto.Id);
                foreach (var ambito in LicenseGuard.Funzioni)
                {
                    var c = azienda ? await _licenseGuard.CoverageAsync(soggetto.Id, ambito, reale: true)
                                    : await _licenseGuard.StudioCoverageAsync(soggetto.Id, ambito, reale: true);
                    bool coperta = c.Livello != null;
                    var f = new LicenseFunctionStatusModel
                    {
                        Ambito = (int)ambito,
                        Stato = (int)c.Stato,
                        Livello = (int?)c.Livello,
                        DescrizionePiano = c.DescrizionePiano,
                        Fonte = (int)c.Fonte,
                        Studio = c.Studio,
                        DataScadenza = c.DataScadenza
                    };
                    switch (ambito)
                    {
                        case AmbitoLicenza.AttendanceTracking:
                            f.MaxUtenti = coperta ? c.MaxUtenti : null;
                            f.OltreLimite = coperta && f.MaxUtenti != null && retVal.UtentiAttivi > f.MaxUtenti;
                            retVal.LimiteUtenti = f.MaxUtenti;
                            break;
                        case AmbitoLicenza.Crm:
                            f.MaxClienti = coperta ? c.MaxClienti : null;
                            if (azienda)
                            {
                                f.ClientiAttivi = await _licenseGuard.ActiveClientsAsync(soggetto.Id);
                                f.OltreLimite = coperta && f.MaxClienti != null && f.ClientiAttivi > f.MaxClienti;
                            }
                            break;
                        case AmbitoLicenza.DocumentManagement:
                            f.MaxSpazioMB = coperta ? c.MaxSpazioMB : null;
                            if (azienda)
                            {
                                f.SpazioUsatoMB = (await _storageQuotaService.StatusAsync(soggetto.Id)).UsedBytes / (1024L * 1024L);
                                f.OltreLimite = coperta && f.MaxSpazioMB != null && f.SpazioUsatoMB > f.MaxSpazioMB;
                            }
                            break;
                    }
                    retVal.Funzione.Add(f);
                }

                // Cumulativo: la copertura per l'ambito Cumulativo considera solo le licenze cumulative (proprie o dello studio)
                var cum = azienda ? await _licenseGuard.CoverageAsync(soggetto.Id, AmbitoLicenza.Cumulativo, reale: true)
                                  : await _licenseGuard.StudioCoverageAsync(soggetto.Id, AmbitoLicenza.Cumulativo, reale: true);
                retVal.Cumulativo = new LicenseFunctionStatusModel
                {
                    Ambito = (int)AmbitoLicenza.Cumulativo,
                    Stato = (int)cum.Stato,
                    Livello = (int?)cum.Livello,
                    DescrizionePiano = cum.DescrizionePiano,
                    Fonte = (int)cum.Fonte,
                    Studio = cum.Studio,
                    DataScadenza = cum.DataScadenza,
                    MaxUtenti = cum.MaxUtenti,
                    MaxClienti = cum.MaxClienti,
                    MaxSpazioMB = cum.MaxSpazioMB
                };

                // abbonamenti propri (gli annullati non si mostrano)
                var propri = await AbbonamentiSoggetto(soggetto).Where(x => x.Stato != StatoAbbonamento.Annullato)
                                                                .OrderBy(x => x.Ambito).ThenByDescending(x => x.DataScadenza)
                                                                .ToListAsync();
                retVal.Abbonamento = propri.Select(x => SubscriptionModel(x, settings)).ToList();

                // offerte: listino risolto per funzione; l'azienda coperta dallo studio vede solo i piani superiori (§4.3)
                foreach (var ambito in AmbitiOfferta)
                {
                    var listino = await _resolver.ResolveListinoAsync(soggetto, ambito);
                    if (listino == null)
                        continue;
                    LivelloLicenza? delloStudio = azienda && soggetto.IdFinancialAdvisor != null
                                                  ? await _subscriptionManager.StudioCoverageLevelAsync(soggetto.IdFinancialAdvisor.Value, ambito)
                                                  : null;
                    var offerta = new LicenseOfferModel
                    {
                        Ambito = (int)ambito,
                        IdListino = listino.Id,
                        Descrizione = listino.Descrizione,
                        Valuta = listino.Valuta,
                        DisponibileSuCompany = listino.DisponibileSuCompany,
                        Piano = (listino.Lic_Piano ?? new List<Lic_Piano>())
                                .Where(p => p.Attivo && (delloStudio == null || p.Livello > delloStudio))
                                .OrderBy(p => p.Livello)
                                .Select(PlanModel)
                                .ToList()
                    };
                    if (offerta.Piano.Count > 0)
                        retVal.Offerta.Add(offerta);
                }

                // storico pagamenti
                var pagamenti = azienda ? _pagamentoRepository.FindAll(x => x.IdCompany == soggetto.Id)
                                        : _pagamentoRepository.FindAll(x => x.IdFinancialAdvisor == soggetto.Id);
                retVal.Pagamento = (await pagamenti.OrderByDescending(x => x.CreationDate).Take(50).ToListAsync())
                                   .Select(x => new LicensePaymentModel
                                   {
                                       Id = x.Id,
                                       Data = x.CreationDate ?? x.DataInizioCoperta,
                                       Ambito = (int)x.Ambito,
                                       Livello = (int)x.Livello,
                                       Importo = x.Importo,
                                       Valuta = x.Valuta,
                                       Periodicita = (int)x.Periodicita,
                                       DataInizioCoperta = x.DataInizioCoperta,
                                       DataFineCoperta = x.DataFineCoperta,
                                       Metodo = (int)x.Metodo,
                                       Stato = (int)x.Stato,
                                       Debug = x.Debug
                                   })
                                   .ToList();

                // studio: aziende coperte dalle sue licenze, con quelle che hanno un piano proprio superiore
                if (!azienda)
                {
                    var limite = oggi.AddDays(-settings.GraceGiorni);
                    var condivisi = propri.Where(x => x.Stato == StatoAbbonamento.Attivo && x.DisponibileSuCompany && x.DataScadenza >= limite).ToList();
                    if (condivisi.Count > 0)
                    {
                        var aziende = await _companyRepository.FindAll(x => x.IdFinancialAdvisor == soggetto.Id).OrderBy(x => x.Descrizione)
                                                              .Select(x => new { x.Id, x.Descrizione }).ToListAsync();
                        var idAziende = aziende.Select(x => x.Id).ToList();
                        var proprieAziende = await _abbonamentoRepository.FindAll(x => x.IdCompany != null && idAziende.Contains(x.IdCompany.Value)
                                                                                       && x.Stato == StatoAbbonamento.Attivo && x.DataScadenza >= limite)
                                                                         .Select(x => new { x.IdCompany, x.Ambito, x.Livello })
                                                                         .ToListAsync();
                        foreach (var a in aziende)
                        {
                            var riga = new LicenseCoveredCompanyModel { IdCompany = a.Id, Azienda = a.Descrizione ?? string.Empty };
                            foreach (var ambito in LicenseGuard.Funzioni)
                            {
                                var studio = condivisi.Where(x => x.Ambito == ambito || x.Ambito == AmbitoLicenza.Cumulativo).Select(x => (int?)x.Livello).Max();
                                var propria = proprieAziende.Where(x => x.IdCompany == a.Id && (x.Ambito == ambito || x.Ambito == AmbitoLicenza.Cumulativo)).Select(x => (int?)x.Livello).Max();
                                if (studio != null && propria != null && propria > studio)
                                    riga.AmbitoSuperiore.Add((int)ambito);
                            }
                            retVal.AziendaCoperta.Add(riga);
                        }
                    }
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseSubscribeOutModel>> LicenseSubscribe(GenericRequest<LicenseSubscribeInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (soggetto, canManage) = await SoggettoCorrente();
                if (!canManage)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

                var dati = model.Data;
                if (!Enum.IsDefined(typeof(AmbitoLicenza), dati.Ambito) || !Enum.IsDefined(typeof(LivelloLicenza), dati.Livello)
                    || !Enum.IsDefined(typeof(PeriodicitaLicenza), dati.Periodicita) || !Enum.IsDefined(typeof(MetodoPagamento), dati.Metodo))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseSubscribeInvalid"));

                var esito = await _subscriptionManager.SubscribeAsync(soggetto, (AmbitoLicenza)dati.Ambito, (LivelloLicenza)dati.Livello,
                                                                      (PeriodicitaLicenza)dati.Periodicita, (MetodoPagamento)dati.Metodo,
                                                                      dati.RinnovoAutomatico, dati.ReturnUrl);
                var retVal = new LicenseSubscribeOutModel
                {
                    Abbonamento = SubscriptionModel(esito.Abbonamento, await _settingsProvider.GetAsync()),
                    RedirectUrl = esito.RedirectUrl,
                    Debug = esito.Pagamento?.Debug ?? false
                };
                if (retVal.RedirectUrl == null)
                    retVal.Messages.Add(new Message(InfrastructureMessages.Get(retVal.Debug ? "LicenseSubscribedDebug" : "LicenseSubscribed"), MessageType.Information));

                // azienda che si sottoscrive da se' la licenza: si attivano gli applicativi coperti, con schema e dati iniziali
                // come in companyedit (ANALISI_INFRASTRUCTURE_LICENZE.md §15.6). Un errore non annulla la sottoscrizione: si avvisa.
                if (soggetto.Tipo == TipoSoggettoLicenza.Company && esito.Abbonamento.Stato == StatoAbbonamento.Attivo)
                {
                    try
                    {
                        var attivati = await _companyApplicationService.ActivateForLicenseAsync(soggetto.Id, esito.Abbonamento.Ambito);
                        if (attivati.Count > 0)
                            retVal.Messages.Add(new Message(InfrastructureMessages.Get("LicenseApplicationsActivated",
                                string.Join(", ", attivati.Select(a => LicenseSubscriptionManager.NomeFunzione((AmbitoLicenza)(int)a)))), MessageType.Information));
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error(ex, "Attivazione degli applicativi per la licenza {Id} non riuscita", esito.Abbonamento.Id);
                        retVal.Messages.Add(new Message(InfrastructureMessages.Get("LicenseApplicationsActivationFailed"), MessageType.Warning));
                    }
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseRenewOutModel>> LicenseRenew(GenericRequest<LicenseRenewInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (soggetto, canManage) = await SoggettoCorrente();
                if (!canManage)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

                var abbonamento = await AbbonamentiSoggetto(soggetto).FirstOrDefaultAsync(x => x.Id == model.Data.IdAbbonamento)
                                  ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseSubscriptionNotFound"));
                var esito = await _subscriptionManager.RenewAsync(abbonamento, automatico: false, model.Data.ReturnUrl)
                            ?? throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentNotConfigured"));
                var retVal = new LicenseRenewOutModel
                {
                    Abbonamento = SubscriptionModel(esito.Abbonamento, await _settingsProvider.GetAsync()),
                    RedirectUrl = esito.RedirectUrl,
                    Debug = esito.Pagamento?.Debug ?? false
                };
                if (retVal.RedirectUrl == null)
                    retVal.Messages.Add(new Message(InfrastructureMessages.Get("LicenseRenewed", esito.Abbonamento.DataScadenza.ToString("dd/MM/yyyy")), MessageType.Information));
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LicenseAutoRenewPutOutModel>> LicenseAutoRenewPut(GenericRequest<LicenseAutoRenewPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (soggetto, canManage) = await SoggettoCorrente();
                if (!canManage)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

                var abbonamento = await AbbonamentiSoggetto(soggetto).FirstOrDefaultAsync(x => x.Id == model.Data.IdAbbonamento)
                                  ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseSubscriptionNotFound"));
                if (abbonamento.Stato != StatoAbbonamento.Attivo)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotRenewable"));
                abbonamento.RinnovoAutomatico = model.Data.RinnovoAutomatico;
                abbonamento = await _abbonamentoRepository.UpdateAsync(abbonamento);
                return new LicenseAutoRenewPutOutModel { Abbonamento = SubscriptionModel(abbonamento, await _settingsProvider.GetAsync()) };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        private sealed class Accesso
        {
            public bool SuperUser { get; set; }
            public int? IdDealer { get; set; }
            public bool DealerPowerAdmin { get; set; }
            public bool PuoGestire { get; set; }
        }

        // chi vede i listini: SuperUser e utenti del centro (DealerPowerAdmin, DealerAdmin)
        private async Task<Accesso> AccessoListini()
        {
            var accesso = new Accesso { SuperUser = await _authorizer.IsSuperUserAsync() };
            if (accesso.SuperUser)
                return accesso;

            bool powerAdmin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerPowerAdmin));
            bool admin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerAdmin));
            if ((!powerAdmin && !admin) || !int.TryParse(CurrentDealer, out var idDealer) || idDealer <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));

            accesso.IdDealer = idDealer;
            accesso.DealerPowerAdmin = powerAdmin;
            accesso.PuoGestire = powerAdmin && await _dealerRepository.FindAll(x => x.Id == idDealer && x.PuoGestireListini).AnyAsync();
            return accesso;
        }

        private static bool PuoModificare(int? idDealerListino, bool dealerAbilitato, Accesso accesso)
            => accesso.SuperUser
               || (idDealerListino != null && idDealerListino == accesso.IdDealer && accesso.DealerPowerAdmin && dealerAbilitato);

        // soggetto dell'utente: azienda (utenti dell'azienda) oppure studio (utenti dello studio)
        private async Task<(LicenseSubject Soggetto, bool CanManage)> SoggettoCorrente()
        {
            if (int.TryParse(CurrentCompany, out var idCompany) && idCompany > 0)
            {
                bool powerAdmin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.CompanyPowerAdmin));
                if (!powerAdmin && !await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.CompanyAdmin)))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                var soggetto = await _resolver.CompanySubjectAsync(idCompany)
                               ?? throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));
                return (soggetto, powerAdmin);
            }

            if (int.TryParse(CurrentFinancialAdvisor, out var idStudio) && idStudio > 0)
            {
                bool powerAdmin = await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorPowerAdmin));
                if (!powerAdmin && !await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorAdmin)))
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                var soggetto = await _resolver.StudioSubjectAsync(idStudio)
                               ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
                return (soggetto, powerAdmin);
            }

            throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
        }

        private IQueryable<Lic_Abbonamento> AbbonamentiSoggetto(LicenseSubject soggetto)
            => soggetto.Tipo == TipoSoggettoLicenza.Company
               ? _abbonamentoRepository.FindAll(x => x.IdCompany == soggetto.Id)
               : _abbonamentoRepository.FindAll(x => x.IdFinancialAdvisor == soggetto.Id);

        private async Task<Dictionary<int, int>> AbbonamentiPerListino()
            => await _abbonamentoRepository.FindAll(x => x.IdListino != null && (x.Stato == StatoAbbonamento.Attivo || x.Stato == StatoAbbonamento.InAttesa))
                                           .GroupBy(x => x.IdListino!.Value)
                                           .Select(g => new { g.Key, Count = g.Count() })
                                           .ToDictionaryAsync(x => x.Key, x => x.Count);

        private async Task<LicenseCatalogModel> NuovoListinoAsync(int? idDealer)
        {
            var settings = await _settingsProvider.GetAsync();
            return new LicenseCatalogModel
            {
                IdDealer = idDealer,
                TipoSoggetto = (int)TipoSoggettoLicenza.Company,
                Ambito = (int)AmbitoLicenza.AttendanceTracking,
                Attivo = true,
                CanEdit = true,
                Piano = Enum.GetValues<LivelloLicenza>().Select(l => new LicensePlanModel
                {
                    Livello = (int)l,
                    Codice = l.ToString(),
                    Ordine = (int)l * 10,
                    MaxUtenti = settings.MaxUtenti(l),
                    MaxClienti = settings.MaxClienti(l),
                    MaxSpazioMB = settings.MaxSpazioMB(l),
                    Attivo = true
                }).ToList()
            };
        }

        private static LicenseCatalogModel CatalogModel(Lic_Listino l, Dictionary<int, int> usati, Accesso accesso) => new()
        {
            Id = l.Id,
            IdDealer = l.IdDealer,
            Dealer = l.DealerNavigation?.Descrizione,
            TipoSoggetto = (int)l.TipoSoggetto,
            Ambito = (int)l.Ambito,
            Descrizione = l.Descrizione,
            Valuta = l.Valuta,
            DisponibileSuCompany = l.DisponibileSuCompany,
            Attivo = l.Attivo,
            Piano = (l.Lic_Piano ?? new List<Lic_Piano>()).OrderBy(p => p.Livello).Select(PlanModel).ToList(),
            Abbonamenti = usati.TryGetValue(l.Id, out var n) ? n : 0,
            CanEdit = PuoModificare(l.IdDealer, l.DealerNavigation?.PuoGestireListini ?? false, accesso)
        };

        private static LicensePlanModel PlanModel(Lic_Piano p) => new()
        {
            Id = p.Id,
            Livello = (int)p.Livello,
            Codice = p.Codice,
            Descrizione = p.Descrizione,
            Ordine = p.Ordine,
            PrezzoMensile = p.PrezzoMensile,
            PrezzoAnnuale = p.PrezzoAnnuale,
            MaxUtenti = p.MaxUtenti,
            MaxClienti = p.MaxClienti,
            MaxSpazioMB = p.MaxSpazioMB,
            Attivo = p.Attivo
        };

        private static LicenseSubscriptionModel SubscriptionModel(Lic_Abbonamento x, LicenseSettings settings)
        {
            var oggi = DateTime.Today;
            return new LicenseSubscriptionModel
            {
                Id = x.Id,
                Ambito = (int)x.Ambito,
                Livello = (int)x.Livello,
                DescrizionePiano = x.DescrizionePiano,
                Periodicita = (int)x.Periodicita,
                Prezzo = x.PrezzoBloccato,
                Valuta = x.Valuta,
                MaxUtenti = x.MaxUtentiBloccato,
                MaxClienti = x.MaxClientiBloccato,
                MaxSpazioMB = x.MaxSpazioMBBloccato,
                DataInizio = x.DataInizio,
                DataScadenza = x.DataScadenza,
                Stato = (int)x.Stato,
                RinnovoAutomatico = x.RinnovoAutomatico,
                Metodo = (int)x.Metodo,
                DisponibileSuCompany = x.DisponibileSuCompany,
                InGrace = x.Stato == StatoAbbonamento.Attivo && oggi > x.DataScadenza.Date && oggi <= LicenseGuard.FineValidita(x.DataScadenza, settings.GraceGiorni),
                InScadenza = x.Stato == StatoAbbonamento.Attivo && oggi <= x.DataScadenza.Date && x.DataScadenza.Date <= oggi.AddDays(settings.AvvisoGiorni)
            };
        }

        private static LicenseSettingsModel SettingsModel(LicenseSettings s) => new()
        {
            Attive = s.Attive,
            MaxUtentiFree = s.MaxUtentiFree,
            MaxUtentiStandard = s.MaxUtentiStandard,
            MaxUtentiPro = s.MaxUtentiPro,
            MaxClientiFree = s.MaxClientiFree,
            MaxClientiStandard = s.MaxClientiStandard,
            MaxClientiPro = s.MaxClientiPro,
            MaxSpazioMBFree = s.MaxSpazioMBFree,
            MaxSpazioMBStandard = s.MaxSpazioMBStandard,
            MaxSpazioMBPro = s.MaxSpazioMBPro,
            GraceGiorni = s.GraceGiorni,
            AvvisoGiorni = s.AvvisoGiorni
        };

        private async Task<List<LicenseDealerModel>> DealersAsync()
            => await _dealerRepository.FindAll(x => true).OrderBy(x => x.Descrizione)
                                      .Select(x => new LicenseDealerModel { IdDealer = x.Id, Descrizione = x.Descrizione ?? string.Empty, PuoGestireListini = x.PuoGestireListini })
                                      .ToListAsync();

        private async Task SoloSuperUser()
        {
            if (!await _authorizer.IsSuperUserAsync())
                throw new UserMessageException(InfrastructureMessages.Get("LicenseNotAllowed"));
        }
    }

    public interface ILicenseService : IServiceBase
    {
        Task<GenericResult<LicenseSettingsGetOutModel>> LicenseSettingsGet(GenericRequest<LicenseSettingsGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseSettingsPutOutModel>> LicenseSettingsPut(GenericRequest<LicenseSettingsPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseCatalogListOutModel>> LicenseCatalogList(GenericRequest<LicenseCatalogListInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseCatalogGetOutModel>> LicenseCatalogGet(GenericRequest<LicenseCatalogGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseCatalogPutOutModel>> LicenseCatalogPut(GenericRequest<LicenseCatalogPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseCatalogDeleteOutModel>> LicenseCatalogDelete(GenericRequest<LicenseCatalogDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicensePublicPriceListOutModel>> LicensePublicPriceList(GenericRequest<LicensePublicPriceListInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseStatusGetOutModel>> LicenseStatusGet(GenericRequest<LicenseStatusGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseSubscribeOutModel>> LicenseSubscribe(GenericRequest<LicenseSubscribeInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseRenewOutModel>> LicenseRenew(GenericRequest<LicenseRenewInModel> model, Boolean isSubProcess);
        Task<GenericResult<LicenseAutoRenewPutOutModel>> LicenseAutoRenewPut(GenericRequest<LicenseAutoRenewPutInModel> model, Boolean isSubProcess);
    }
}
