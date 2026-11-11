using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    public class SubscribeResult
    {
        public Lic_Abbonamento Abbonamento { get; set; } = null!;
        public Lic_Pagamento? Pagamento { get; set; }
        /// <summary>pagina del provider su cui portare l'utente; null = sottoscrizione gia' conclusa</summary>
        public string? RedirectUrl { get; set; }
    }

    /*
     Sottoscrizione, rinnovo e scadenza delle licenze (ANALISI_LICENZE.md §6, §7).
     Non dipende dall'utente corrente: i controlli su chi puo' farlo stanno in LicenseService; lo usa anche la manutenzione notturna.
        - il prezzo lo calcola sempre il server dal listino risolto (§7.3), mai dal client
        - i piani a prezzo 0 (Free) non passano dal pagamento
        - periodo: dal giorno di inizio all'ultimo giorno compreso (DataScadenza); il rinnovo riparte dal giorno dopo la
          scadenza se la licenza vale ancora (anche nella grace), altrimenti da oggi
        - cambio di piano sulla stessa funzione: il nuovo abbonamento parte da oggi e il precedente viene annullato
          (nessun calcolo pro-rata in questa fase)
        - al rinnovo si rifotografano prezzo, limiti (utenti, clienti, spazio) ed etichette dal piano, se esiste ancora ed e' attivo (§5.1)
    */
    public interface ILicenseSubscriptionManager
    {
        Task<SubscribeResult> SubscribeAsync(LicenseSubject soggetto, AmbitoLicenza ambito, LivelloLicenza livello, PeriodicitaLicenza periodicita,
                                             MetodoPagamento metodo, bool rinnovoAutomatico, string? returnUrl);

        /// <summary>rinnova; automatico = dalla manutenzione notturna (null = non rinnovabile adesso, es. gestito dal provider)</summary>
        Task<SubscribeResult?> RenewAsync(Lic_Abbonamento abbonamento, bool automatico, string? returnUrl);

        /// <summary>livello piu' alto con cui lo studio copre l'azienda per la funzione (null = nessuno)</summary>
        Task<LivelloLicenza?> StudioCoverageLevelAsync(int idFinancialAdvisor, AmbitoLicenza ambito);

        /// <summary>porta a Scaduto gli abbonamenti oltre la grace; restituisce quanti</summary>
        Task<int> ExpireAsync();

        /// <summary>rinnova gli abbonamenti con rinnovo automatico scaduti (gratuiti o in modalita' debug)</summary>
        Task<int> AutoRenewAsync();

        /// <summary>avvisi di scadenza via email ai PowerAdmin del soggetto</summary>
        Task<int> SendWarningsAsync();

        DateTime FinePeriodo(DateTime inizio, PeriodicitaLicenza periodicita);
    }

    public class LicenseSubscriptionManager : ILicenseSubscriptionManager
    {
        private readonly ILic_AbbonamentoRepository _abbonamentoRepository;
        private readonly ILic_PagamentoRepository _pagamentoRepository;
        private readonly ILic_PianoRepository _pianoRepository;
        private readonly ILicenseCatalogResolver _resolver;
        private readonly ILicenseSettingsProvider _settingsProvider;
        private readonly IPaymentGatewayResolver _gatewayResolver;
        private readonly IEmailService _emailService;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;

        public LicenseSubscriptionManager(ILic_AbbonamentoRepository abbonamentoRepository,
                                          ILic_PagamentoRepository pagamentoRepository,
                                          ILic_PianoRepository pianoRepository,
                                          ILicenseCatalogResolver resolver,
                                          ILicenseSettingsProvider settingsProvider,
                                          IPaymentGatewayResolver gatewayResolver,
                                          IEmailService emailService,
                                          IUserCompanyRepository userCompanyRepository,
                                          IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IAspNetRolesRepository aspNetRolesRepository,
                                          IAspNetUserRolesRepository aspNetUserRolesRepository,
                                          ICompanyRepository companyRepository,
                                          IFinancialAdvisorRepository financialAdvisorRepository)
        {
            _abbonamentoRepository = abbonamentoRepository;
            _pagamentoRepository = pagamentoRepository;
            _pianoRepository = pianoRepository;
            _resolver = resolver;
            _settingsProvider = settingsProvider;
            _gatewayResolver = gatewayResolver;
            _emailService = emailService;
            _userCompanyRepository = userCompanyRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _companyRepository = companyRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
        }

        public DateTime FinePeriodo(DateTime inizio, PeriodicitaLicenza periodicita)
            => (periodicita == PeriodicitaLicenza.Annuale ? inizio.Date.AddYears(1) : inizio.Date.AddMonths(1)).AddDays(-1);

        public async Task<SubscribeResult> SubscribeAsync(LicenseSubject soggetto, AmbitoLicenza ambito, LivelloLicenza livello, PeriodicitaLicenza periodicita,
                                                          MetodoPagamento metodo, bool rinnovoAutomatico, string? returnUrl)
        {
            var listino = await _resolver.ResolveListinoAsync(soggetto, ambito)
                          ?? throw new UserMessageException(InfrastructureMessages.Get("LicenseNoCatalog"));
            var piano = listino.Lic_Piano?.FirstOrDefault(p => p.Livello == livello && p.Attivo)
                        ?? throw new UserMessageException(InfrastructureMessages.Get("LicensePlanNotAvailable"));

            var precedenti = await AbbonamentiValidi(soggetto, ambito).ToListAsync();
            if (precedenti.Any(x => x.Stato == StatoAbbonamento.InAttesa))
                throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentPending"));
            if (precedenti.Any(x => x.Livello == livello))
                throw new UserMessageException(InfrastructureMessages.Get("LicenseAlreadySubscribed"));

            // azienda gia' coperta dallo studio a livello uguale o superiore: si compra solo un piano superiore (§4.3)
            if (soggetto.Tipo == TipoSoggettoLicenza.Company && soggetto.IdFinancialAdvisor != null
                && await StudioCoverageLevelAsync(soggetto.IdFinancialAdvisor.Value, ambito) is LivelloLicenza delloStudio
                && delloStudio >= livello)
                throw new UserMessageException(InfrastructureMessages.Get("LicenseCoveredByStudio"));

            var oggi = DateTime.Today;
            var prezzo = periodicita == PeriodicitaLicenza.Annuale ? piano.PrezzoAnnuale : piano.PrezzoMensile;
            var abbonamento = new Lic_Abbonamento
            {
                IdCompany = soggetto.Tipo == TipoSoggettoLicenza.Company ? soggetto.Id : null,
                IdFinancialAdvisor = soggetto.Tipo == TipoSoggettoLicenza.FinancialAdvisor ? soggetto.Id : null,
                IdListino = listino.Id,
                IdPiano = piano.Id,
                Ambito = ambito,
                Livello = piano.Livello,
                DescrizionePiano = Etichetta(piano),
                Periodicita = periodicita,
                PrezzoBloccato = prezzo,
                Valuta = listino.Valuta,
                MaxUtentiBloccato = piano.MaxUtenti,
                MaxClientiBloccato = piano.MaxClienti,
                MaxSpazioMBBloccato = piano.MaxSpazioMB,
                DisponibileSuCompany = listino.TipoSoggetto == TipoSoggettoLicenza.FinancialAdvisor && listino.DisponibileSuCompany,
                IdDealerListino = listino.IdDealer,
                DataInizio = oggi,
                DataScadenza = FinePeriodo(oggi, periodicita),
                Stato = prezzo > 0 ? StatoAbbonamento.InAttesa : StatoAbbonamento.Attivo,
                RinnovoAutomatico = rinnovoAutomatico,
                Metodo = prezzo > 0 ? metodo : MetodoPagamento.Nessuno
            };

            var retVal = new SubscribeResult();
            if (prezzo <= 0)
            {
                retVal.Abbonamento = await _abbonamentoRepository.CreateAsync(abbonamento);
                await AnnullaPrecedenti(precedenti);
                return retVal;
            }

            var gateway = _gatewayResolver.Resolve(metodo)
                          ?? throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentNotConfigured"));
            abbonamento = await _abbonamentoRepository.CreateAsync(abbonamento);
            retVal.Abbonamento = abbonamento;

            var esito = await PagaAsync(gateway, abbonamento, soggetto.Descrizione, prezzo, abbonamento.DataInizio, abbonamento.DataScadenza, metodo, returnUrl);
            retVal.Pagamento = esito.Pagamento;
            switch (esito.Result.Stato)
            {
                case StatoPagamento.Completato:
                case StatoPagamento.Debug:
                    abbonamento.Stato = StatoAbbonamento.Attivo;
                    await _abbonamentoRepository.UpdateAsync(abbonamento);
                    await AnnullaPrecedenti(precedenti);
                    break;
                case StatoPagamento.InAttesa:
                case StatoPagamento.Creato:
                    // pagamento ospitato dal provider: l'esito arriva dal callback (fase dei pagamenti reali)
                    retVal.RedirectUrl = esito.Result.RedirectUrl;
                    break;
                default:
                    abbonamento.Stato = StatoAbbonamento.Annullato;
                    await _abbonamentoRepository.UpdateAsync(abbonamento);
                    throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentFailed", esito.Result.Errore ?? string.Empty));
            }
            return retVal;
        }

        public async Task<SubscribeResult?> RenewAsync(Lic_Abbonamento abbonamento, bool automatico, string? returnUrl)
        {
            if (abbonamento.Stato is not (StatoAbbonamento.Attivo or StatoAbbonamento.Scaduto))
                throw new UserMessageException(InfrastructureMessages.Get("LicenseNotRenewable"));

            var settings = await _settingsProvider.GetAsync();
            var oggi = DateTime.Today;
            bool valida = abbonamento.Stato == StatoAbbonamento.Attivo && oggi <= LicenseGuard.FineValidita(abbonamento.DataScadenza, settings.GraceGiorni);

            // una licenza scaduta si rinnova solo se non ce n'e' gia' un'altra valida per la stessa funzione
            if (!valida)
            {
                var altre = await _abbonamentoRepository.FindAll(x => x.Id != abbonamento.Id && x.Ambito == abbonamento.Ambito
                                                                      && x.IdCompany == abbonamento.IdCompany && x.IdFinancialAdvisor == abbonamento.IdFinancialAdvisor
                                                                      && (x.Stato == StatoAbbonamento.Attivo || x.Stato == StatoAbbonamento.InAttesa))
                                                        .AnyAsync();
                if (altre)
                    throw new UserMessageException(InfrastructureMessages.Get("LicenseAlreadySubscribed"));
            }

            // si rifotografa il piano se esiste ancora ed e' attivo (le modifiche del listino valgono dai rinnovi)
            var piano = abbonamento.IdPiano == null ? null
                        : await _pianoRepository.FindAll(x => x.Id == abbonamento.IdPiano && x.Attivo && x.ListinoNavigation!.Attivo)
                                                .Include(x => x.ListinoNavigation).FirstOrDefaultAsync();
            if (piano != null)
            {
                abbonamento.PrezzoBloccato = abbonamento.Periodicita == PeriodicitaLicenza.Annuale ? piano.PrezzoAnnuale : piano.PrezzoMensile;
                abbonamento.MaxUtentiBloccato = piano.MaxUtenti;
                abbonamento.MaxClientiBloccato = piano.MaxClienti;
                abbonamento.MaxSpazioMBBloccato = piano.MaxSpazioMB;
                abbonamento.DescrizionePiano = Etichetta(piano);
                abbonamento.DisponibileSuCompany = piano.ListinoNavigation!.TipoSoggetto == TipoSoggettoLicenza.FinancialAdvisor && piano.ListinoNavigation.DisponibileSuCompany;
            }

            var inizio = valida ? abbonamento.DataScadenza.Date.AddDays(1) : oggi;
            var fine = FinePeriodo(inizio, abbonamento.Periodicita);
            var retVal = new SubscribeResult { Abbonamento = abbonamento };

            if (abbonamento.PrezzoBloccato > 0)
            {
                var metodo = abbonamento.Metodo == MetodoPagamento.Nessuno ? MetodoPagamento.CartaCredito : abbonamento.Metodo;
                var gateway = _gatewayResolver.Resolve(metodo);
                if (gateway == null)
                {
                    if (automatico)
                        return null;
                    throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentNotConfigured"));
                }
                // con un provider reale i rinnovi automatici li addebita il provider (subscription), non il server (§7)
                if (automatico && !gateway.Debug)
                    return null;

                var soggetto = abbonamento.IdCompany != null
                               ? await _resolver.CompanySubjectAsync(abbonamento.IdCompany.Value)
                               : await _resolver.StudioSubjectAsync(abbonamento.IdFinancialAdvisor!.Value);
                var esito = await PagaAsync(gateway, abbonamento, soggetto?.Descrizione ?? string.Empty, abbonamento.PrezzoBloccato, inizio, fine, metodo, returnUrl);
                retVal.Pagamento = esito.Pagamento;
                if (esito.Result.Stato is StatoPagamento.InAttesa or StatoPagamento.Creato)
                {
                    retVal.RedirectUrl = esito.Result.RedirectUrl;
                    return retVal;
                }
                if (esito.Result.Stato is not (StatoPagamento.Completato or StatoPagamento.Debug))
                    throw new UserMessageException(InfrastructureMessages.Get("LicensePaymentFailed", esito.Result.Errore ?? string.Empty));
                abbonamento.Metodo = metodo;
            }

            if (!valida)
                abbonamento.DataInizio = inizio;
            abbonamento.DataScadenza = fine;
            abbonamento.Stato = StatoAbbonamento.Attivo;
            abbonamento.DataUltimoAvviso = null;
            retVal.Abbonamento = await _abbonamentoRepository.UpdateAsync(abbonamento);
            return retVal;
        }

        public async Task<LivelloLicenza?> StudioCoverageLevelAsync(int idFinancialAdvisor, AmbitoLicenza ambito)
        {
            var settings = await _settingsProvider.GetAsync();
            var limite = DateTime.Today.AddDays(-settings.GraceGiorni);
            var livelli = await _abbonamentoRepository
                .FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor && x.DisponibileSuCompany && x.Stato == StatoAbbonamento.Attivo
                              && (x.Ambito == ambito || x.Ambito == AmbitoLicenza.Cumulativo) && x.DataScadenza >= limite)
                .Select(x => x.Livello)
                .ToListAsync();
            return livelli.Count == 0 ? null : livelli.Max();
        }

        public async Task<int> ExpireAsync()
        {
            var settings = await _settingsProvider.GetAsync();
            var limite = DateTime.Today.AddDays(-settings.GraceGiorni);
            return await _abbonamentoRepository.FindAll(x => x.Stato == StatoAbbonamento.Attivo && x.DataScadenza < limite)
                                               .ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, StatoAbbonamento.Scaduto)
                                                                         .SetProperty(x => x.ModifiedDate, DateTime.Now));
        }

        public async Task<int> AutoRenewAsync()
        {
            var oggi = DateTime.Today;
            var daRinnovare = await _abbonamentoRepository.FindAll(x => x.Stato == StatoAbbonamento.Attivo && x.RinnovoAutomatico && x.DataScadenza < oggi)
                                                          .ToListAsync();
            int rinnovati = 0;
            foreach (var abbonamento in daRinnovare)
            {
                try
                {
                    if (await RenewAsync(abbonamento, automatico: true, returnUrl: null) != null)
                        rinnovati++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Rinnovo automatico della licenza {Id} non riuscito", abbonamento.Id);
                }
            }
            return rinnovati;
        }

        public async Task<int> SendWarningsAsync()
        {
            if (!_emailService.Configured)
                return 0;

            var settings = await _settingsProvider.GetAsync();
            var oggi = DateTime.Today;
            var entro = oggi.AddDays(settings.AvvisoGiorni);
            var inScadenza = await _abbonamentoRepository.FindAll(x => x.Stato == StatoAbbonamento.Attivo && !x.RinnovoAutomatico && x.DataUltimoAvviso == null
                                                                       && x.DataScadenza >= oggi && x.DataScadenza <= entro)
                                                         .Include(x => x.CompanyNavigation)
                                                         .Include(x => x.FinancialAdvisorNavigation)
                                                         .ToListAsync();
            int inviati = 0;
            foreach (var abbonamento in inScadenza)
            {
                try
                {
                    var soggetto = abbonamento.CompanyNavigation?.Descrizione ?? abbonamento.FinancialAdvisorNavigation?.Descrizione ?? string.Empty;
                    var oggetto = InfrastructureMessages.Get("LicenseWarningSubject", soggetto);
                    var testo = InfrastructureMessages.Get("LicenseWarningText", soggetto, NomeFunzione(abbonamento.Ambito),
                                                           abbonamento.DescrizionePiano ?? abbonamento.Livello.ToString(), abbonamento.DataScadenza.ToString("dd/MM/yyyy"));
                    foreach (var email in await EmailPowerAdmin(abbonamento))
                        await _emailService.SendAsync(email, oggetto, $"<p>{System.Net.WebUtility.HtmlEncode(testo)}</p>", testo);

                    var ora = DateTime.Now;
                    await _abbonamentoRepository.FindAll(x => x.Id == abbonamento.Id)
                                                .ExecuteUpdateAsync(u => u.SetProperty(x => x.DataUltimoAvviso, ora));
                    inviati++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Avviso di scadenza della licenza {Id} non inviato", abbonamento.Id);
                }
            }
            return inviati;
        }

        public static string NomeFunzione(AmbitoLicenza ambito) => InfrastructureMessages.Get("LicenseAmbito_" + (int)ambito);

        // ---------------------------------------------------------------------------

        private IQueryable<Lic_Abbonamento> AbbonamentiValidi(LicenseSubject soggetto, AmbitoLicenza ambito)
            => soggetto.Tipo == TipoSoggettoLicenza.Company
               ? _abbonamentoRepository.FindAll(x => x.IdCompany == soggetto.Id && x.Ambito == ambito
                                                     && (x.Stato == StatoAbbonamento.Attivo || x.Stato == StatoAbbonamento.InAttesa))
               : _abbonamentoRepository.FindAll(x => x.IdFinancialAdvisor == soggetto.Id && x.Ambito == ambito
                                                     && (x.Stato == StatoAbbonamento.Attivo || x.Stato == StatoAbbonamento.InAttesa));

        private async Task AnnullaPrecedenti(List<Lic_Abbonamento> precedenti)
        {
            foreach (var p in precedenti)
            {
                p.Stato = StatoAbbonamento.Annullato;
                p.RinnovoAutomatico = false;
                await _abbonamentoRepository.UpdateAsync(p);
            }
        }

        private async Task<(Lic_Pagamento Pagamento, PaymentResult Result)> PagaAsync(IPaymentGateway gateway, Lic_Abbonamento abbonamento, string soggetto,
                                                                                     decimal importo, DateTime inizio, DateTime fine, MetodoPagamento metodo, string? returnUrl)
        {
            var centro = await CentroDelSoggettoAsync(abbonamento);
            var pagamento = await _pagamentoRepository.CreateAsync(new Lic_Pagamento
            {
                IdAbbonamento = abbonamento.Id,
                IdCompany = abbonamento.IdCompany,
                IdFinancialAdvisor = abbonamento.IdFinancialAdvisor,
                Soggetto = soggetto.Length > 100 ? soggetto[..100] : soggetto,
                Ambito = abbonamento.Ambito,
                Livello = abbonamento.Livello,
                Importo = importo,
                Valuta = abbonamento.Valuta,
                Periodicita = abbonamento.Periodicita,
                DataInizioCoperta = inizio,
                DataFineCoperta = fine,
                Metodo = metodo,
                Stato = StatoPagamento.Creato,
                Debug = gateway.Debug,
                IdDealerListino = abbonamento.IdDealerListino,
                IdDealer = centro?.IdDealer,
                DealerDescrizione = centro?.Descrizione
            });

            PaymentResult result;
            try
            {
                result = await gateway.StartAsync(new PaymentRequest
                {
                    Importo = importo,
                    Valuta = abbonamento.Valuta,
                    Metodo = metodo,
                    Descrizione = $"{NomeFunzione(abbonamento.Ambito)} - {abbonamento.DescrizionePiano}",
                    IdAbbonamento = abbonamento.Id,
                    ReturnUrl = returnUrl
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagamento della licenza {Id} non riuscito", abbonamento.Id);
                result = new PaymentResult { Stato = StatoPagamento.Fallito, Errore = ex.Message };
            }

            pagamento.Stato = result.Stato;
            pagamento.RiferimentoGateway = result.RiferimentoGateway;
            pagamento.Debug = result.Debug;
            pagamento.Note = result.Errore is { Length: > 500 } e ? e[..500] : result.Errore;
            if (result.Stato is StatoPagamento.Completato or StatoPagamento.Debug)
                pagamento.DataPagamento = DateTime.Now;
            pagamento = await _pagamentoRepository.UpdateAsync(pagamento);
            return (pagamento, result);
        }

        /*
         Centro del cliente che paga (ANALISI_PAGAMENTI_CENTRI.md §2): azienda -> studio -> centro, oppure studio -> centro.
         Si fotografa sul pagamento perche' la quota va al centro a cui il cliente apparteneva quando ha pagato.
         Non e' IdDealerListino: quello e' il proprietario del listino (null = listino globale), non il centro del cliente.
        */
        private async Task<(int IdDealer, string? Descrizione)?> CentroDelSoggettoAsync(Lic_Abbonamento abbonamento)
        {
            int? idFinancialAdvisor = abbonamento.IdFinancialAdvisor;
            if (idFinancialAdvisor == null && abbonamento.IdCompany != null)
                idFinancialAdvisor = await _companyRepository.FindAll(c => c.Id == abbonamento.IdCompany)
                                                             .Select(c => (int?)c.IdFinancialAdvisor)
                                                             .FirstOrDefaultAsync();
            if (idFinancialAdvisor == null)
                return null;

            var centro = await _financialAdvisorRepository.FindAll(f => f.Id == idFinancialAdvisor)
                                                          .Select(f => new { f.IdDealer, f.DealerNavigation!.Descrizione })
                                                          .FirstOrDefaultAsync();
            return centro == null ? null : (centro.IdDealer, centro.Descrizione);
        }

        private async Task<List<string>> EmailPowerAdmin(Lic_Abbonamento abbonamento)
        {
            string ruolo = abbonamento.IdCompany != null ? nameof(RoleCode.CompanyPowerAdmin) : nameof(RoleCode.FinancialAdvisorPowerAdmin);
            var idRuolo = await _aspNetRolesRepository.FindAll(r => r.Name == ruolo).Select(r => r.Id).FirstOrDefaultAsync();
            if (idRuolo == null)
                return new List<string>();

            var utenti = abbonamento.IdCompany != null
                         ? await _userCompanyRepository.FindAll(x => x.IdCompany == abbonamento.IdCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync()
                         : await _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == abbonamento.IdFinancialAdvisor && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            var conRuolo = await _aspNetUserRolesRepository.FindAll(x => x.RoleId == idRuolo && utenti.Contains(x.UserId)).Select(x => x.UserId).ToListAsync();
            return await _aspNetUsersRepository.FindAll(x => conRuolo.Contains(x.Id) && x.Email != null && x.Email != "")
                                               .Select(x => x.Email!)
                                               .ToListAsync();
        }

        private static string Etichetta(Lic_Piano piano)
            => string.IsNullOrWhiteSpace(piano.Descrizione) ? piano.Codice : piano.Descrizione!;
    }
}
