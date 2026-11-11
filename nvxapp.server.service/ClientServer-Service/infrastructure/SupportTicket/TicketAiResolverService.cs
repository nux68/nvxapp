using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Text;
using System.Text.Json;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Risposta automatica AI ai ticket dalla documentazione del livello (ANALISI_DOCUMENTMANAGEMENT_DOCUMENTAZIONE_AI.md [branch DocumentManagement] §7).
     TicketService la chiama in background quando il ticket arriva a un livello che ha l'AI attiva e dei documenti.
       - provider AI:TicketLLM (vuoto = AI:UseLLM), risposta lunga al massimo AI:TicketMaxTokens
       - documenti nell'ordine ricevuto (prima quelli del tipo, poi il nodo, poi i livelli sopra) fino a
         AI:TicketMaxChars caratteri: l'ultimo che non ci sta si tronca, gli altri restano fuori
       - l'AI risponde solo dai documenti; se non trova la risposta lo dichiara (Resolved = false)
     Gli errori del provider (timeout, limiti) arrivano a chi chiama, che li registra e lascia il ticket all'operatore.
    */
    public class TicketAiResolverService : ServiceBase, ITicketAiResolverService
    {
        private const int DefaultMaxChars = 60000;

        private readonly ILlmClient _llmClient;

        public TicketAiResolverService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration,

                                       ILlmClient llmClient) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _llmClient = llmClient;
        }

        public virtual async Task<GenericResult<TicketAiResolveOutModel>> TicketAiResolve(GenericRequest<TicketAiResolveInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketAiResolveOutModel { Resolved = false };
                var data = model.Data;

                var documents = Budget(data.Document, int.TryParse(_configuration["AI:TicketMaxChars"], out int max) && max > 0 ? max : DefaultMaxChars);
                if (documents.Count == 0)
                    return retVal;

                var provider = _configuration["AI:TicketLLM"];
                int? maxTokens = int.TryParse(_configuration["AI:TicketMaxTokens"], out int tokens) && tokens > 0 ? tokens : null;

                var risposta = await _llmClient.CompleteJsonAsync(SystemPrompt, UserMessage(data, documents), Schema, "risposta_ticket",
                                                                  provider: string.IsNullOrWhiteSpace(provider) ? null : provider.Trim(),
                                                                  maxTokens: maxTokens);

                var letta = Leggi(risposta);
                var ids = documents.Select(d => d.Id).ToHashSet();
                retVal.Resolved = letta.Resolved && !string.IsNullOrWhiteSpace(letta.Answer);
                retVal.Text = string.IsNullOrWhiteSpace(letta.Answer) ? null : Taglia(letta.Answer.Trim(), TicketRules.TextMaxLength);
                retVal.UsedDocumentIds = letta.UsedDocumentIds.Where(ids.Contains).Distinct().ToList();

                return retVal;
            }, isSubProcess);
        }

        private const string SystemPrompt =
            "Sei l'assistenza di un'applicazione gestionale (presenze, paghe, gestione aziende). " +
            "Rispondi alla richiesta di assistenza di un utente usando SOLO le informazioni dei documenti forniti.\n" +
            "Regole:\n" +
            "- se i documenti contengono la risposta, scrivila in modo chiaro e pratico (passi da seguire, dove cliccare) e metti resolved = true;\n" +
            "- se i documenti non contengono la risposta, o la richiesta richiede un intervento umano (dati da correggere, " +
            "autorizzazioni, errori del sistema), metti resolved = false e in answer spiega in una frase cosa manca; non inventare;\n" +
            "- se ci sono risposte automatiche precedenti con l'obiezione dell'utente, tienine conto e non ripetere la stessa risposta;\n" +
            "- rispondi nella stessa lingua in cui e' scritta la richiesta;\n" +
            "- non citare questi documenti come \"documento 1\": se serve, usa il loro titolo;\n" +
            "- in used_document_ids metti gli id dei documenti su cui si basa la risposta.\n" +
            "Rispondi solo con il JSON richiesto.";

        private static readonly object Schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["resolved"] = new Dictionary<string, object> { ["type"] = "boolean" },
                ["answer"] = new Dictionary<string, object> { ["type"] = "string" },
                ["used_document_ids"] = new Dictionary<string, object>
                {
                    ["type"] = "array",
                    ["items"] = new Dictionary<string, object> { ["type"] = "integer" }
                },
            },
            ["required"] = new[] { "resolved", "answer", "used_document_ids" },
            ["additionalProperties"] = false
        };

        private static string UserMessage(TicketAiResolveInModel data, List<TicketAiDocumentModel> documents)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Richiesta di assistenza");
            sb.AppendLine($"Tipo: {data.TipoDescrizione}");
            sb.AppendLine($"Oggetto: {data.Oggetto}");
            sb.AppendLine("Testo:");
            sb.AppendLine(data.Text);

            if (data.History.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# Passaggi precedenti");
                foreach (var h in data.History)
                {
                    sb.AppendLine(h.FromAi ? "Risposta automatica:" : "Obiezione dell'utente (ha riaperto la richiesta):");
                    sb.AppendLine(h.Text);
                }
            }

            sb.AppendLine();
            sb.AppendLine("# Documenti");
            foreach (var d in documents)
            {
                sb.AppendLine($"## [id {d.Id}] {d.Titolo}");
                if (!string.IsNullOrWhiteSpace(d.Descrizione))
                    sb.AppendLine($"Descrizione: {d.Descrizione}");
                sb.AppendLine(d.Contenuto);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // documenti nell'ordine ricevuto fino al testo massimo; l'ultimo che non ci sta per intero si tronca
        private static List<TicketAiDocumentModel> Budget(List<TicketAiDocumentModel> documents, int maxChars)
        {
            var retVal = new List<TicketAiDocumentModel>();
            int usati = 0;
            foreach (var d in documents)
            {
                int resto = maxChars - usati;
                if (resto < 500)
                    break;
                var contenuto = d.Contenuto.Length <= resto ? d.Contenuto : d.Contenuto.Substring(0, resto) + "\n[...]";
                retVal.Add(new TicketAiDocumentModel { Id = d.Id, Titolo = d.Titolo, Descrizione = d.Descrizione, Contenuto = contenuto });
                usati += contenuto.Length;
            }
            return retVal;
        }

        private record Letta(bool Resolved, string? Answer, List<int> UsedDocumentIds);

        // risposta del LLM: tollera testo prima o dopo il JSON e tipi diversi (es. "true" come stringa)
        private static Letta Leggi(string? raw)
        {
            var vuota = new Letta(false, null, new List<int>());
            if (string.IsNullOrEmpty(raw))
                return vuota;

            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start < 0 || end <= start)
                throw new InvalidOperationException("Risposta AI non valida: JSON mancante");

            using var doc = JsonDocument.Parse(raw.Substring(start, end - start + 1));
            var root = doc.RootElement;

            bool resolved = root.TryGetProperty("resolved", out var r)
                            && (r.ValueKind == JsonValueKind.True || (r.ValueKind == JsonValueKind.String && bool.TryParse(r.GetString(), out var b) && b));
            string? answer = root.TryGetProperty("answer", out var a) && a.ValueKind == JsonValueKind.String ? a.GetString() : null;

            var ids = new List<int>();
            if (root.TryGetProperty("used_document_ids", out var u) && u.ValueKind == JsonValueKind.Array)
                foreach (var e in u.EnumerateArray())
                    if ((e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out int id)) || (e.ValueKind == JsonValueKind.String && int.TryParse(e.GetString(), out id)))
                        ids.Add(id);

            return new Letta(resolved, answer, ids);
        }

        private static string Taglia(string testo, int max) => testo.Length <= max ? testo : testo.Substring(0, max - 3) + "...";
    }

    public interface ITicketAiResolverService : IServiceBase
    {
        Task<GenericResult<TicketAiResolveOutModel>> TicketAiResolve(GenericRequest<TicketAiResolveInModel> model, Boolean isSubProcess);
    }
}
