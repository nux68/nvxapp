using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments.Models;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Payments
{
    /// <summary>esito di un invio email a un centro</summary>
    public class PaymentSendResult
    {
        public bool Esito { get; set; }
        public string Destinatari { get; set; } = string.Empty;
        public string? Errore { get; set; }
    }

    /*
     Ripartizione degli incassi delle licenze ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §2, §3, §5).
     Non dipende dall'utente corrente: chi puo' fare cosa lo controlla PaymentReportService; lo usa anche la manutenzione.
        - criterio di cassa: conta Lic_Pagamento.DataPagamento
        - incassati: Completato e anche Debug (pagamenti di prova, contati come incassati: decisione dell'11/11/2026)
        - da liquidare: incassato, con centro, senza rendiconto, incassato entro la fine del mese del rendiconto
        - rettifiche: pagamenti gia' liquidati (IdRendiconto) e poi rimborsati, non ancora stornati
        - la bozza e' un'anteprima: i pagamenti si agganciano al rendiconto solo alla conferma, ricalcolando
        - un solo rendiconto non annullato per centro e mese; si genera solo per un mese concluso
    */
    public interface IPaymentSettlementManager
    {
        Task<decimal> PercentualeAsync(int idDealer);
        /// <summary>trattenuta propria del centro (null = vale quella generale) e trattenuta generale</summary>
        Task<(decimal? Propria, decimal Generale)> PercentualiAsync(int idDealer);
        /// <summary>salva la trattenuta propria del centro (null = torna a quella generale)</summary>
        Task SalvaPercentualeAsync(int idDealer, decimal? percentuale);
        IQueryable<Lic_Pagamento> PagamentiPeriodo(int anno, int? mese, int? idDealer);
        /// <summary>riepilogo per centro del periodo; idDealer null = tutti i centri (anche senza movimenti) e i clienti senza centro</summary>
        Task<List<PaymentDealerSummaryModel>> RiepilogoAsync(int anno, int? mese, int? idDealer);
        /// <summary>crea o aggiorna la bozza; null = niente da liquidare (l'eventuale bozza viene tolta)</summary>
        Task<Pag_Rendiconto?> GeneraAsync(int idDealer, int anno, int mese);
        /// <summary>bozze del mese per tutti i centri con qualcosa da liquidare; restituisce quante</summary>
        Task<int> GeneraTuttiAsync(int anno, int mese);
        Task<Pag_Rendiconto> ConfermaAsync(int idRendiconto);
        /// <summary>bozza: eliminata; confermato: annullato e pagamenti di nuovo da liquidare; liquidato: non consentito</summary>
        Task AnnullaAsync(int idRendiconto);
        Task<Pag_Rendiconto> LiquidaAsync(int idRendiconto, DateTime data, string? riferimento, string? note);
        Task<Pag_Rendiconto> FatturaAsync(int idRendiconto, string? numero, DateTime? data);
        Task<PaymentDocument> DocumentoRendicontoAsync(Pag_Rendiconto rendiconto);
        Task<PaymentDocument> DocumentoPeriodoAsync(int idDealer, int anno, int? mese);
        Task<List<string>> DestinatariAsync(int idDealer);
        /// <summary>email del rendiconto confermato (Mensile) o liquidato (Liquidazione), con il PDF</summary>
        Task<PaymentSendResult> InviaRendicontoAsync(Pag_Rendiconto rendiconto, TipoRiepilogoPagamenti tipo, bool automatico);
        /// <summary>riepilogo del mese o dell'anno (mese null), con il PDF</summary>
        Task<PaymentSendResult> InviaRiepilogoAsync(int idDealer, int anno, int? mese, bool automatico);
        /// <summary>riepiloghi annuali automatici ai centri con movimenti nell'anno, una volta sola; restituisce quanti</summary>
        Task<int> InviaRiepiloghiAnnualiAsync(int anno);
    }

    public class PaymentSettlementManager : IPaymentSettlementManager
    {
        private readonly ILic_PagamentoRepository _pagamentoRepository;
        private readonly IPag_RendicontoRepository _rendicontoRepository;
        private readonly IPag_DealerConfigRepository _dealerConfigRepository;
        private readonly IPag_InvioEmailRepository _invioEmailRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly IUserDealerRepository _userDealerRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IPaymentSettingsProvider _settingsProvider;
        private readonly IEmailService _emailService;

        public PaymentSettlementManager(ILic_PagamentoRepository pagamentoRepository,
                                        IPag_RendicontoRepository rendicontoRepository,
                                        IPag_DealerConfigRepository dealerConfigRepository,
                                        IPag_InvioEmailRepository invioEmailRepository,
                                        IDealerRepository dealerRepository,
                                        IUserDealerRepository userDealerRepository,
                                        IAspNetUsersRepository aspNetUsersRepository,
                                        IAspNetRolesRepository aspNetRolesRepository,
                                        IAspNetUserRolesRepository aspNetUserRolesRepository,
                                        IPaymentSettingsProvider settingsProvider,
                                        IEmailService emailService)
        {
            _pagamentoRepository = pagamentoRepository;
            _rendicontoRepository = rendicontoRepository;
            _dealerConfigRepository = dealerConfigRepository;
            _invioEmailRepository = invioEmailRepository;
            _dealerRepository = dealerRepository;
            _userDealerRepository = userDealerRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _settingsProvider = settingsProvider;
            _emailService = emailService;
        }

        // ---------------------------------------------------------------------------
        // Calcoli

        public static (DateTime Dal, DateTime Al) Periodo(int anno, int? mese)
            => mese == null ? (new DateTime(anno, 1, 1), new DateTime(anno + 1, 1, 1))
                            : (new DateTime(anno, mese.Value, 1), new DateTime(anno, mese.Value, 1).AddMonths(1));

        /// <summary>stati dei pagamenti incassati: andati a buon fine, anche in modalita' debug</summary>
        public static bool Incassato(StatoPagamento stato) => stato is StatoPagamento.Completato or StatoPagamento.Debug;

        public static decimal Trattenuta(decimal imponibile, decimal percentuale)
            => Math.Round(imponibile * percentuale / 100m, 2, MidpointRounding.AwayFromZero);

        public async Task<decimal> PercentualeAsync(int idDealer)
        {
            var propria = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).Select(x => x.PercentualePiattaforma).FirstOrDefaultAsync();
            return propria ?? (await _settingsProvider.GetAsync()).PercentualePiattaforma;
        }

        public async Task<(decimal? Propria, decimal Generale)> PercentualiAsync(int idDealer)
        {
            var propria = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).Select(x => x.PercentualePiattaforma).FirstOrDefaultAsync();
            return (propria, (await _settingsProvider.GetAsync()).PercentualePiattaforma);
        }

        public async Task SalvaPercentualeAsync(int idDealer, decimal? percentuale)
        {
            if (percentuale is < 0 or > 100)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentDealerConfigInvalid"));
            var valore = percentuale == null ? (decimal?)null : Math.Round(percentuale.Value, 2);
            var config = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).FirstOrDefaultAsync();
            if (config == null)
            {
                if (valore != null)
                    await _dealerConfigRepository.CreateAsync(new Pag_DealerConfig { IdDealer = idDealer, PercentualePiattaforma = valore });
            }
            else if (config.PercentualePiattaforma != valore)
            {
                config.PercentualePiattaforma = valore;
                await _dealerConfigRepository.UpdateAsync(config);
            }
        }

        public IQueryable<Lic_Pagamento> PagamentiPeriodo(int anno, int? mese, int? idDealer)
        {
            var (dal, al) = Periodo(anno, mese);
            var q = _pagamentoRepository.FindAll(x => x.DataPagamento != null && x.DataPagamento >= dal && x.DataPagamento < al);
            return idDealer == null ? q : q.Where(x => x.IdDealer == idDealer);
        }

        public async Task<List<PaymentDealerSummaryModel>> RiepilogoAsync(int anno, int? mese, int? idDealer)
        {
            var gruppi = await PagamentiPeriodo(anno, mese, idDealer)
                               .GroupBy(x => new { x.IdDealer, x.Stato, x.Debug })
                               .Select(g => new { g.Key.IdDealer, g.Key.Stato, g.Key.Debug, Numero = g.Count(), Totale = g.Sum(x => x.Importo), Nome = g.Max(x => x.DealerDescrizione) })
                               .ToListAsync();

            var rendiconti = await _rendicontoRepository.FindAll(x => x.Anno == anno && (mese == null || x.Mese == mese) && x.Stato != StatoRendiconto.Annullato
                                                                      && (idDealer == null || x.IdDealer == idDealer))
                                                        .ToListAsync();

            var centri = await _dealerRepository.FindAll(x => idDealer == null || x.Id == idDealer)
                                                .Select(x => new { x.Id, x.Descrizione })
                                                .ToListAsync();
            var config = await _dealerConfigRepository.FindAll(x => x.PercentualePiattaforma != null && (idDealer == null || x.IdDealer == idDealer))
                                                      .ToDictionaryAsync(x => x.IdDealer, x => x.PercentualePiattaforma!.Value);
            var generale = (await _settingsProvider.GetAsync()).PercentualePiattaforma;

            // centri esistenti, piu' quelli cancellati con movimenti (descrizione fotografata) e i clienti senza centro
            var chiavi = centri.Select(c => (int?)c.Id)
                               .Union(gruppi.Select(g => g.IdDealer))
                               .Union(rendiconti.Select(r => (int?)r.IdDealer))
                               .Distinct()
                               .ToList();

            var retVal = new List<PaymentDealerSummaryModel>();
            foreach (var chiave in chiavi)
            {
                var propri = gruppi.Where(g => g.IdDealer == chiave).ToList();
                var suoi = rendiconti.Where(r => r.IdDealer == chiave).ToList();
                var incassati = propri.Where(g => Incassato(g.Stato)).ToList();
                decimal perc = chiave == null ? 100m : config.TryGetValue(chiave.Value, out var p) ? p : generale;

                var riga = new PaymentDealerSummaryModel
                {
                    IdDealer = chiave,
                    Dealer = chiave == null ? null
                             : centri.FirstOrDefault(c => c.Id == chiave)?.Descrizione
                               ?? propri.Select(g => g.Nome).FirstOrDefault(n => n != null)
                               ?? suoi.Select(r => r.DealerDescrizione).FirstOrDefault(),
                    NumeroPagamenti = incassati.Sum(g => g.Numero),
                    Incassato = incassati.Sum(g => g.Totale),
                    Rimborsato = propri.Where(g => g.Stato == StatoPagamento.Rimborsato).Sum(g => g.Totale),
                    NumeroDebug = propri.Where(g => g.Debug).Sum(g => g.Numero),
                    ImportoDebug = propri.Where(g => g.Debug).Sum(g => g.Totale),
                    PercentualePiattaforma = perc,
                    Liquidato = suoi.Where(r => r.Stato == StatoRendiconto.Liquidato).Sum(r => r.QuotaCentro),
                    DaLiquidare = suoi.Where(r => r.Stato == StatoRendiconto.Confermato).Sum(r => r.QuotaCentro)
                };
                riga.QuotaPiattaforma = Trattenuta(riga.Incassato, perc);
                riga.QuotaCentro = riga.Incassato - riga.QuotaPiattaforma;
                if (mese != null)
                {
                    var r = suoi.FirstOrDefault();
                    riga.Rendiconto = r == null ? null : SettlementModel(r);
                }
                retVal.Add(riga);
            }

            return retVal.OrderBy(x => x.IdDealer == null).ThenBy(x => x.Dealer).ToList();
        }

        private sealed class Calcolo
        {
            public List<int> Pagamenti { get; set; } = new();
            public List<int> Rettifiche { get; set; } = new();
            public decimal Incassato { get; set; }
            public decimal TotaleRettifiche { get; set; }
            public decimal Percentuale { get; set; }
            public string Valuta { get; set; } = "EUR";
            public bool Vuoto => Pagamenti.Count == 0 && Rettifiche.Count == 0;
        }

        private IQueryable<Lic_Pagamento> DaLiquidare(int idDealer, DateTime al)
            => _pagamentoRepository.FindAll(x => x.IdDealer == idDealer && (x.Stato == StatoPagamento.Completato || x.Stato == StatoPagamento.Debug)
                                                 && x.IdRendiconto == null && x.DataPagamento != null && x.DataPagamento < al);

        private IQueryable<Lic_Pagamento> DaStornare(int idDealer)
            => _pagamentoRepository.FindAll(x => x.IdDealer == idDealer && x.Stato == StatoPagamento.Rimborsato
                                                 && x.IdRendiconto != null && x.IdRendicontoRettifica == null);

        private async Task<Calcolo> CalcolaAsync(int idDealer, DateTime al)
        {
            var pagamenti = await DaLiquidare(idDealer, al).Select(x => new { x.Id, x.Importo, x.Valuta }).ToListAsync();
            var rettifiche = await DaStornare(idDealer).Select(x => new { x.Id, x.Importo }).ToListAsync();
            return new Calcolo
            {
                Pagamenti = pagamenti.Select(x => x.Id).ToList(),
                Rettifiche = rettifiche.Select(x => x.Id).ToList(),
                Incassato = pagamenti.Sum(x => x.Importo),
                TotaleRettifiche = rettifiche.Sum(x => x.Importo),
                Percentuale = await PercentualeAsync(idDealer),
                Valuta = pagamenti.Select(x => x.Valuta).FirstOrDefault() ?? "EUR"
            };
        }

        private static void Applica(Pag_Rendiconto r, Calcolo c)
        {
            r.NumeroPagamenti = c.Pagamenti.Count;
            r.TotaleIncassato = c.Incassato;
            r.TotaleRettifiche = c.TotaleRettifiche;
            r.PercentualePiattaforma = c.Percentuale;
            r.QuotaPiattaforma = Trattenuta(c.Incassato - c.TotaleRettifiche, c.Percentuale);
            r.QuotaCentro = c.Incassato - c.TotaleRettifiche - r.QuotaPiattaforma;
            r.Valuta = c.Valuta;
        }

        // ---------------------------------------------------------------------------
        // Rendiconti

        public async Task<Pag_Rendiconto?> GeneraAsync(int idDealer, int anno, int mese)
        {
            if (anno < 2000 || mese < 1 || mese > 12)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentPeriodInvalid"));
            var (_, al) = Periodo(anno, mese);
            if (al > DateTime.Today)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentPeriodNotClosed"));

            var esistente = await _rendicontoRepository.FindAll(x => x.IdDealer == idDealer && x.Anno == anno && x.Mese == mese && x.Stato != StatoRendiconto.Annullato)
                                                       .FirstOrDefaultAsync();
            if (esistente != null && esistente.Stato != StatoRendiconto.Bozza)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementAlreadyConfirmed"));

            var calcolo = await CalcolaAsync(idDealer, al);
            if (calcolo.Vuoto)
            {
                if (esistente != null)
                    await _rendicontoRepository.DeleteAsync(esistente);
                return null;
            }

            var dealer = await _dealerRepository.FindAll(x => x.Id == idDealer).Select(x => x.Descrizione).FirstOrDefaultAsync();
            var r = esistente ?? new Pag_Rendiconto { IdDealer = idDealer, Anno = anno, Mese = mese, Stato = StatoRendiconto.Bozza };
            r.DealerDescrizione = dealer ?? r.DealerDescrizione;
            Applica(r, calcolo);
            return esistente == null ? await _rendicontoRepository.CreateAsync(r) : await _rendicontoRepository.UpdateAsync(r);
        }

        public async Task<int> GeneraTuttiAsync(int anno, int mese)
        {
            var (_, al) = Periodo(anno, mese);
            var conMovimenti = await _pagamentoRepository.FindAll(x => x.IdDealer != null
                                                                       && (((x.Stato == StatoPagamento.Completato || x.Stato == StatoPagamento.Debug) && x.IdRendiconto == null && x.DataPagamento != null && x.DataPagamento < al)
                                                                           || (x.Stato == StatoPagamento.Rimborsato && x.IdRendiconto != null && x.IdRendicontoRettifica == null)))
                                                         .Select(x => x.IdDealer!.Value)
                                                         .Distinct()
                                                         .ToListAsync();
            var delMese = await _rendicontoRepository.FindAll(x => x.Anno == anno && x.Mese == mese && x.Stato != StatoRendiconto.Annullato)
                                                     .Select(x => new { x.IdDealer, x.Stato })
                                                     .ToListAsync();
            // le bozze gia' presenti si aggiornano (o si tolgono se non c'e' piu' niente); i confermati non si toccano
            var centri = conMovimenti.Union(delMese.Where(x => x.Stato == StatoRendiconto.Bozza).Select(x => x.IdDealer))
                                     .Except(delMese.Where(x => x.Stato != StatoRendiconto.Bozza).Select(x => x.IdDealer))
                                     .Distinct()
                                     .ToList();
            int generati = 0;
            foreach (var idDealer in centri)
            {
                if (await GeneraAsync(idDealer, anno, mese) != null)
                    generati++;
            }
            return generati;
        }

        public async Task<Pag_Rendiconto> ConfermaAsync(int idRendiconto)
        {
            var r = await TrovaAsync(idRendiconto);
            if (r.Stato != StatoRendiconto.Bozza)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementStateInvalid"));

            await using var tx = await _rendicontoRepository.BeginTransactionAsync();
            var (_, al) = Periodo(r.Anno, r.Mese);
            var calcolo = await CalcolaAsync(r.IdDealer, al);
            if (calcolo.Vuoto)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentNothingToSettle"));

            Applica(r, calcolo);
            r.Stato = StatoRendiconto.Confermato;
            r.DataConferma = DateTime.Now;
            r = await _rendicontoRepository.UpdateAsync(r);

            int id = r.Id;
            var idsPagamenti = calcolo.Pagamenti;
            var idsRettifiche = calcolo.Rettifiche;
            int agganciati = await _pagamentoRepository.FindAll(x => idsPagamenti.Contains(x.Id) && x.IdRendiconto == null)
                                                       .ExecuteUpdateAsync(u => u.SetProperty(x => x.IdRendiconto, id));
            int stornati = await _pagamentoRepository.FindAll(x => idsRettifiche.Contains(x.Id) && x.IdRendicontoRettifica == null)
                                                     .ExecuteUpdateAsync(u => u.SetProperty(x => x.IdRendicontoRettifica, id));
            // un altro rendiconto ha preso nel frattempo gli stessi pagamenti: si lascia tutto com'era
            if (agganciati != idsPagamenti.Count || stornati != idsRettifiche.Count)
            {
                await tx.RollbackAsync();
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementChanged"));
            }

            await tx.CommitAsync();
            return r;
        }

        public async Task AnnullaAsync(int idRendiconto)
        {
            var r = await TrovaAsync(idRendiconto);
            switch (r.Stato)
            {
                case StatoRendiconto.Bozza:
                    await _rendicontoRepository.DeleteAsync(r);
                    return;
                case StatoRendiconto.Confermato:
                    await using (var tx = await _rendicontoRepository.BeginTransactionAsync())
                    {
                        int id = r.Id;
                        await _pagamentoRepository.FindAll(x => x.IdRendiconto == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.IdRendiconto, (int?)null));
                        await _pagamentoRepository.FindAll(x => x.IdRendicontoRettifica == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.IdRendicontoRettifica, (int?)null));
                        r.Stato = StatoRendiconto.Annullato;
                        await _rendicontoRepository.UpdateAsync(r);
                        await tx.CommitAsync();
                    }
                    return;
                default:
                    throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementStateInvalid"));
            }
        }

        public async Task<Pag_Rendiconto> LiquidaAsync(int idRendiconto, DateTime data, string? riferimento, string? note)
        {
            var r = await TrovaAsync(idRendiconto);
            if (r.Stato != StatoRendiconto.Confermato)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementStateInvalid"));
            if (data == default || data.Date > DateTime.Today)
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementPayInvalid"));

            r.Stato = StatoRendiconto.Liquidato;
            r.DataLiquidazione = data.Date;
            r.RiferimentoBonifico = Taglia(riferimento, 100);
            if (!string.IsNullOrWhiteSpace(note))
                r.Note = Taglia(note, 500);
            return await _rendicontoRepository.UpdateAsync(r);
        }

        public async Task<Pag_Rendiconto> FatturaAsync(int idRendiconto, string? numero, DateTime? data)
        {
            var r = await TrovaAsync(idRendiconto);
            if (r.Stato is not (StatoRendiconto.Confermato or StatoRendiconto.Liquidato))
                throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementStateInvalid"));
            r.NumeroFatturaCentro = Taglia(numero, 50);
            r.DataFatturaCentro = string.IsNullOrWhiteSpace(r.NumeroFatturaCentro) ? null : data?.Date;
            return await _rendicontoRepository.UpdateAsync(r);
        }

        private async Task<Pag_Rendiconto> TrovaAsync(int idRendiconto)
            => await _rendicontoRepository.FindAll(x => x.Id == idRendiconto).FirstOrDefaultAsync()
               ?? throw new UserMessageException(InfrastructureMessages.Get("PaymentSettlementNotFound"));

        private static string? Taglia(string? testo, int max)
        {
            var t = testo?.Trim();
            return string.IsNullOrEmpty(t) ? null : t.Length > max ? t[..max] : t;
        }

        public static PaymentSettlementModel SettlementModel(Pag_Rendiconto r) => new()
        {
            Id = r.Id,
            IdDealer = r.IdDealer,
            Dealer = r.DealerDescrizione,
            Anno = r.Anno,
            Mese = r.Mese,
            NumeroPagamenti = r.NumeroPagamenti,
            TotaleIncassato = r.TotaleIncassato,
            TotaleRettifiche = r.TotaleRettifiche,
            PercentualePiattaforma = r.PercentualePiattaforma,
            QuotaPiattaforma = r.QuotaPiattaforma,
            QuotaCentro = r.QuotaCentro,
            Valuta = r.Valuta,
            Stato = (int)r.Stato,
            DataConferma = r.DataConferma,
            DataLiquidazione = r.DataLiquidazione,
            RiferimentoBonifico = r.RiferimentoBonifico,
            NumeroFatturaCentro = r.NumeroFatturaCentro,
            DataFatturaCentro = r.DataFatturaCentro,
            Note = r.Note
        };

        // ---------------------------------------------------------------------------
        // Documenti (PDF ed email)

        public static CultureInfo Cultura()
            => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? CultureInfo.GetCultureInfo("en-GB") : CultureInfo.GetCultureInfo("it-IT");

        public static string Importo(decimal valore, string valuta = "EUR") => valore.ToString("N2", Cultura()) + " " + valuta;
        public static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
        public static string NomeMese(int mese) => Cultura().TextInfo.ToTitleCase(Cultura().DateTimeFormat.GetMonthName(mese));
        public static string NomePeriodo(int anno, int? mese) => mese == null ? anno.ToString(CultureInfo.InvariantCulture) : NomeMese(mese.Value) + " " + anno;
        public static string NomeStato(StatoRendiconto stato) => InfrastructureMessages.Get("PaymentSettlementState_" + (int)stato);
        public static string NomeFunzione(AmbitoLicenza ambito) => InfrastructureMessages.Get("LicenseAmbito_" + (int)ambito);

        private static string Percentuale(decimal p) => p.ToString("0.##", Cultura()) + "%";

        private async Task<string> NomeCentroAsync(int idDealer)
            => await _dealerRepository.FindAll(x => x.Id == idDealer).Select(x => x.Descrizione).FirstOrDefaultAsync()
               ?? await _pagamentoRepository.FindAll(x => x.IdDealer == idDealer && x.DealerDescrizione != null).Select(x => x.DealerDescrizione).FirstOrDefaultAsync()
               ?? idDealer.ToString(CultureInfo.InvariantCulture);

        private static string[] IntestazionePagamenti() => new[]
        {
            InfrastructureMessages.Get("PaymentPdfColDate"), InfrastructureMessages.Get("PaymentPdfColSubject"), InfrastructureMessages.Get("PaymentPdfColFunction"),
            InfrastructureMessages.Get("PaymentPdfColPeriod"), InfrastructureMessages.Get("PaymentPdfColAmount")
        };

        private static string[] RigaPagamento(Lic_Pagamento p, bool storno = false) => new[]
        {
            Data(p.DataPagamento),
            (p.Soggetto ?? string.Empty) + (storno ? " (" + InfrastructureMessages.Get("PaymentPdfRefund") + ")" : p.Debug ? " (debug)" : string.Empty),
            NomeFunzione(p.Ambito) + " " + p.Livello,
            Data(p.DataInizioCoperta) + " - " + Data(p.DataFineCoperta),
            Importo(storno ? -p.Importo : p.Importo, p.Valuta)
        };

        private static List<string[]> RiepilogoRendiconto(Pag_Rendiconto r)
        {
            var righe = new List<string[]>
            {
                new[] { InfrastructureMessages.Get("PaymentPdfState"), NomeStato(r.Stato) },
                new[] { InfrastructureMessages.Get("PaymentPdfPayments"), r.NumeroPagamenti.ToString(CultureInfo.InvariantCulture) },
                new[] { InfrastructureMessages.Get("PaymentPdfCollected"), Importo(r.TotaleIncassato, r.Valuta) },
            };
            if (r.TotaleRettifiche != 0)
                righe.Add(new[] { InfrastructureMessages.Get("PaymentPdfAdjustments"), Importo(-r.TotaleRettifiche, r.Valuta) });
            righe.Add(new[] { InfrastructureMessages.Get("PaymentPdfFee") + " " + Percentuale(r.PercentualePiattaforma), Importo(r.QuotaPiattaforma, r.Valuta) });
            righe.Add(new[] { InfrastructureMessages.Get("PaymentPdfDealerShare"), Importo(r.QuotaCentro, r.Valuta) });
            if (r.DataLiquidazione != null)
                righe.Add(new[] { InfrastructureMessages.Get("PaymentPdfPaidOn"), Data(r.DataLiquidazione) + (string.IsNullOrEmpty(r.RiferimentoBonifico) ? string.Empty : " - " + r.RiferimentoBonifico) });
            if (!string.IsNullOrEmpty(r.NumeroFatturaCentro))
                righe.Add(new[] { InfrastructureMessages.Get("PaymentPdfInvoice"), r.NumeroFatturaCentro + (r.DataFatturaCentro == null ? string.Empty : " - " + Data(r.DataFatturaCentro)) });
            return righe;
        }

        public async Task<PaymentDocument> DocumentoRendicontoAsync(Pag_Rendiconto r)
        {
            int id = r.Id;
            var pagamenti = await _pagamentoRepository.FindAll(x => x.IdRendiconto == id).OrderBy(x => x.DataPagamento).ToListAsync();
            var storni = await _pagamentoRepository.FindAll(x => x.IdRendicontoRettifica == id).OrderBy(x => x.DataPagamento).ToListAsync();
            // in bozza i pagamenti non sono ancora agganciati: si mostrano quelli che la conferma prenderebbe
            if (r.Stato == StatoRendiconto.Bozza)
            {
                var (_, al) = Periodo(r.Anno, r.Mese);
                pagamenti = await DaLiquidare(r.IdDealer, al).OrderBy(x => x.DataPagamento).ToListAsync();
                storni = await DaStornare(r.IdDealer).OrderBy(x => x.DataPagamento).ToListAsync();
            }

            var dettaglio = new List<string[]> { IntestazionePagamenti() };
            dettaglio.AddRange(pagamenti.Select(p => RigaPagamento(p)));
            dettaglio.AddRange(storni.Select(p => RigaPagamento(p, true)));

            return new PaymentDocument
            {
                Titolo = InfrastructureMessages.Get("PaymentPdfTitleSettlement", NomePeriodo(r.Anno, r.Mese)),
                Sottotitolo = InfrastructureMessages.Get("PaymentPdfDealer") + ": " + (r.DealerDescrizione ?? await NomeCentroAsync(r.IdDealer)),
                Riepilogo = RiepilogoRendiconto(r),
                Tabelle = { new PaymentDocumentTable { Titolo = InfrastructureMessages.Get("PaymentPdfDetail"), Righe = dettaglio, ColonneADestra = { 4 } } }
            };
        }

        public async Task<PaymentDocument> DocumentoPeriodoAsync(int idDealer, int anno, int? mese)
        {
            var riepilogo = (await RiepilogoAsync(anno, mese, idDealer)).FirstOrDefault(x => x.IdDealer == idDealer) ?? new PaymentDealerSummaryModel { IdDealer = idDealer };
            var pagamenti = await PagamentiPeriodo(anno, mese, idDealer).Where(x => x.Stato == StatoPagamento.Completato || x.Stato == StatoPagamento.Debug
                                                                                    || x.Stato == StatoPagamento.Rimborsato)
                                                                       .OrderBy(x => x.DataPagamento)
                                                                       .ToListAsync();
            var doc = new PaymentDocument
            {
                Titolo = InfrastructureMessages.Get(mese == null ? "PaymentPdfTitleAnnual" : "PaymentPdfTitleMonthly", NomePeriodo(anno, mese)),
                Sottotitolo = InfrastructureMessages.Get("PaymentPdfDealer") + ": " + (riepilogo.Dealer ?? await NomeCentroAsync(idDealer)),
                Riepilogo =
                {
                    new[] { InfrastructureMessages.Get("PaymentPdfPayments"), riepilogo.NumeroPagamenti.ToString(CultureInfo.InvariantCulture) },
                    new[] { InfrastructureMessages.Get("PaymentPdfCollected"), Importo(riepilogo.Incassato) },
                    new[] { InfrastructureMessages.Get("PaymentPdfFee") + " " + Percentuale(riepilogo.PercentualePiattaforma), Importo(riepilogo.QuotaPiattaforma) },
                    new[] { InfrastructureMessages.Get("PaymentPdfDealerShareEstimated"), Importo(riepilogo.QuotaCentro) },
                    new[] { InfrastructureMessages.Get("PaymentPdfPaid"), Importo(riepilogo.Liquidato) },
                    new[] { InfrastructureMessages.Get("PaymentPdfToPay"), Importo(riepilogo.DaLiquidare) }
                },
                Nota = InfrastructureMessages.Get("PaymentPdfEstimatedNote")
            };
            if (riepilogo.Rimborsato != 0)
                doc.Riepilogo.Insert(2, new[] { InfrastructureMessages.Get("PaymentPdfRefunded"), Importo(riepilogo.Rimborsato) });

            if (mese == null)
            {
                // mese per mese: incassato e rendiconto
                var rendiconti = await _rendicontoRepository.FindAll(x => x.IdDealer == idDealer && x.Anno == anno && x.Stato != StatoRendiconto.Annullato && x.Stato != StatoRendiconto.Bozza)
                                                            .ToListAsync();
                var mesi = new List<string[]>
                {
                    new[] { InfrastructureMessages.Get("PaymentPdfColMonth"), InfrastructureMessages.Get("PaymentPdfPayments"), InfrastructureMessages.Get("PaymentPdfCollected"),
                            InfrastructureMessages.Get("PaymentPdfSettlement"), InfrastructureMessages.Get("PaymentPdfDealerShare"), InfrastructureMessages.Get("PaymentPdfPaidOn") }
                };
                for (int m = 1; m <= 12; m++)
                {
                    var delMese = pagamenti.Where(p => p.DataPagamento!.Value.Month == m && Incassato(p.Stato)).ToList();
                    var r = rendiconti.FirstOrDefault(x => x.Mese == m);
                    if (delMese.Count == 0 && r == null)
                        continue;
                    mesi.Add(new[]
                    {
                        NomeMese(m), delMese.Count.ToString(CultureInfo.InvariantCulture), Importo(delMese.Sum(p => p.Importo)),
                        r == null ? "-" : NomeStato(r.Stato), r == null ? "-" : Importo(r.QuotaCentro, r.Valuta), Data(r?.DataLiquidazione)
                    });
                }
                doc.Tabelle.Add(new PaymentDocumentTable { Titolo = InfrastructureMessages.Get("PaymentPdfMonths"), Righe = mesi, ColonneADestra = { 1, 2, 4 } });
            }
            else
            {
                var r = await _rendicontoRepository.FindAll(x => x.IdDealer == idDealer && x.Anno == anno && x.Mese == mese
                                                                 && x.Stato != StatoRendiconto.Annullato && x.Stato != StatoRendiconto.Bozza)
                                                   .FirstOrDefaultAsync();
                if (r != null)
                {
                    var righe = new List<string[]> { new[] { string.Empty, string.Empty } };
                    righe.AddRange(RiepilogoRendiconto(r));
                    doc.Tabelle.Add(new PaymentDocumentTable { Titolo = InfrastructureMessages.Get("PaymentPdfSettlement"), Righe = righe, SenzaIntestazione = true });
                }
            }

            var dettaglio = new List<string[]> { IntestazionePagamenti() };
            dettaglio.AddRange(pagamenti.Select(p => RigaPagamento(p, p.Stato == StatoPagamento.Rimborsato)));
            doc.Tabelle.Add(new PaymentDocumentTable { Titolo = InfrastructureMessages.Get("PaymentPdfDetail"), Righe = dettaglio, ColonneADestra = { 4 } });
            return doc;
        }

        // ---------------------------------------------------------------------------
        // Email

        public async Task<List<string>> DestinatariAsync(int idDealer)
        {
            var configurata = await _dealerConfigRepository.FindAll(x => x.IdDealer == idDealer).Select(x => x.EmailAmministrazione).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(configurata))
                return configurata.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();

            var idRuolo = await _aspNetRolesRepository.FindAll(r => r.Name == nameof(RoleCode.DealerPowerAdmin)).Select(r => r.Id).FirstOrDefaultAsync();
            if (idRuolo == null)
                return new List<string>();
            var utenti = await _userDealerRepository.FindAll(x => x.IdDealer == idDealer && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            var conRuolo = await _aspNetUserRolesRepository.FindAll(x => x.RoleId == idRuolo && utenti.Contains(x.UserId)).Select(x => x.UserId).ToListAsync();
            return await _aspNetUsersRepository.FindAll(x => conRuolo.Contains(x.Id) && x.Email != null && x.Email != "")
                                               .Select(x => x.Email!)
                                               .Distinct()
                                               .ToListAsync();
        }

        public async Task<PaymentSendResult> InviaRendicontoAsync(Pag_Rendiconto r, TipoRiepilogoPagamenti tipo, bool automatico)
        {
            var periodo = NomePeriodo(r.Anno, r.Mese);
            var centro = r.DealerDescrizione ?? await NomeCentroAsync(r.IdDealer);
            string oggetto, testo;
            if (tipo == TipoRiepilogoPagamenti.Liquidazione)
            {
                oggetto = InfrastructureMessages.Get("PaymentPaidSubject", centro, periodo);
                testo = InfrastructureMessages.Get("PaymentPaidText", centro, periodo, Importo(r.QuotaCentro, r.Valuta), Data(r.DataLiquidazione), r.RiferimentoBonifico ?? "-");
            }
            else
            {
                oggetto = InfrastructureMessages.Get("PaymentSettlementSubject", centro, periodo);
                testo = InfrastructureMessages.Get("PaymentSettlementText", centro, periodo, Importo(r.TotaleIncassato - r.TotaleRettifiche, r.Valuta),
                                                   Percentuale(r.PercentualePiattaforma), Importo(r.QuotaPiattaforma, r.Valuta), Importo(r.QuotaCentro, r.Valuta));
            }
            var pdf = PaymentReportPdf.Crea(await DocumentoRendicontoAsync(r));
            return await InviaAsync(r.IdDealer, r.Anno, r.Mese, tipo, r.Id, automatico, oggetto, testo, NomeFile(centro, r.Anno, r.Mese), pdf);
        }

        public async Task<PaymentSendResult> InviaRiepilogoAsync(int idDealer, int anno, int? mese, bool automatico)
        {
            var doc = await DocumentoPeriodoAsync(idDealer, anno, mese);
            var centro = await NomeCentroAsync(idDealer);
            var periodo = NomePeriodo(anno, mese);
            var oggetto = InfrastructureMessages.Get("PaymentSummarySubject", centro, periodo);
            var testo = InfrastructureMessages.Get("PaymentSummaryText", centro, periodo);
            return await InviaAsync(idDealer, anno, mese, mese == null ? TipoRiepilogoPagamenti.Annuale : TipoRiepilogoPagamenti.Mensile, null, automatico,
                                    oggetto, testo, NomeFile(centro, anno, mese), PaymentReportPdf.Crea(doc));
        }

        public async Task<int> InviaRiepiloghiAnnualiAsync(int anno)
        {
            if (!_emailService.Configured)
                return 0;
            var (dal, al) = Periodo(anno, null);
            var conMovimenti = await _pagamentoRepository.FindAll(x => x.IdDealer != null && (x.Stato == StatoPagamento.Completato || x.Stato == StatoPagamento.Debug)
                                                                       && x.DataPagamento >= dal && x.DataPagamento < al)
                                                         .Select(x => x.IdDealer!.Value)
                                                         .Distinct()
                                                         .ToListAsync();
            var esistenti = await _dealerRepository.FindAll(x => conMovimenti.Contains(x.Id)).Select(x => x.Id).ToListAsync();
            var giaInviati = await _invioEmailRepository.FindAll(x => x.Anno == anno && x.Tipo == TipoRiepilogoPagamenti.Annuale && x.Automatico && x.Esito)
                                                        .Select(x => x.IdDealer)
                                                        .ToListAsync();
            int inviati = 0;
            foreach (var idDealer in esistenti.Except(giaInviati))
            {
                try
                {
                    if ((await InviaRiepilogoAsync(idDealer, anno, null, true)).Esito)
                        inviati++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Riepilogo annuale {Anno} dei pagamenti del centro {IdDealer} non inviato", anno, idDealer);
                }
            }
            return inviati;
        }

        public static string NomeFile(string centro, int anno, int? mese)
        {
            var pulito = new string(centro.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
            return $"{InfrastructureMessages.Get("PaymentFileName")}_{pulito}_{anno}{(mese == null ? string.Empty : "_" + mese.Value.ToString("00", CultureInfo.InvariantCulture))}.pdf";
        }

        private async Task<PaymentSendResult> InviaAsync(int idDealer, int anno, int? mese, TipoRiepilogoPagamenti tipo, int? idRendiconto, bool automatico,
                                                         string oggetto, string testo, string nomeFile, byte[] pdf)
        {
            var esito = new PaymentSendResult();
            var destinatari = await DestinatariAsync(idDealer);
            esito.Destinatari = string.Join("; ", destinatari);
            if (!_emailService.Configured)
                esito.Errore = InfrastructureMessages.Get("PaymentEmailNotConfigured");
            else if (destinatari.Count == 0)
                esito.Errore = InfrastructureMessages.Get("PaymentNoRecipients");
            else
            {
                var html = string.Join(string.Empty, testo.Split('\n').Select(r => $"<p>{WebUtility.HtmlEncode(r)}</p>"));
                var allegato = new[] { new EmailAttachment { FileName = nomeFile, ContentType = "application/pdf", Content = pdf } };
                var errori = new List<string>();
                foreach (var email in destinatari)
                {
                    try
                    {
                        await _emailService.SendAsync(email, oggetto, html, testo, allegato);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Email dei pagamenti a {Email} non inviata", email);
                        errori.Add(email + ": " + ex.Message);
                    }
                }
                esito.Esito = errori.Count < destinatari.Count;
                if (errori.Count > 0)
                    esito.Errore = string.Join("; ", errori);
            }

            await _invioEmailRepository.CreateAsync(new Pag_InvioEmail
            {
                IdDealer = idDealer,
                Anno = anno,
                Mese = mese,
                Tipo = tipo,
                IdRendiconto = idRendiconto,
                Automatico = automatico,
                Destinatari = Taglia(esito.Destinatari, 1000),
                Esito = esito.Esito,
                Errore = Taglia(esito.Errore, 500)
            });
            return esito;
        }
    }
}
