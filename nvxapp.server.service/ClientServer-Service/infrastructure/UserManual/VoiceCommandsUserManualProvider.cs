using Microsoft.Extensions.DependencyInjection;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual
{
    /*
     Manuale dei comandi vocali dell'assistente (url /usermanual/voicecommands, voce "Comandi vocali" in Assistenza).

     Non ha testi per funzione: si compone dai comandi REGISTRATI nell'applicazione (gli ICommandHandler della chat AI,
     ognuno con la sua IntentDefinition). Per questo segue il ramo: nel ramo presenze ci sono solo i comandi delle funzioni
     base e delle presenze, in AllFunction tutti. In piu' restano solo i comandi dei ruoli dell'utente e degli applicativi
     attivi per la sua azienda (stesse regole della chat, vedi ChatAIService).

     Organizzazione: un capitolo per applicativo (prima gli applicativi, poi le funzioni base, come nell'help della chat),
     una sezione per argomento (IntentDefinition.Category/CategoryOrder), per ogni comando la funzione, le frasi di
     esempio e le parole riconosciute; in fondo l'indice alfabetico dei comandi.
     Introduzione e appendice: Manuals/voicecommands.<lingua>.md (stessa sintassi degli altri manuali, senza blocchi @@).
    */
    public class VoiceCommandsUserManualProvider : IUserManualProvider
    {
        public const string ManualCode = "voicecommands";

        private readonly IServiceProvider _serviceProvider;

        public VoiceCommandsUserManualProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string Code => ManualCode;

        // comandi delle funzioni base sempre presenti; quelli degli applicativi filtrati in Build
        public ApplicationType? Application => null;

        // senza gli applicativi attivi: solo i comandi comuni
        public UserManualDocument Build(IReadOnlyCollection<string> ruoli, IReadOnlyList<UserManualMenuSection> menu)
            => Build(ruoli, menu, Array.Empty<ApplicationType>());

        public UserManualDocument Build(IReadOnlyCollection<string> ruoli, IReadOnlyList<UserManualMenuSection> menu, IReadOnlyCollection<ApplicationType> applicativiAttivi)
        {
            var lingua = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var assembly = GetType().Assembly;
            var testo = UserManualMarkdownProvider.LeggiRisorsa(assembly, $".Manuals.{Code}.{lingua}.md")
                        ?? UserManualMarkdownProvider.LeggiRisorsa(assembly, $".Manuals.{Code}.it.md")
                        ?? throw new InvalidOperationException($"Manuale {Code} mancante (Manuals/{Code}.it.md)");
            var parti = UserManualMarkdown.Parse(testo, ruoli);

            var doc = new UserManualDocument { Title = parti.Title, Subtitle = parti.Subtitle };
            doc.Blocks.AddRange(parti.Introduzione);

            var comandi = Comandi(ruoli, applicativiAttivi);
            if (comandi.Count == 0)
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Note, Text = InfrastructureMessages.Get("VoiceManualNoCommands") });

            // capitoli per applicativo: prima gli applicativi (ordine dell'enum), in fondo i comandi comuni
            var indice = new List<string[]>();
            var perApplicativo = comandi.GroupBy(c => c.Application)
                                        .OrderBy(g => g.Key == null ? int.MaxValue : (int)g.Key.Value);
            foreach (var applicativo in perApplicativo)
            {
                var nomeApplicativo = NomeApplicativo(applicativo.Key);
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Chapter, Text = nomeApplicativo });

                // argomenti nell'ordine dell'help della chat (CategoryOrder), senza argomento in fondo
                var altro = ChatAIMessages.Get("HelpOtherCategory");
                var argomenti = applicativo
                    .GroupBy(c => string.IsNullOrWhiteSpace(c.Category) ? altro : c.Category.Trim())
                    .OrderBy(g => g.Min(c => string.IsNullOrWhiteSpace(c.Category) ? int.MaxValue : c.CategoryOrder))
                    .ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
                doc.Blocks.Add(new UserManualBlock
                {
                    Type = UserManualBlockType.Paragraph,
                    Text = InfrastructureMessages.Get("VoiceManualAppIntro", nomeApplicativo, string.Join(", ", argomenti.Select(a => a.Key)))
                });

                foreach (var argomento in argomenti)
                {
                    doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Section, Text = argomento.Key });
                    foreach (var comando in argomento)
                    {
                        var nome = Nome(comando);
                        doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Subsection, Text = nome });
                        var funzione = Funzione(comando);
                        if (funzione.Length > 0)
                            doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = funzione });

                        var esempi = comando.Examples.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim())
                                                     .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        if (esempi.Count > 0)
                        {
                            doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = "**" + InfrastructureMessages.Get("VoiceManualExamples") + "**" });
                            foreach (var esempio in esempi)
                                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Bullet, Text = "“" + esempio + "”" });
                        }

                        var parole = comando.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim())
                                                     .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        if (parole.Count > 0)
                            doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Path, Text = InfrastructureMessages.Get("VoiceManualKeywords", string.Join(", ", parole)) });

                        if (comando.NeedsConfirmation)
                            doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Note, Text = InfrastructureMessages.Get("VoiceManualConfirmation") });

                        indice.Add(new[] { nome, funzione, nomeApplicativo + " › " + argomento.Key });
                    }
                }
            }

            doc.Blocks.AddRange(parti.Appendice);

            // indice alfabetico dei comandi, con applicativo e argomento
            if (indice.Count > 0)
            {
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Chapter, Text = InfrastructureMessages.Get("VoiceManualIndex") });
                var righe = new List<string[]>
                {
                    new[] { InfrastructureMessages.Get("VoiceManualIndexCommand"), InfrastructureMessages.Get("VoiceManualIndexFunction"), InfrastructureMessages.Get("VoiceManualIndexWhere") }
                };
                righe.AddRange(indice.OrderBy(r => r[0], StringComparer.CurrentCultureIgnoreCase));
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Table, Rows = righe });
            }
            return doc;
        }

        // comandi registrati nel ramo (uno per handler), per i ruoli dell'utente e gli applicativi attivi
        private List<IntentDefinition> Comandi(IReadOnlyCollection<string> ruoli, IReadOnlyCollection<ApplicationType> applicativiAttivi)
        {
            // risolti qui (non nel costruttore): gli handler sono molti e servono solo per questo manuale
            var handlers = _serviceProvider.GetServices<ICommandHandler>();
            return handlers.Select(h => h.IntentDefinition)
                           .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Name))
                           .Where(i => i.Usable4Role.Contains("*") || i.Usable4Role.Any(r => ruoli.Contains(r, StringComparer.OrdinalIgnoreCase)))
                           .Where(i => i.Application == null || applicativiAttivi.Contains(i.Application.Value))
                           .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
                           .ToList();
        }

        private static string Nome(IntentDefinition comando)
            => string.IsNullOrWhiteSpace(comando.DisplayName) ? comando.Name : comando.DisplayName.Trim();

        // funzione del comando (descrizione dell'intent) con l'iniziale maiuscola: nell'help segue il nome ("Aiuto — fornisce...")
        private static string Funzione(IntentDefinition comando)
        {
            var d = comando.Description?.Trim() ?? string.Empty;
            return d.Length == 0 ? d : char.ToUpper(d[0], CultureInfo.CurrentCulture) + d[1..];
        }

        // nome dell'applicativo nella lingua corrente (VoiceManualApp_<n>, 0 = funzioni base)
        private static string NomeApplicativo(ApplicationType? applicativo)
        {
            var codice = (int)(applicativo ?? ApplicationType.Common);
            var chiave = "VoiceManualApp_" + codice;
            var nome = InfrastructureMessages.Get(chiave);
            return string.IsNullOrWhiteSpace(nome) || nome == chiave ? (applicativo?.ToString() ?? chiave) : nome;
        }
    }
}
