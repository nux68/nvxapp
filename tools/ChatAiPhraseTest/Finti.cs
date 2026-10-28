using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

// Servizi e dati finti per la prova delle frasi. Il programma compila su ogni ramo: i modelli dei moduli
// (es. anagrafica presenze) si riconoscono per nome di tipo e si valorizzano via reflection.
static class Finti
{
    public const string UtenteCorrente = "u-1";

    // dipendenti dell'anagrafica finta: (cognome, nome, utente, rapporto di lavoro); u-1 = utente corrente
    public static readonly (string Cognome, string Nome, string Utente, int Rapporto)[] Dipendenti =
    {
        ("Rossi", "Mario", "u-2", 10), ("Bianchi", "Luca", "u-3", 11), ("Pugliese", "Antonio", "u-1", 12), ("Verdi", "Giuseppe", "u-4", 13)
    };

    // catalogo della prova in corso (per l'help, che lo legge dal service provider)
    public static IIntentCatalog? Catalogo;

    // tutti i comandi dell'assembly dei servizi, con i servizi finti
    public static List<ICommandHandler> Comandi(IChatUserContext ctx)
    {
        object? Risolvi(Type t) =>
            t == typeof(IChatUserContext) ? ctx
            : t == typeof(IServiceProvider) ? new ProviderFinto(ctx)
            : t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? Tutti(t.GetGenericArguments()[0], Risolvi)
            : t.IsInterface ? DispatchProxy.Create(t, typeof(ServizioFinto))
            : null;

        return Assembly.GetAssembly(typeof(ChatAIService))!.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(t))
            .Select(t => (ICommandHandler)Costruisci(t, Risolvi))
            .ToList();
    }

    // istanza di T con il primo costruttore, parametri da resolve (null se non serve)
    public static T Crea<T>(Func<Type, object?> resolve) => (T)Costruisci(typeof(T), resolve);

    public static object Opzioni(Type t)
    {
        var create = typeof(Options).GetMethod(nameof(Options.Create))!.MakeGenericMethod(t);
        return create.Invoke(null, new[] { Activator.CreateInstance(t) })!;
    }

    private static object Costruisci(Type t, Func<Type, object?> resolve)
    {
        var ctor = t.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        return ctor.Invoke(ctor.GetParameters().Select(p => resolve(p.ParameterType)).ToArray());
    }

    // IEnumerable<X>: tutte le implementazioni di X nell'assembly dei servizi
    private static object Tutti(Type x, Func<Type, object?> resolve)
    {
        var lista = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(x))!;
        foreach (var t in Assembly.GetAssembly(typeof(ChatAIService))!.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && x.IsAssignableFrom(t)))
            lista.Add(Costruisci(t, resolve));
        return lista;
    }

    // Dati dei servizi finti per tipo restituito (nome del tipo); gli altri tipi restano vuoti
    public static object? Dati(Type tipo)
    {
        if (tipo.Name == "Dip_Anagrafica_GetAll_OutModel")
        {
            var dati = Activator.CreateInstance(tipo)!;
            var lista = (IList)tipo.GetProperty("Dip_Anagrafica")!.GetValue(dati)!;
            var modello = lista.GetType().GetGenericArguments()[0];
            foreach (var d in Dipendenti)
            {
                var dip = Activator.CreateInstance(modello)!;
                modello.GetProperty("Cognome")!.SetValue(dip, d.Cognome);
                modello.GetProperty("Nome")!.SetValue(dip, d.Nome);
                modello.GetProperty("IdAspNetUsers")!.SetValue(dip, d.Utente);
                var rapporti = (IList)modello.GetProperty("Dip_RapportoLavoro")!.GetValue(dip)!;
                var rapporto = Activator.CreateInstance(rapporti.GetType().GetGenericArguments()[0])!;
                rapporto.GetType().GetProperty("Id")!.SetValue(rapporto, d.Rapporto);
                rapporti.Add(rapporto);
                lista.Add(dip);
            }
            return dati;
        }

        // richieste in attesa: una timbratura di Rossi (l'approvazione elenca le richieste nel riepilogo)
        if (tipo.Name == "Dip_GG_Richiesta_GetAll4Admin_OutModel")
        {
            var dati = Activator.CreateInstance(tipo)!;
            var lista = (IList)tipo.GetProperty("Dip_GG_Richiesta")!.GetValue(dati)!;
            var modello = lista.GetType().GetGenericArguments()[0];
            var richiesta = Activator.CreateInstance(modello)!;
            void Imposta(string nome, object valore)
            {
                var p = modello.GetProperty(nome)!;
                p.SetValue(richiesta, p.PropertyType.IsEnum ? Enum.Parse(p.PropertyType, (string)valore) : valore);
            }
            Imposta("Id", 1);
            Imposta("IdDip_RapportoLavoro", Dipendenti.First(d => d.Cognome == "Rossi").Rapporto);
            Imposta("RichiestaTipo", "Timbratura");
            Imposta("RichiestaStato", "Immessa");
            Imposta("Data", DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy"));
            Imposta("DataA", DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy"));
            Imposta("Dati", "{\"hhmm\":\"09:00\",\"idAz_SubCommessaAttivita\":1}");
            lista.Add(richiesta);
            return dati;
        }
        return null;
    }
}

// Proxy dei servizi: i metodi Task<GenericResult<T>> rispondono con i dati finti di T (o T vuoto)
public class ServizioFinto : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        var rt = method!.ReturnType;
        if (rt.IsGenericType && rt.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var risultato = rt.GetGenericArguments()[0];
            if (risultato.IsGenericType && risultato.GetGenericTypeDefinition() == typeof(GenericResult<>))
            {
                var tipo = risultato.GetGenericArguments()[0];
                var dati = Finti.Dati(tipo) ?? Activator.CreateInstance(tipo);
                var generic = Activator.CreateInstance(risultato, dati, null);
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(risultato).Invoke(null, new[] { generic });
            }
        }
        return rt.IsValueType ? Activator.CreateInstance(rt) : null;
    }
}

// Service provider per i comandi che risolvono servizi in modo lazy (es. help)
class ProviderFinto : IServiceProvider
{
    private readonly IChatUserContext _ctx;
    public ProviderFinto(IChatUserContext ctx) => _ctx = ctx;
    public object? GetService(Type t) =>
        t == typeof(IIntentCatalog) ? Finti.Catalogo
        : t == typeof(IChatUserContext) ? _ctx
        : null;
}
