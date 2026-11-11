using Jint;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /// <summary>Causale con i minuti (o l'importo) in un momento della prova.</summary>
    public class FormulaProvaCausale
    {
        public string Codice { get; set; } = string.Empty;
        public int Minuti { get; set; }
        public decimal? Importo { get; set; }
    }

    /// <summary>Riga di log prodotta dalle formule.</summary>
    public class FormulaLogRiga
    {
        public int? IdPar_Formule { get; set; }
        public FormulaLogTipo Tipo { get; set; }
        public string Messaggio { get; set; } = string.Empty;
    }

    /// <summary>Risultato di "Prova formula" su un giorno.</summary>
    public class FormulaProvaRisultato
    {
        public bool Eseguita { get; set; }
        public List<FormulaProvaCausale> CausaliPrima { get; set; } = new();
        public List<FormulaProvaCausale> CausaliDopo { get; set; } = new();
        public List<FormulaLogRiga> Log { get; set; } = new();
        public bool Warning { get; set; }
        public bool Anomalia { get; set; }
    }

    /*
     Esegue le formule nel calcolo giornata (TimeSheet_EngineService):
       PreparaRapportoAsync  -> all'inizio di CalcolaRapporto: licenza Pro, profili del rapporto nel periodo, righe, formule, codici
       Esegui(fase, giorno)  -> fase Giorno (dopo GeneraCausali) e fase Finale (prima di ScriviCausali)
       SalvaLogAsync         -> in SaveData: sostituisce i log dei giorni ricalcolati
     Un errore di una formula non ferma il calcolo: va nel log e alza Warning_2 sul giorno.
     Le formule sono una funzione del piano Pro: senza Pro non si esegue nulla (e i log restano com'erano).
    */
    public class FormuleRunner : IFormuleRunner
    {
        public const GG_ResultStato StatoWarningFormula = GG_ResultStato.Warning_2;
        public const GG_ResultStato StatoAnomaliaFormula = GG_ResultStato.Err_4;

        private readonly ILicenseGuard _licenseGuard;
        private readonly IDip_ProfiloFormuleRepository _dip_ProfiloFormuleRepository;
        private readonly IPar_ProfiloFormuleRepository _par_ProfiloFormuleRepository;
        private readonly IPar_ProfiloFormuleDettRepository _par_ProfiloFormuleDettRepository;
        private readonly IPar_FormuleRepository _par_FormuleRepository;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IDip_GG_FormuleLogRepository _dip_GG_FormuleLogRepository;

        private bool? _pro;
        private Engine? _engine;
        private readonly Dictionary<int, FormulaCodici> _codiciAzienda = new();

        // per rapporto: periodi del profilo, righe dei profili, formule
        private readonly Dictionary<int, List<Dip_ProfiloFormule>> _profiliRapporto = new();
        private readonly Dictionary<int, List<Par_ProfiloFormuleDett>> _righeProfilo = new();
        private readonly Dictionary<int, Par_Formule> _formule = new();
        private readonly Dictionary<int, int> _aziendaRapporto = new();

        // log e giorni eseguiti
        private readonly Dictionary<(int Rapporto, DateTime Giorno), List<FormulaLogRiga>> _log = new();

        // prova di una formula (non salvata): sostituisce il profilo
        private string? _provaTesto;
        private FormulaFase _provaFase;
        private DateTime _provaGiorno;
        private int _provaIdAzienda;

        public FormulaProvaRisultato? Prova { get; private set; }

        public FormuleRunner(ILicenseGuard licenseGuard,
                             IDip_ProfiloFormuleRepository dip_ProfiloFormuleRepository,
                             IPar_ProfiloFormuleRepository par_ProfiloFormuleRepository,
                             IPar_ProfiloFormuleDettRepository par_ProfiloFormuleDettRepository,
                             IPar_FormuleRepository par_FormuleRepository,
                             IPar_CausaliRepository par_CausaliRepository,
                             IDip_GG_FormuleLogRepository dip_GG_FormuleLogRepository)
        {
            _licenseGuard = licenseGuard;
            _dip_ProfiloFormuleRepository = dip_ProfiloFormuleRepository;
            _par_ProfiloFormuleRepository = par_ProfiloFormuleRepository;
            _par_ProfiloFormuleDettRepository = par_ProfiloFormuleDettRepository;
            _par_FormuleRepository = par_FormuleRepository;
            _par_CausaliRepository = par_CausaliRepository;
            _dip_GG_FormuleLogRepository = dip_GG_FormuleLogRepository;
        }

        public void ImpostaProva(string testo, FormulaFase fase, DateTime giorno, int idAzienda)
        {
            _provaTesto = testo;
            _provaFase = fase;
            _provaGiorno = giorno.Date;
            _provaIdAzienda = idAzienda;
            Prova = new FormulaProvaRisultato();
        }

        public async Task PreparaRapportoAsync(int idCompany, Dip_RapportoLavoroModel rapporto, DateTime dal, DateTime al, List<Par_GiustificativiModel> parGiustificativi)
        {
            _pro ??= await _licenseGuard.HasAtLeastAsync(idCompany, ApplicationType.AttendanceTracking, LivelloLicenza.Pro);
            if (_pro != true || _profiliRapporto.ContainsKey(rapporto.Id))
                return;

            var profili = _dip_ProfiloFormuleRepository.FindAll(p => p.IdDip_RapportoLavoro == rapporto.Id && p.Dal <= al.Date && p.Al >= dal.Date).ToList();
            _profiliRapporto[rapporto.Id] = profili;

            int? idAzienda = _provaTesto != null ? _provaIdAzienda : null;
            var idProfili = profili.Select(p => p.IdPar_ProfiloFormule).Distinct().ToList();
            foreach (var idProfilo in idProfili.Where(id => !_righeProfilo.ContainsKey(id)))
            {
                var testata = await _par_ProfiloFormuleRepository.FindByIdAsync(idProfilo);
                idAzienda ??= testata?.IdAz_Anagrafica;
                var righe = _par_ProfiloFormuleDettRepository.FindAll(r => r.IdPar_ProfiloFormule == idProfilo).OrderBy(r => r.Ordine).ThenBy(r => r.Id).ToList();
                _righeProfilo[idProfilo] = righe;

                var idFormule = righe.Select(r => r.IdPar_Formule).Where(id => !_formule.ContainsKey(id)).Distinct().ToList();
                foreach (var f in _par_FormuleRepository.FindAll(f => idFormule.Contains(f.Id)).ToList())
                    _formule[f.Id] = f;
            }

            if (idAzienda == null)
                return;
            _aziendaRapporto[rapporto.Id] = idAzienda.Value;
            if (!_codiciAzienda.ContainsKey(idAzienda.Value))
            {
                var codici = new FormulaCodici();
                foreach (var c in _par_CausaliRepository.FindAll(c => c.IdAz_Anagrafica == idAzienda.Value).ToList())
                {
                    if (string.IsNullOrWhiteSpace(c.Codice)) continue;
                    codici.Causali[c.Codice.Trim()] = (c.Id, c.Tipo);
                    codici.CodiceCausale[c.Id] = c.Codice.Trim();
                }
                foreach (var g in parGiustificativi.Where(g => !string.IsNullOrWhiteSpace(g.Codice)))
                    codici.Giustificativi[g.Codice!.Trim()] = g.Id;
                _codiciAzienda[idAzienda.Value] = codici;
            }
        }

        public GG_ResultStato Esegui(FormulaFase fase, FormulaDatiGiorno dati)
        {
            var stato = (GG_ResultStato)0;
            if (_pro != true || !_aziendaRapporto.TryGetValue(dati.Rapporto.Id, out var idAzienda))
                return stato;

            var giorno = dati.Giorno.Date;
            var chiave = (dati.Rapporto.Id, giorno);
            if (!_log.ContainsKey(chiave))
                _log[chiave] = new List<FormulaLogRiga>();

            // formule del giorno: in prova solo quella in prova, altrimenti le righe attive del profilo attivo
            var daEseguire = new List<(int? IdFormula, string Testo)>();
            if (_provaTesto != null)
            {
                if (giorno != _provaGiorno || fase != _provaFase) return stato;
                daEseguire.Add((null, _provaTesto));
            }
            else
            {
                var profilo = _profiliRapporto.TryGetValue(dati.Rapporto.Id, out var pp)
                    ? pp.FirstOrDefault(p => p.Dal.Date <= giorno && p.Al.Date >= giorno)
                    : null;
                if (profilo == null || !_righeProfilo.TryGetValue(profilo.IdPar_ProfiloFormule, out var righe))
                    return stato;
                foreach (var r in righe.Where(r => !r.Sospesa && r.Fase == fase && r.Dal.Date <= giorno && r.Al.Date >= giorno))
                    if (_formule.TryGetValue(r.IdPar_Formule, out var f) && !string.IsNullOrWhiteSpace(f.Testo))
                        daEseguire.Add((f.Id, f.Testo!));
            }
            if (daEseguire.Count == 0)
                return stato;

            var codici = _codiciAzienda[idAzienda];
            if (Prova != null)
                Prova.CausaliPrima = Fotografa(dati, codici);

            _engine ??= FormuleScriptHost.NuovoEngine();
            foreach (var (idFormula, testo) in daEseguire)
            {
                var righeLog = _log[chiave];
                void Log(FormulaLogTipo tipo, string messaggio)
                {
                    var m = messaggio.Length > 500 ? messaggio[..500] : messaggio;
                    righeLog.Add(new FormulaLogRiga { IdPar_Formule = idFormula, Tipo = tipo, Messaggio = m });
                }

                var ctx = new FormulaContext(dati, codici, Log, debug: Prova != null);
                try
                {
                    FormuleScriptHost.Esegui(_engine, testo, ctx);
                }
                catch (Exception ex)
                {
                    Log(FormulaLogTipo.Errore, FormuleScriptHost.MessaggioErrore(ex));
                    Dip_GG_Result_Helper.AddDetail(ref stato, StatoWarningFormula);
                    if (ex is not FormulaException)
                        Serilog.Log.Debug(ex, "Formula {IdFormula} in errore sul rapporto {Rapporto} giorno {Giorno}", idFormula, dati.Rapporto.Id, giorno);
                }
                if (ctx.WarningAlzato) Dip_GG_Result_Helper.AddDetail(ref stato, StatoWarningFormula);
                if (ctx.AnomaliaAlzata) Dip_GG_Result_Helper.AddDetail(ref stato, StatoAnomaliaFormula);
            }

            if (Prova != null)
            {
                Prova.Eseguita = true;
                Prova.CausaliDopo = Fotografa(dati, codici);
                Prova.Log = _log[chiave].ToList();
                Prova.Warning = stato.HasFlag(StatoWarningFormula);
                Prova.Anomalia = stato.HasFlag(StatoAnomaliaFormula);
            }
            return stato;
        }

        private static List<FormulaProvaCausale> Fotografa(FormulaDatiGiorno dati, FormulaCodici codici)
        {
            string Cod(int id) => codici.CodiceCausale.TryGetValue(id, out var c) ? c : id.ToString();
            return dati.Causali.Where(x => x.Value > TimeSpan.Zero)
                               .Select(x => new FormulaProvaCausale { Codice = Cod(x.Key), Minuti = (int)Math.Round(x.Value.TotalMinutes) })
                               .Concat(dati.Importi.Where(x => x.Value != 0)
                                               .Select(x => new FormulaProvaCausale { Codice = Cod(x.Key), Importo = x.Value }))
                               .OrderBy(x => x.Codice)
                               .ToList();
        }

        public async Task SalvaLogAsync()
        {
            if (_provaTesto != null || _log.Count == 0)
                return;

            foreach (var perRapporto in _log.GroupBy(x => x.Key.Rapporto))
            {
                var giorni = perRapporto.Select(x => x.Key.Giorno).ToHashSet();
                var min = giorni.Min();
                var max = giorni.Max();
                var esistenti = _dip_GG_FormuleLogRepository.FindAll(l => l.IdDip_RapportoLavoro == perRapporto.Key && l.Giorno >= min && l.Giorno <= max).ToList();
                foreach (var vecchio in esistenti.Where(l => giorni.Contains(l.Giorno.Date)))
                    await _dip_GG_FormuleLogRepository.DeleteAsync(vecchio);

                var adesso = DateTime.UtcNow;
                foreach (var giorno in perRapporto)
                    foreach (var riga in giorno.Value)
                        await _dip_GG_FormuleLogRepository.UpsertAsync(new Dip_GG_FormuleLog
                        {
                            IdDip_RapportoLavoro = perRapporto.Key,
                            Giorno = giorno.Key.Giorno,
                            IdPar_Formule = riga.IdPar_Formule,
                            Tipo = riga.Tipo,
                            Messaggio = riga.Messaggio,
                            Data = adesso
                        });
            }
            _log.Clear();
        }
    }

    public interface IFormuleRunner
    {
        FormulaProvaRisultato? Prova { get; }
        void ImpostaProva(string testo, FormulaFase fase, DateTime giorno, int idAzienda);
        Task PreparaRapportoAsync(int idCompany, Dip_RapportoLavoroModel rapporto, DateTime dal, DateTime al, List<Par_GiustificativiModel> parGiustificativi);
        GG_ResultStato Esegui(FormulaFase fase, FormulaDatiGiorno dati);
        Task SalvaLogAsync();
    }
}
