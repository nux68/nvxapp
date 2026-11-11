using System.Reflection;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /// <summary>Voce della "Lista metodi" delle formule.</summary>
    public class FormulaMetodo
    {
        public string Categoria { get; set; } = string.Empty;   // "x", "Causale", "Timbrate", ...
        public string Tipo { get; set; } = string.Empty;        // "Metodo", "Proprieta'", "Oggetto"
        public string Testo { get; set; } = string.Empty;       // testo da inserire dopo "x." (es. "Causale.GetMin(codice)")
        public string Descrizione { get; set; } = string.Empty;
        public bool InEvidenza { get; set; }
    }

    /// <summary>
    /// Catalogo dei membri della facciata "x", ricavato per reflection dai membri [FormulaApi]
    /// (sostituisce la reflection su Formule_DEF del legacy con i summary XML e [inevidenza]).
    /// </summary>
    public static class FormuleCatalogo
    {
        private static readonly Lazy<List<FormulaMetodo>> _metodi = new(Costruisci);

        public static List<FormulaMetodo> Metodi => _metodi.Value;

        private static IEnumerable<MemberInfo> MembriApi(Type tipo)
            => tipo.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                   .Where(m => m.GetCustomAttribute<FormulaApiAttribute>() != null);

        /// <summary>Esiste il membro (con [FormulaApi]) nel tipo della facciata.</summary>
        public static bool EsisteMembro(Type tipo, string nome) => MembriApi(tipo).Any(m => m.Name == nome);

        /// <summary>Tipo della proprieta' "nome" di un oggetto della facciata (null se non e' una proprieta' oggetto).</summary>
        public static Type? TipoMembro(Type tipo, string nome)
            => MembriApi(tipo).OfType<PropertyInfo>().FirstOrDefault(p => p.Name == nome && IsOggetto(p.PropertyType))?.PropertyType;

        private static bool IsOggetto(Type t) => t.IsClass && t != typeof(string) && !t.IsArray && t.Namespace == typeof(FormulaContext).Namespace;

        private static string Firma(MemberInfo m, FormulaApiAttribute api)
        {
            if (!string.IsNullOrWhiteSpace(api.Firma)) return api.Firma!;
            if (m is MethodInfo mi) return $"{mi.Name}({string.Join(", ", mi.GetParameters().Select(p => p.Name))})";
            return m.Name;
        }

        private static List<FormulaMetodo> Costruisci()
        {
            var lista = new List<FormulaMetodo>();
            foreach (var m in MembriApi(typeof(FormulaContext)))
            {
                var api = m.GetCustomAttribute<FormulaApiAttribute>()!;
                if (m is PropertyInfo p && IsOggetto(p.PropertyType))
                {
                    lista.Add(new FormulaMetodo { Categoria = p.Name, Tipo = "Oggetto", Testo = p.Name, Descrizione = api.Descrizione, InEvidenza = false });
                    foreach (var sm in MembriApi(p.PropertyType))
                    {
                        var sapi = sm.GetCustomAttribute<FormulaApiAttribute>()!;
                        lista.Add(new FormulaMetodo
                        {
                            Categoria = p.Name,
                            Tipo = sm is MethodInfo ? "Metodo" : "Proprieta'",
                            Testo = p.Name + "." + Firma(sm, sapi),
                            Descrizione = sapi.Descrizione,
                            InEvidenza = sapi.InEvidenza
                        });
                    }
                }
                else
                {
                    lista.Add(new FormulaMetodo
                    {
                        Categoria = "x",
                        Tipo = m is MethodInfo ? "Metodo" : "Proprieta'",
                        Testo = Firma(m, api),
                        Descrizione = api.Descrizione,
                        InEvidenza = api.InEvidenza
                    });
                }
            }
            // i metodi con overload (es. Sposta) compaiono una volta per firma
            return lista.GroupBy(x => x.Testo).Select(g => g.First()).ToList();
        }

        /// <summary>Catalogo in testo, per il prompt dell'AI.</summary>
        public static string ComeTesto()
        {
            var sb = new StringBuilder();
            foreach (var g in Metodi.GroupBy(m => m.Categoria))
            {
                foreach (var m in g)
                    sb.Append("x.").Append(m.Testo).Append("  // ").Append(m.Descrizione).Append('\n');
                sb.Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>Testo iniziale di una nuova formula.</summary>
        public const string Template =
@"// Controllo applicabilita' della formula
// con x.Attiva.Sempre(true) le altre scelte vengono ignorate
x.Attiva.Sempre(true);
// x.Attiva.Lun(true); x.Attiva.Mar(true); ... x.Attiva.Dom(true);
// x.Attiva.Mese(1, true); ... x.Attiva.Festivo(true); x.Attiva.NonFestivo(true);
if (!x.Attiva.ApplicaLaFormula()) return;

// inizio formula (durate in minuti, causali e giustificativi per codice)
// esempio (sostituisci ORDI e STR1 con i codici delle tue causali): le ore ORDI oltre le 8 vanno su STR1
// x.Causale.TogliEccedenza(""ORDI"", x.Time.Min(8, 0), ""STR1"");
";
    }
}
