using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Text.Json;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService
{
    // Formule personalizzate del calcolo giornata (parametro dell'azienda, CompanyPowerAdmin, piano Pro). ANALISI_ATTENDANCETRACKING_FORMULE.md
    public class Par_FormuleService : ServiceBase, IPar_FormuleService
    {
        private readonly IPar_FormuleRepository _par_FormuleRepository;
        private readonly IPar_ProfiloFormuleDettRepository _par_ProfiloFormuleDettRepository;
        private readonly IDip_GG_FormuleLogRepository _dip_GG_FormuleLogRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IFormuleAccesso _formuleAccesso;
        private readonly ITimeSheet_EngineService _timeSheet_EngineService;
        private readonly ILlmClient _llmClient;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public Par_FormuleService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_FormuleRepository par_FormuleRepository,
                                  IPar_ProfiloFormuleDettRepository par_ProfiloFormuleDettRepository,
                                  IDip_GG_FormuleLogRepository dip_GG_FormuleLogRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IFormuleAccesso formuleAccesso,
                                  ITimeSheet_EngineService timeSheet_EngineService,
                                  ILlmClient llmClient,
                                  IPar_CausaliRepository par_CausaliRepository,
                                  IPar_GiustificativiRepository par_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_FormuleRepository = par_FormuleRepository;
            _par_ProfiloFormuleDettRepository = par_ProfiloFormuleDettRepository;
            _dip_GG_FormuleLogRepository = dip_GG_FormuleLogRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _formuleAccesso = formuleAccesso;
            _timeSheet_EngineService = timeSheet_EngineService;
            _llmClient = llmClient;
            _par_CausaliRepository = par_CausaliRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        private Task<int> Azienda() => _formuleAccesso.VerificaAsync(this.CurrentUserId, this.CurrentCompany);

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);

        // codici di causali e giustificativi dell'azienda: per il controllo dei codici scritti nella formula
        private FormulaCodici Codici(int idAzienda)
        {
            var codici = new FormulaCodici();
            foreach (var c in _par_CausaliRepository.FindAll(c => c.IdAz_Anagrafica == idAzienda).ToList())
                if (!string.IsNullOrWhiteSpace(c.Codice))
                {
                    codici.Causali[c.Codice.Trim()] = (c.Id, c.Tipo);
                    codici.CodiceCausale[c.Id] = c.Codice.Trim();
                }
            foreach (var g in _par_GiustificativiRepository.FindAll(g => g.IdAz_Anagrafica == idAzienda).ToList())
                if (!string.IsNullOrWhiteSpace(g.Codice))
                    codici.Giustificativi[g.Codice.Trim()] = g.Id;
            return codici;
        }

        // elenco di causali e giustificativi dell'azienda per il prompt dell'AI (configurazione, nessun dato dei dipendenti)
        private string CodiciPerAI(int idAzienda)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Causali dell'azienda (codice - descrizione - tipo):\n");
            foreach (var c in _par_CausaliRepository.FindAll(c => c.IdAz_Anagrafica == idAzienda).OrderBy(c => c.Codice).ToList())
                sb.Append($"{c.Codice} - {c.Descrizione} - {(c.Tipo == TipoValore.Importo ? "importo" : "ore")}\n");
            sb.Append("\nGiustificativi dell'azienda (codice - descrizione):\n");
            foreach (var g in _par_GiustificativiRepository.FindAll(g => g.IdAz_Anagrafica == idAzienda).OrderBy(g => g.Codice).ToList())
                sb.Append($"{g.Codice} - {g.Descrizione}\n");
            return sb.ToString();
        }

        private static string Riassunto(List<FormulaErrore> errori)
            => string.Join("; ", errori.Take(3).Select(e => $"riga {e.Riga}: {e.Messaggio}"));

        public virtual async Task<GenericResult<Par_FormuleOutModel>> GetAll(GenericRequest<Par_FormuleInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var formule = _par_FormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda).OrderBy(x => x.Codice).ToList();
                return new Par_FormuleOutModel { Par_Formule = _mapper.Map<List<Par_FormuleModel>>(formule) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_FormuleGetOutModel>> Par_FormuleGet(GenericRequest<Par_FormuleGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_FormuleGetOutModel();
                var formula = model.Data.Id > 0 ? await _par_FormuleRepository.FindByIdAsync(model.Data.Id) : null;
                retVal.Par_Formule = formula != null && formula.IdAz_Anagrafica == idAzienda
                    ? _mapper.Map<Par_FormuleModel>(formula)
                    : new Par_FormuleModel { IdAz_Anagrafica = idAzienda, Testo = FormuleCatalogo.Template };
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_FormulePutOutModel>> Par_FormulePut(GenericRequest<Par_FormulePutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_FormulePutOutModel { Par_Formule = model.Data.Par_Formule };
                var m = model.Data.Par_Formule;
                m.Codice = m.Codice?.Trim();
                m.Titolo = m.Titolo?.Trim();
                if (string.IsNullOrWhiteSpace(m.Codice) || string.IsNullOrWhiteSpace(m.Titolo))
                    throw new UserMessageException(T("FormulaCodiceObbligatorio"));

                if (_par_FormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda && x.Codice == m.Codice && x.Id != m.Id).Any())
                    throw new UserMessageException(T("FormulaCodiceDuplicato", m.Codice));

                // una formula con errori non si salva: al calcolo fallirebbe su ogni giorno
                retVal.Errori = FormuleScriptHost.CtrlSintassi(m.Testo, Codici(idAzienda));
                if (retVal.Errori.Count > 0)
                {
                    retVal.Messages.Add(new Message(T("FormulaNonValida", Riassunto(retVal.Errori)), MessageType.Exception));
                    return retVal;
                }

                if (m.Id > 0)
                {
                    var esistente = await _par_FormuleRepository.FindByIdAsync(m.Id);
                    if (esistente == null || esistente.IdAz_Anagrafica != idAzienda)
                        m.Id = 0;
                }
                var entity = _mapper.Map<Par_Formule>(m);
                entity.IdAz_Anagrafica = idAzienda;
                entity.Hash = FormuleScriptHost.Impronta(entity.Testo);
                entity = await _par_FormuleRepository.UpsertAsync(entity);
                retVal.Par_Formule = _mapper.Map<Par_FormuleModel>(entity);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_DeleteOutModel>> Par_FormuleDelete(GenericRequest<Par_Formule_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_Formule_DeleteOutModel();

                int usata = _par_ProfiloFormuleDettRepository.FindAll(x => x.IdPar_Formule == model.Data.Id).Count();
                if (usata > 0)
                {
                    retVal.Messages.Add(new Message(T("FormulaInUso", usata), MessageType.Exception));
                    return retVal;
                }

                var entity = await _par_FormuleRepository.FindByIdAsync(model.Data.Id);
                if (entity != null && entity.IdAz_Anagrafica == idAzienda)
                    await _par_FormuleRepository.DeleteAsync(entity);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_DuplicaOutModel>> Par_FormuleDuplica(GenericRequest<Par_Formule_DuplicaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var origine = await _par_FormuleRepository.FindByIdAsync(model.Data.Id);
                if (origine == null || origine.IdAz_Anagrafica != idAzienda)
                    throw new UserMessageException(T("FormulaCodiceObbligatorio"));

                var codice = model.Data.Codice?.Trim();
                var titolo = string.IsNullOrWhiteSpace(model.Data.Titolo) ? "copia di " + origine.Titolo : model.Data.Titolo.Trim();
                if (string.IsNullOrWhiteSpace(codice))
                    throw new UserMessageException(T("FormulaCodiceObbligatorio"));
                if (_par_FormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda && x.Codice == codice).Any())
                    throw new UserMessageException(T("FormulaCodiceDuplicato", codice));

                var copia = new Par_Formule
                {
                    IdAz_Anagrafica = idAzienda,
                    Codice = codice,
                    Titolo = titolo.Length > 60 ? titolo[..60] : titolo,
                    Descrizione = origine.Descrizione,
                    Testo = origine.Testo,
                    Hash = origine.Hash
                };
                copia = await _par_FormuleRepository.UpsertAsync(copia);
                return new Par_Formule_DuplicaOutModel { Par_Formule = _mapper.Map<Par_FormuleModel>(copia) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_CtrlSintassiOutModel>> CtrlSintassi(GenericRequest<Par_Formule_CtrlSintassiInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                return new Par_Formule_CtrlSintassiOutModel { Errori = FormuleScriptHost.CtrlSintassi(model.Data.Testo, Codici(idAzienda)) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_MetodiOutModel>> Metodi(GenericRequest<Par_Formule_MetodiInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await Azienda();
                return new Par_Formule_MetodiOutModel { Metodi = FormuleCatalogo.Metodi, Template = FormuleCatalogo.Template };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_RapportiProvaOutModel>> RapportiProva(GenericRequest<Par_Formule_RapportiProvaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var anagrafiche = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda).ToList();
                var ids = anagrafiche.Select(a => a.Id).ToList();
                var rapporti = _dip_RapportoLavoroRepository.FindAll(r => ids.Contains(r.IdDip_Anagrafica)).ToList();

                var retVal = new Par_Formule_RapportiProvaOutModel();
                foreach (var r in rapporti)
                {
                    var a = anagrafiche.First(x => x.Id == r.IdDip_Anagrafica);
                    var periodo = r.DataAss.HasValue ? $" (dal {r.DataAss:dd/MM/yyyy}{(r.DataLic.HasValue ? $" al {r.DataLic:dd/MM/yyyy}" : "")})" : "";
                    retVal.Rapporti.Add(new Par_Formule_RapportoProvaModel
                    {
                        IdDip_RapportoLavoro = r.Id,
                        Descrizione = $"{a.Cognome} {a.Nome}".Trim() + periodo
                    });
                }
                retVal.Rapporti = retVal.Rapporti.OrderBy(x => x.Descrizione).ToList();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_ProvaOutModel>> Prova(GenericRequest<Par_Formule_ProvaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_Formule_ProvaOutModel { Errori = FormuleScriptHost.CtrlSintassi(model.Data.Testo, Codici(idAzienda)) };
                if (retVal.Errori.Count > 0)
                    return retVal;

                var rapporto = await _dip_RapportoLavoroRepository.FindByIdAsync(model.Data.IdDip_RapportoLavoro);
                var anagrafica = rapporto != null ? await _dip_AnagraficaRepository.FindByIdAsync(rapporto.IdDip_Anagrafica) : null;
                if (anagrafica == null || anagrafica.IdAz_Anagrafica != idAzienda || string.IsNullOrEmpty(anagrafica.IdAspNetUsers))
                    throw new UserMessageException(T("FormulaProvaRapporto"));

                retVal.Risultato = await _timeSheet_EngineService.SimulaFormula(anagrafica.IdAspNetUsers, rapporto!.Id, model.Data.Giorno.Date,
                                                                                model.Data.Testo!, model.Data.Fase, idAzienda);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Formule_LogOutModel>> Log(GenericRequest<Par_Formule_LogInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var anagrafiche = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda).ToList();
                var idAnagrafiche = anagrafiche.Select(a => a.Id).ToList();
                var rapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafiche.Contains(r.IdDip_Anagrafica)).ToList();
                var idRapporti = rapporti.Select(r => r.Id).ToList();
                var formule = _par_FormuleRepository.FindAll(f => f.IdAz_Anagrafica == idAzienda).ToDictionary(f => f.Id, f => f.Codice ?? "");

                var dal = model.Data.Dal.Date;
                var al = model.Data.Al.Date;
                var idFormula = model.Data.IdPar_Formule;
                var righe = _dip_GG_FormuleLogRepository.FindAll(l => idRapporti.Contains(l.IdDip_RapportoLavoro) && l.Giorno >= dal && l.Giorno <= al &&
                                                                      (idFormula == null || l.IdPar_Formule == idFormula))
                                                        .OrderByDescending(l => l.Giorno).ThenBy(l => l.Id)
                                                        .Take(1000)
                                                        .ToList();

                var retVal = new Par_Formule_LogOutModel();
                foreach (var l in righe)
                {
                    var r = rapporti.First(x => x.Id == l.IdDip_RapportoLavoro);
                    var a = anagrafiche.First(x => x.Id == r.IdDip_Anagrafica);
                    retVal.Log.Add(new Par_Formule_LogRigaModel
                    {
                        Giorno = l.Giorno,
                        Dipendente = $"{a.Cognome} {a.Nome}".Trim(),
                        Formula = l.IdPar_Formule.HasValue && formule.TryGetValue(l.IdPar_Formule.Value, out var cod) ? cod : "",
                        Tipo = l.Tipo,
                        Messaggio = l.Messaggio,
                        Data = l.Data
                    });
                }
                return retVal;
            }, isSubProcess);
        }

        #region "AI formula"

        private static readonly object SchemaAI = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["codice"] = new Dictionary<string, object> { ["type"] = "string" },
                ["spiegazione"] = new Dictionary<string, object> { ["type"] = "string" }
            },
            ["required"] = new[] { "codice", "spiegazione" },
            ["additionalProperties"] = false
        };

        // versione del prompt: cambiarla quando cambiano regole o catalogo
        private const string VersionePromptAI = "formule-v1";

        private static string SystemPromptAI(string codiciAzienda) =>
$@"Sei un assistente che scrive FORMULE per il calcolo delle presenze di un software HR ({VersionePromptAI}).
Una formula e' uno script JavaScript (ES2020) eseguito sul server per ogni dipendente e giorno.
Regole:
- l'unico oggetto disponibile e' ""x"" (vedi elenco sotto): non esistono require, import, fetch, console, file o rete;
- le durate sono in MINUTI interi; causali e giustificativi si indicano con il loro CODICE (stringa, es. ""ORDI"");
- il testo e' il corpo di una funzione: si puo' usare ""return"" per uscire;
- comincia sempre con il blocco di applicabilita' x.Attiva... e if (!x.Attiva.ApplicaLaFormula()) return;
- usa solo i membri di x elencati, con le maiuscole esatte; commenta in italiano le righe principali;
- usa SOLO i codici di causali e giustificativi dell'azienda elencati sotto (ORDI e STR1 dell'esempio sono inventati);
- niente cicli infiniti: ci sono limiti di tempo e di istruzioni.

Membri di x:
{FormuleCatalogo.ComeTesto()}
{codiciAzienda}
Esempio (le ore ORDI oltre le 8 vanno su STR1, con avviso nel cartellino):
{FormuleCatalogo.Template.Replace("// x.Causale.TogliEccedenza", "x.Causale.TogliEccedenza")}
if (x.Causale.GetMin(""STR1"") > 0) x.Stato.Warning(""Straordinario: "" + x.Time.HHMM(x.Causale.GetMin(""STR1"")));

Rispondi SOLO con un JSON {{ ""codice"": ""<testo della formula>"", ""spiegazione"": ""<breve spiegazione in italiano>"" }}.";

        public virtual async Task<GenericResult<Par_Formule_GeneraAIOutModel>> GeneraAI(GenericRequest<Par_Formule_GeneraAIInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var codici = Codici(idAzienda);
                var codiciAI = CodiciPerAI(idAzienda);
                var richiesta = (model.Data.Richiesta ?? string.Empty).Trim();
                if (richiesta.Length == 0)
                    throw new UserMessageException(T("FormulaAIRispostaNonValida", "richiesta vuota"));
                if (richiesta.Length > 4000) richiesta = richiesta[..4000];

                // all'AI vanno solo la richiesta, la formula attuale e la documentazione: nessun dato dei dipendenti
                var messaggio = "Richiesta:\n" + richiesta;
                if (!string.IsNullOrWhiteSpace(model.Data.TestoAttuale))
                    messaggio += "\n\nFormula attuale (da modificare o completare):\n" + model.Data.TestoAttuale;

                var provider = _configuration["AI:FormuleLLM"];
                var retVal = new Par_Formule_GeneraAIOutModel();
                for (int tentativo = 1; tentativo <= 2; tentativo++)
                {
                    var risposta = await _llmClient.CompleteJsonAsync(SystemPromptAI(codiciAI), messaggio, SchemaAI, "formula",
                                                                      provider: string.IsNullOrWhiteSpace(provider) ? null : provider, maxTokens: 3000);
                    var (codice, spiegazione) = LeggiRispostaAI(risposta);
                    if (codice == null)
                        throw new UserMessageException(T("FormulaAIRispostaNonValida", "risposta non leggibile"));

                    retVal.Testo = codice;
                    retVal.Spiegazione = spiegazione ?? string.Empty;
                    retVal.Errori = FormuleScriptHost.CtrlSintassi(codice, codici);
                    if (retVal.Errori.Count == 0)
                        break;

                    // un secondo tentativo con gli errori trovati
                    Serilog.Log.Information("Formula AI con errori (tentativo {Tentativo}): {Errori}", tentativo, Riassunto(retVal.Errori));
                    messaggio += $"\n\nLa formula che hai proposto ha questi errori, correggili:\n{Riassunto(retVal.Errori)}\n\nFormula proposta:\n{codice}";
                }
                return retVal;
            }, isSubProcess);
        }

        private static (string? Codice, string? Spiegazione) LeggiRispostaAI(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return (null, null);
            var inizio = json.IndexOf('{');
            var fine = json.LastIndexOf('}');
            if (inizio < 0 || fine <= inizio) return (null, null);
            try
            {
                using var doc = JsonDocument.Parse(json[inizio..(fine + 1)]);
                var root = doc.RootElement;
                string? Prop(string nome) => root.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                return (Prop("codice"), Prop("spiegazione"));
            }
            catch (JsonException)
            {
                return (null, null);
            }
        }

        #endregion
    }

    public interface IPar_FormuleService : IServiceBase
    {
        Task<GenericResult<Par_FormuleOutModel>> GetAll(GenericRequest<Par_FormuleInModel> model, bool isSubProcess);
        Task<GenericResult<Par_FormuleGetOutModel>> Par_FormuleGet(GenericRequest<Par_FormuleGetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_FormulePutOutModel>> Par_FormulePut(GenericRequest<Par_FormulePutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_DeleteOutModel>> Par_FormuleDelete(GenericRequest<Par_Formule_DeleteInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_DuplicaOutModel>> Par_FormuleDuplica(GenericRequest<Par_Formule_DuplicaInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_CtrlSintassiOutModel>> CtrlSintassi(GenericRequest<Par_Formule_CtrlSintassiInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_MetodiOutModel>> Metodi(GenericRequest<Par_Formule_MetodiInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_RapportiProvaOutModel>> RapportiProva(GenericRequest<Par_Formule_RapportiProvaInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_ProvaOutModel>> Prova(GenericRequest<Par_Formule_ProvaInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_LogOutModel>> Log(GenericRequest<Par_Formule_LogInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Formule_GeneraAIOutModel>> GeneraAI(GenericRequest<Par_Formule_GeneraAIInModel> model, bool isSubProcess);
    }
}
