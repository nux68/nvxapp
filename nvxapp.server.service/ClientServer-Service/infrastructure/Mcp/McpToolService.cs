using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp
{
    /// <summary>strumento MCP: un comando della chat (ICommandHandler) visto da un'AI esterna</summary>
    public class McpToolDescriptor
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<McpToolParameter> Parameters { get; set; } = new();
        /// <summary>vero = non modifica dati: il client AI puo' eseguirlo senza chiedere il permesso</summary>
        public bool ReadOnly { get; set; }
    }

    public class McpToolParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
    }

    public class McpToolCallResult
    {
        public bool Success { get; set; }
        /// <summary>risposta per l'utente (il messaggio del comando, oppure l'errore che l'AI puo' correggere)</summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>dati del risultato in JSON, quando il comando li restituisce; null = solo testo</summary>
        public string? DataJson { get; set; }
    }

    /*
     Adattatore da comando della chat a strumento MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md, sezioni 4, 5 e 6).
     Non conosce il protocollo MCP: l'endpoint (nvxapp.server, Utility/McpEndpoint.cs) traduce questi modelli.

     Elenco degli strumenti = tutti gli ICommandHandler della DI che l'utente puo' usare:
        - ruolo dell'utente (IntentDefinition.Usable4Role) e applicativi attivi dell'azienda (IntentDefinition.Application),
          come nella chat
        - livello del comando (IntentDefinition.EffectiveMcpAccess) entro quello dell'azienda (McpCompanySetting, non oltre
          Mcp:MaxLevel); i comandi della sola interfaccia sono esclusi
        - se l'utente lavora in un'azienda con l'accesso AI spento: nessun comando
     In piu' lo strumento fisso "nvx_whoami" (sempre presente): dice all'AI per chi sta lavorando e perche' vede o non
     vede gli strumenti.

     Chiamata di uno strumento: i parametri diventano gli slot del comando, con la stessa normalizzazione e validazione
     della chat (CommandSlotProcessor); gli errori tornano come testo, cosi' l'AI corregge e riprova. Ogni chiamata e'
     registrata (McpCallLog).

     Limiti (MCP-21): chiamate per token al minuto e al giorno (Mcp:MaxCallsPerMinute / MaxCallsPerDay), contate sul
     registro delle chiamate, quindi validi anche con piu' istanze del server; ogni elenco dei dati restituiti si ferma a
     Mcp:MaxRows righe, e il testo avvisa l'AI che il risultato e' parziale.

     L'identita' e' quella dei claim della richiesta, costruiti dal token personale (McpTokenAuthenticationHandler):
     i comandi e i servizi che usano lavorano come nell'applicazione, senza sapere di MCP.
    */
    public class McpToolService : ServiceBase, IMcpToolService
    {
        public const string WhoAmITool = "nvx_whoami";
        // claim aggiunti dall'autenticazione del token personale
        public const string ClaimTokenId = "mcp_token_id";
        public const string ClaimTokenName = "mcp_token_name";
        public const string ClaimLanguage = "mcp_lang";

        private const int MaxDataChars = 200_000;

        private readonly IEnumerable<ICommandHandler> _handlers;
        private readonly IChatUserContext _chatUserContext;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMcpCompanySettingRepository _mcpCompanySettingRepository;
        private readonly IMcpAccessTokenRepository _mcpAccessTokenRepository;
        private readonly IMcpCallLogRepository _mcpCallLogRepository;

        public McpToolService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,

                              IEnumerable<ICommandHandler> handlers,
                              IChatUserContext chatUserContext,
                              ICompanyApplicationRepository companyApplicationRepository,
                              ICompanyRepository companyRepository,
                              IMcpCompanySettingRepository mcpCompanySettingRepository,
                              IMcpAccessTokenRepository mcpAccessTokenRepository,
                              IMcpCallLogRepository mcpCallLogRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _handlers = handlers;
            _chatUserContext = chatUserContext;
            _companyApplicationRepository = companyApplicationRepository;
            _companyRepository = companyRepository;
            _mcpCompanySettingRepository = mcpCompanySettingRepository;
            _mcpAccessTokenRepository = mcpAccessTokenRepository;
            _mcpCallLogRepository = mcpCallLogRepository;
        }

        // ---------------------------------------------------------------------------
        // Elenco degli strumenti
        // ---------------------------------------------------------------------------

        public virtual async Task<List<McpToolDescriptor>> ListToolsAsync()
        {
            var contesto = await ContestoAsync();
            var strumenti = new List<McpToolDescriptor>
            {
                new()
                {
                    Name = WhoAmITool,
                    Title = InfrastructureMessages.Get("McpWhoAmITitle"),
                    Description = InfrastructureMessages.Get("McpWhoAmIDescription"),
                    ReadOnly = true
                }
            };
            strumenti.AddRange(contesto.Strumenti.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => Descrittore(x.Key, x.Value.IntentDefinition)));
            return strumenti;
        }

        private static McpToolDescriptor Descrittore(string nome, IntentDefinition def)
        {
            var descrizione = new StringBuilder(def.Description.Trim());
            if (def.Examples.Count > 0)
            {
                if (descrizione.Length > 0 && descrizione[^1] != '.') descrizione.Append('.');
                descrizione.Append(' ').Append(InfrastructureMessages.Get("McpToolExamples", string.Join(" | ", def.Examples)));
            }

            return new McpToolDescriptor
            {
                Name = nome,
                Title = string.IsNullOrWhiteSpace(def.DisplayName) ? def.Name : def.DisplayName,
                Description = descrizione.ToString(),
                ReadOnly = def.EffectiveMcpAccess == McpAccessLevel.Read,
                Parameters = def.Slots.Select(slot => new McpToolParameter
                {
                    Name = slot.Name,
                    Required = slot.Required,
                    Description = DescrizioneParametro(slot)
                }).ToList()
            };
        }

        // descrizione dello slot scritta per il LLM della chat, piu' formato, valore se omesso e "@me" per le persone
        private static string DescrizioneParametro(SlotDefinition slot)
        {
            var testo = new StringBuilder(string.IsNullOrWhiteSpace(slot.PromptDescription) ? slot.Label : slot.PromptDescription.Trim());
            void Aggiungi(string frase)
            {
                if (testo.Length > 0 && testo[^1] != '.') testo.Append('.');
                if (testo.Length > 0) testo.Append(' ');
                testo.Append(frase);
            }

            if (slot.Type == SlotNormalizers.DateFormat || slot.Type == SlotNormalizers.TimeFormat)
                Aggiungi(InfrastructureMessages.Get("McpParamFormat", slot.Type));
            if (!string.IsNullOrEmpty(slot.Default))
                Aggiungi(InfrastructureMessages.Get("McpParamDefault", slot.Default));
            if (slot.IsPerson)
                Aggiungi(InfrastructureMessages.Get("McpParamSelf", ChatSelf.Token));
            return testo.ToString();
        }

        // ---------------------------------------------------------------------------
        // Chiamata di uno strumento
        // ---------------------------------------------------------------------------

        public virtual async Task<McpToolCallResult> CallToolAsync(string name, IReadOnlyDictionary<string, string?> arguments)
        {
            var cronometro = Stopwatch.StartNew();
            McpToolCallResult risultato;
            Contesto? contesto = null;
            try
            {
                contesto = await ContestoAsync();
                var limite = await LimiteSuperatoAsync();
                risultato = limite != null
                    ? Errore(limite)
                    : await EseguiAsync(contesto, name ?? string.Empty, arguments);
            }
            catch (Exception ex)
            {
                // errore previsto: il messaggio e' per l'utente; errore tecnico: dettaglio solo nel log
                if (ex.IsUserMessage())
                    risultato = Errore(ex.Message);
                else
                {
                    Log.Error(ex, "[MCP] Strumento {Tool} fallito", name);
                    risultato = Errore(ChatAIMessages.Get("CommandExecutionError"));
                }
            }
            cronometro.Stop();

            await RegistraAsync(contesto, name ?? string.Empty, arguments, risultato, (int)cronometro.ElapsedMilliseconds);
            return risultato;
        }

        private async Task<McpToolCallResult> EseguiAsync(Contesto contesto, string name, IReadOnlyDictionary<string, string?> arguments)
        {
            if (name == WhoAmITool)
                return new McpToolCallResult { Success = true, Text = await ChiSonoAsync(contesto) };

            if (!contesto.Strumenti.TryGetValue(name, out var handler))
            {
                if (contesto.AccessoAziendaSpento)
                    return Errore(InfrastructureMessages.Get("McpCompanyDisabled", contesto.Azienda));
                var disponibili = new[] { WhoAmITool }.Concat(contesto.Strumenti.Keys.OrderBy(x => x, StringComparer.Ordinal));
                return Errore(InfrastructureMessages.Get("McpToolUnknown", name, string.Join(", ", disponibili)));
            }

            var def = handler.IntentDefinition;

            // parametri -> slot: solo quelli previsti dal comando, normalizzati come nella chat
            var slots = new Dictionary<string, string>();
            foreach (var argomento in arguments)
            {
                var slotDef = def.Slots.FirstOrDefault(s => s.Name.Equals(argomento.Key, StringComparison.OrdinalIgnoreCase));
                if (slotDef == null)
                    return Errore(InfrastructureMessages.Get("McpParamUnknown", argomento.Key,
                        def.Slots.Count > 0 ? string.Join(", ", def.Slots.Select(s => s.Name)) : "-"));

                string valore = (argomento.Value ?? string.Empty).Trim();
                if (valore.Length == 0)
                    continue;

                if (slotDef.IsPerson && valore.Equals(ChatSelf.Token, StringComparison.OrdinalIgnoreCase))
                {
                    slots[slotDef.Name] = ChatSelf.Token;
                    continue;
                }

                var normalizer = CommandSlotProcessor.NormalizerFor(def, slotDef);
                slots[slotDef.Name] = normalizer?.Invoke(valore) ?? valore;
            }
            // un periodo indicato solo con l'inizio ("ottobre") vale fino al suo ultimo giorno
            foreach (var slotDef in def.Slots.Where(s => !string.IsNullOrEmpty(s.PeriodEndSlot)))
            {
                var originale = arguments.FirstOrDefault(a => a.Key.Equals(slotDef.Name, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrWhiteSpace(originale))
                    CommandSlotProcessor.FillPeriodEnd(slots, slotDef, originale.Trim());
            }

            var validazione = CommandSlotProcessor.Validate(def, slots);
            if (!validazione.IsValid)
            {
                var messaggio = new StringBuilder(validazione.MessageToUser);
                if (validazione.Suggestions.Count > 0)
                    messaggio.Append(' ').Append(InfrastructureMessages.Get("McpParamSuggestions", string.Join(" | ", validazione.Suggestions)));
                return Errore(messaggio.ToString());
            }

            var mancanti = CommandSlotProcessor.MissingRequired(def, slots);
            if (mancanti.Count > 0)
                return Errore(InfrastructureMessages.Get("McpParamMissing", string.Join(", ", mancanti)));

            var esito = await handler.ExecuteAsync(slots);

            var testo = new StringBuilder();
            if (!string.IsNullOrEmpty(validazione.InfoMessage))
                testo.AppendLine(validazione.InfoMessage);
            testo.Append(esito.Message);

            string? dati = null;
            if (esito.Success)
            {
                int maxRighe = McpOptions.MaxRows(_configuration);
                (dati, bool troncati) = Dati(esito.Data, maxRighe);
                if (troncati)
                    testo.AppendLine().Append(InfrastructureMessages.Get("McpRowsTruncated", maxRighe));
            }

            return new McpToolCallResult
            {
                Success = esito.Success,
                Text = testo.ToString(),
                DataJson = dati
            };
        }

        private static McpToolCallResult Errore(string messaggio) => new() { Success = false, Text = messaggio };

        // limiti di frequenza del token (MCP-21), contati sul registro delle chiamate: null = chiamata ammessa.
        // Anche le chiamate respinte finiscono nel registro e contano: un client che insiste resta fermo.
        private async Task<string?> LimiteSuperatoAsync()
        {
            var token = TokenCorrente();
            if (token.Id == null)
                return null;

            int alMinuto = McpOptions.MaxCallsPerMinute(_configuration);
            if (alMinuto > 0)
            {
                var daUnMinuto = DateTime.Now.AddMinutes(-1);
                int ultimoMinuto = await _mcpCallLogRepository.FindAll(x => x.IdMcpAccessToken == token.Id && x.Data >= daUnMinuto).CountAsync();
                if (ultimoMinuto >= alMinuto)
                    return InfrastructureMessages.Get("McpRateLimitMinute", alMinuto);
            }

            int alGiorno = McpOptions.MaxCallsPerDay(_configuration);
            if (alGiorno > 0)
            {
                var oggi = DateTime.Today;
                int diOggi = await _mcpCallLogRepository.FindAll(x => x.IdMcpAccessToken == token.Id && x.Data >= oggi).CountAsync();
                if (diOggi >= alGiorno)
                    return InfrastructureMessages.Get("McpRateLimitDay", alGiorno);
            }
            return null;
        }

        // dati del comando in JSON; i payload per il client (navigazione, azioni dell'interfaccia) non servono a un'AI.
        // Ogni elenco, a qualunque livello, si ferma a maxRighe elementi (troncati = true se qualcosa e' stato omesso).
        private static (string? Json, bool Troncati) Dati(object? data, int maxRighe)
        {
            if (data == null || data is string || data is NavigatePayload || data is ClientActionPayload)
                return (null, false);
            try
            {
                var nodo = JsonSerializer.SerializeToNode(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });
                if (nodo == null)
                    return (null, false);

                bool troncati = TagliaElenchi(nodo, maxRighe);
                var json = nodo.ToJsonString();
                return (json.Length > MaxDataChars || json == "{}" || json == "[]" ? null : json, troncati);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[MCP] Dati del comando non serializzabili ({Type})", data.GetType().Name);
                return (null, false);
            }
        }

        private static bool TagliaElenchi(JsonNode nodo, int maxRighe)
        {
            bool troncati = false;
            switch (nodo)
            {
                case JsonArray elenco:
                    while (elenco.Count > maxRighe)
                    {
                        elenco.RemoveAt(elenco.Count - 1);
                        troncati = true;
                    }
                    foreach (var figlio in elenco)
                        if (figlio != null)
                            troncati |= TagliaElenchi(figlio, maxRighe);
                    break;
                case JsonObject oggetto:
                    foreach (var proprieta in oggetto)
                        if (proprieta.Value != null)
                            troncati |= TagliaElenchi(proprieta.Value, maxRighe);
                    break;
            }
            return troncati;
        }

        // ---------------------------------------------------------------------------
        // "Chi sono"
        // ---------------------------------------------------------------------------

        private async Task<string> ChiSonoAsync(Contesto contesto)
        {
            var testo = new StringBuilder();
            testo.AppendLine(InfrastructureMessages.Get("McpWhoUser", contesto.UserName));

            var login = this.UserIdFirstConnection;
            if (!string.IsNullOrEmpty(login) && login != this.CurrentUserId)
            {
                var nomeLogin = await _aspNetUsersRepository.FindAll(x => x.Id == login).Select(x => x.UserName).FirstOrDefaultAsync();
                testo.AppendLine(InfrastructureMessages.Get("McpWhoLogin", nomeLogin ?? string.Empty));
            }

            testo.AppendLine(contesto.IdCompany != null
                ? InfrastructureMessages.Get("McpWhoCompany", contesto.Azienda)
                : InfrastructureMessages.Get("McpWhoNoCompany"));
            testo.AppendLine(InfrastructureMessages.Get("McpWhoRoles", contesto.Ruoli.Count > 0 ? string.Join(", ", contesto.Ruoli) : "-"));

            if (contesto.IdCompany != null)
            {
                testo.AppendLine(InfrastructureMessages.Get("McpWhoApplications",
                    contesto.Applicativi.Count > 0 ? string.Join(", ", contesto.Applicativi) : "-"));
                testo.AppendLine(contesto.AccessoAziendaSpento
                    ? InfrastructureMessages.Get("McpCompanyDisabled", contesto.Azienda)
                    : InfrastructureMessages.Get("McpWhoAccessOn", InfrastructureMessages.Get("McpLevel" + (int)contesto.Livello)));
            }

            var token = TokenCorrente();
            if (token.Id != null)
            {
                var scadenza = await _mcpAccessTokenRepository.FindAll(x => x.Id == token.Id).Select(x => (DateTime?)x.ExpiresAt).FirstOrDefaultAsync();
                if (scadenza != null)
                    testo.AppendLine(InfrastructureMessages.Get("McpWhoToken", token.Name ?? string.Empty, scadenza.Value.ToString("dd/MM/yyyy")));
            }

            testo.Append(InfrastructureMessages.Get("McpWhoTools", contesto.Strumenti.Count));
            return testo.ToString();
        }

        // ---------------------------------------------------------------------------
        // Contesto della richiesta: utente, azienda, livello e strumenti disponibili
        // ---------------------------------------------------------------------------

        private class Contesto
        {
            public string UserName = string.Empty;
            public List<string> Ruoli = new();
            public int? IdCompany;
            public string Azienda = string.Empty;
            public List<ApplicationType> Applicativi = new();
            public bool AccessoAziendaSpento;
            public McpAccessLevel Livello = McpAccessLevel.Read;
            public Dictionary<string, ICommandHandler> Strumenti = new(StringComparer.Ordinal);
        }

        private async Task<Contesto> ContestoAsync()
        {
            var contesto = new Contesto();

            var utente = await _userManager.FindByIdAsync(this.CurrentUserId)
                         ?? throw new UserMessageException(InfrastructureMessages.Get("McpNotAllowed"));
            contesto.UserName = utente.UserName ?? string.Empty;
            contesto.Ruoli = (await _userManager.GetRolesAsync(utente)).ToList();

            var massimo = McpOptions.MaxLevel(_configuration);
            if (int.TryParse(this.CurrentCompany, out var idCompany) && idCompany > 0)
            {
                contesto.IdCompany = idCompany;
                contesto.Azienda = await _companyRepository.FindAll(x => x.Id == idCompany).Select(x => x.Descrizione).FirstOrDefaultAsync() ?? string.Empty;
                contesto.Applicativi = _companyApplicationRepository.ActiveApplications(idCompany);

                var setting = await _mcpCompanySettingRepository.OfCompanyAsync(idCompany);
                contesto.AccessoAziendaSpento = !(setting?.Enabled ?? false);
                contesto.Livello = contesto.AccessoAziendaSpento
                    ? McpAccessLevel.Excluded
                    : (McpAccessLevel)Math.Min((int)setting!.MaxLevel, (int)massimo);
            }
            else
            {
                // centro, studio, superuser: nessun dato di azienda, solo i comandi comuni in lettura
                contesto.Livello = McpAccessLevel.Read;
            }

            // utente corrente per gli handler ("@me", limiti per ruolo), come fa ChatAIService
            _chatUserContext.UserId = this.CurrentUserId;
            _chatUserContext.Roles = contesto.Ruoli;
            _chatUserContext.CurrentRoute = string.Empty;
            _chatUserContext.Applications = contesto.Applicativi;
            _chatUserContext.MenuPages = new List<MenuPageModel>();

            foreach (var handler in _handlers)
            {
                var def = handler.IntentDefinition;
                var accesso = def.EffectiveMcpAccess;
                if (accesso == McpAccessLevel.Excluded || (int)accesso > (int)contesto.Livello)
                    continue;
                if (!def.Usable4Role.Contains("*") && !def.Usable4Role.Intersect(contesto.Ruoli).Any())
                    continue;
                if (def.Application != null && !contesto.Applicativi.Contains(def.Application.Value))
                    continue;

                string nome = McpToolNaming.ToolName(def);
                if (contesto.Strumenti.ContainsKey(nome))
                    nome = McpToolNaming.Prefix(def.Application) + "_" + def.Name;   // due comandi con lo stesso nome: resta quello originale
                contesto.Strumenti.TryAdd(nome, handler);
            }

            return contesto;
        }

        private (int? Id, string? Name) TokenCorrente()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            int? id = int.TryParse(user?.FindFirst(ClaimTokenId)?.Value, out var value) ? value : null;
            return (id, user?.FindFirst(ClaimTokenName)?.Value);
        }

        // ---------------------------------------------------------------------------
        // Registro delle chiamate
        // ---------------------------------------------------------------------------

        private async Task RegistraAsync(Contesto? contesto, string name, IReadOnlyDictionary<string, string?> arguments, McpToolCallResult risultato, int durata)
        {
            try
            {
                var token = TokenCorrente();
                var login = string.IsNullOrEmpty(this.UserIdFirstConnection) ? this.CurrentUserId : this.UserIdFirstConnection;
                string? nomeLogin = contesto?.UserName;
                if (login != this.CurrentUserId)
                    nomeLogin = await _aspNetUsersRepository.FindAll(x => x.Id == login).Select(x => x.UserName).FirstOrDefaultAsync();

                await _mcpCallLogRepository.CreateAsync(new McpCallLog
                {
                    Data = DateTime.Now,
                    IdMcpAccessToken = token.Id,
                    TokenName = Tronca(token.Name, 100),
                    IdAspNetUsersLogin = login,
                    UserNameLogin = Tronca(nomeLogin, 256),
                    IdAspNetUsers = this.CurrentUserId,
                    UserName = Tronca(contesto?.UserName, 256),
                    IdCompany = contesto?.IdCompany ?? (int.TryParse(this.CurrentCompany, out var idCompany) && idCompany > 0 ? idCompany : null),
                    Tool = Tronca(name, 100) ?? string.Empty,
                    Arguments = Tronca(arguments.Count > 0 ? JsonSerializer.Serialize(arguments) : null, 4000),
                    Success = risultato.Success,
                    Message = Tronca(risultato.Text, 1000),
                    DurationMs = durata
                });
            }
            catch (Exception ex)
            {
                // il registro non deve far fallire la chiamata gia' eseguita
                Log.Error(ex, "[MCP] Chiamata a {Tool} non registrata", name);
            }
        }

        private static string? Tronca(string? testo, int massimo) =>
            testo == null || testo.Length <= massimo ? testo : testo[..massimo];
    }

    public interface IMcpToolService : IServiceBase
    {
        /// <summary>strumenti che l'utente del token puo' usare</summary>
        Task<List<McpToolDescriptor>> ListToolsAsync();
        /// <summary>esegue uno strumento; gli errori (strumento o parametri non validi) tornano nel risultato, non come eccezione</summary>
        Task<McpToolCallResult> CallToolAsync(string name, IReadOnlyDictionary<string, string?> arguments);
    }

    /*
     Nome dello strumento MCP di un comando (decisione MCP-D5): prefisso del modulo + nome del comando in minuscolo
     con i trattini bassi, senza la sigla del modulo che alcuni comandi hanno gia' nel nome.
        WhoIsPresent (presenze) -> attendance_who_is_present      CrmAgenda (CRM) -> crm_agenda
        HrMyPayslips (HR)       -> hr_my_payslips                 AstBookResource (Assets) -> assets_book_resource
     Il nome fa parte del contratto con i client AI: cambiare il nome di un comando cambia il nome dello strumento.
    */
    public static class McpToolNaming
    {
        public static string Prefix(ApplicationType? application) => application switch
        {
            ApplicationType.AttendanceTracking => "attendance",
            ApplicationType.Crm => "crm",
            ApplicationType.DocumentManagement => "doc",
            ApplicationType.HumanResources => "hr",
            ApplicationType.Compliance => "compliance",
            ApplicationType.Assets => "assets",
            ApplicationType.Moke => "moke",
            _ => "nvx"
        };

        // sigla che i comandi del modulo hanno gia' all'inizio del nome
        private static string? Sigla(ApplicationType? application) => application switch
        {
            ApplicationType.Crm => "Crm",
            ApplicationType.DocumentManagement => "Doc",
            ApplicationType.HumanResources => "Hr",
            ApplicationType.Compliance => "Cmp",
            ApplicationType.Assets => "Ast",
            _ => null
        };

        public static string ToolName(IntentDefinition def)
        {
            string nome = def.Name;
            var sigla = Sigla(def.Application);
            if (sigla != null && nome.Length > sigla.Length && nome.StartsWith(sigla, StringComparison.Ordinal) && char.IsUpper(nome[sigla.Length]))
                nome = nome[sigla.Length..];
            return Prefix(def.Application) + "_" + SnakeCase(nome);
        }

        // "WhoIsPresent" -> "who_is_present"
        public static string SnakeCase(string nome)
        {
            var testo = new StringBuilder(nome.Length + 8);
            for (int i = 0; i < nome.Length; i++)
            {
                char c = nome[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && (char.IsLower(nome[i - 1]) || char.IsDigit(nome[i - 1]) || (i + 1 < nome.Length && char.IsLower(nome[i + 1]))))
                        testo.Append('_');
                    testo.Append(char.ToLowerInvariant(c));
                }
                else if (char.IsLetterOrDigit(c))
                    testo.Append(c);
                else
                    testo.Append('_');
            }
            return testo.ToString();
        }
    }
}
