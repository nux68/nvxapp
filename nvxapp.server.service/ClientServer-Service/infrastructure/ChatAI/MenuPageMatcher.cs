using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Trova le voci del menu dell'utente (ChatAIInModel.MenuPages, titoli gia' tradotti dal client) che corrispondono
    // al nome di pagina detto o scritto: "vai a tabelle" -> voce "Tabelle" (CRM) -> /crmparlist.
    // Il confronto ignora maiuscole, accenti, articoli e preposizioni, singolare/plurale ("tabella" = "tabelle").
    public static class MenuPageMatcher
    {
        // parole che non distinguono una pagina ("vai alla pagina delle tabelle")
        private static readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "il", "lo", "la", "i", "gli", "le", "l", "un", "uno", "una",
            "di", "del", "dello", "della", "dei", "degli", "delle",
            "a", "al", "allo", "alla", "ai", "agli", "alle", "in", "nel", "nella", "nei", "nelle",
            "pagina", "pagine", "menu", "voce", "sezione",
            "the", "of", "to", "page", "pages", "section"
        };

        // Voci con la corrispondenza piu' stretta, una per pagina: vuoto = nessuna, piu' di una = nome ambiguo.
        // Livelli: 0 titolo uguale (o titolo + sezione, "tabelle crm"); 1 stesse parole in altro ordine, anche con la
        // sezione; 2 parte del titolo ("ferie" -> "Piano ferie").
        public static IReadOnlyList<MenuPageModel> Match(string? page, IReadOnlyList<MenuPageModel>? menuPages)
        {
            if (menuPages == null || menuPages.Count == 0) return [];
            var said = Words(page);
            if (said.Length == 0) return [];
            var saidKey = string.Join(' ', said);

            var byLevel = new[] { new List<MenuPageModel>(), new List<MenuPageModel>(), new List<MenuPageModel>() };
            foreach (var menuPage in menuPages)
            {
                if (string.IsNullOrWhiteSpace(menuPage.Route)) continue;
                var title = Words(menuPage.Title);
                if (title.Length == 0) continue;
                var section = Words(menuPage.Section);

                int level;
                if (string.Join(' ', title) == saidKey ||
                    string.Join(' ', title.Concat(section)) == saidKey ||
                    string.Join(' ', section.Concat(title)) == saidKey)
                    level = 0;
                else if (title.All(said.Contains) && said.All(w => title.Contains(w) || section.Contains(w)))
                    level = 1;
                else if (said.All(title.Contains))
                    level = 2;
                else
                    continue;

                if (!byLevel[level].Any(m => SameRoute(m.Route, menuPage.Route)))
                    byLevel[level].Add(menuPage);
            }

            return byLevel.FirstOrDefault(l => l.Count > 0) ?? [];
        }

        // stessa pagina: confronto senza "/" iniziale/finale, parametri e maiuscole ("crmparlist" = "/crmparlist")
        public static bool SameRoute(string? a, string? b) =>
            NormalizeRoute(a).Length > 0 && NormalizeRoute(a) == NormalizeRoute(b);

        // nome della voce per i messaggi: "Tabelle (CRM)"
        public static string Label(MenuPageModel menuPage) =>
            string.IsNullOrWhiteSpace(menuPage.Section) ? menuPage.Title.Trim() : $"{menuPage.Title.Trim()} ({menuPage.Section.Trim()})";

        private static string NormalizeRoute(string? route) =>
            (route ?? string.Empty).Split('?', '#')[0].Trim().Trim('/').ToLowerInvariant();

        // parole significative, minuscole, senza accenti, ridotte alla radice (ultima vocale o "s" del plurale inglese)
        private static string[] Words(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return [];
            var sb = new StringBuilder();
            foreach (var c in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            return sb.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w))
                .Select(Stem)
                .ToArray();
        }

        private static string Stem(string word) =>
            word.Length > 3 && "aeious".Contains(word[^1]) ? word[..^1] : word;
    }
}
