using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Parti comuni dei comandi presenze della chat: ruoli e accesso ai dati, formati,
    // ricerca di dipendenti e giustificativi, slot ricorrenti (data, orario, mese).
    public static class ChatPresenzeHelper
    {
        // ruoli dell'azienda (RoleCode): l'HR vede e modifica tutto, gli altri solo i propri dati
        public const string RuoloHR = "CompanyPowerAdmin";
        public const string RuoloAdmin = "CompanyAdmin";
        public const string RuoloUser = "User";

        // I servizi non controllano i ruoli: i comandi che leggono i dati di un dipendente
        // lo consentono all'HR o al dipendente stesso.
        public static bool PuoAccedere(IChatUserContext ctx, Dip_AnagraficaModel dipendente) =>
            ctx.IsInRole(RuoloHR) || dipendente.IdAspNetUsers == ctx.UserId;

        public static string Nome(Dip_AnagraficaModel d) => $"{d.Cognome} {d.Nome}".Trim();

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        // minuti -> "H:mm"
        public static string Ore(int minuti) => $"{minuti / 60}:{Math.Abs(minuti % 60):D2}";

        // inizio e fine di un periodo: "dal 3 al 7 novembre" -> il mese vale anche per la data iniziale
        public const string SignificatoDal = "data di INIZIO del periodo; se il mese è indicato solo per la data finale (es. 'dal 3 al 7 novembre') copialo anche qui ('3 novembre')";
        public const string SignificatoAl = "data di FINE del periodo";

        // contatori "HHH:mm:ss" -> "HHH:mm"
        public static string OreContatore(string valore)
        {
            var parti = (valore ?? string.Empty).Split(':');
            return parti.Length >= 2 ? $"{parti[0]}:{parti[1]}" : valore ?? string.Empty;
        }

        // "ottobre 2026" nella lingua della richiesta
        public static string Mese(int anno, int mese) =>
            $"{CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(mese)} {anno}";

        // elenco con al massimo max voci: "A, B, C e altri 4"
        public static string Elenco(IEnumerable<string> voci, int max = 12)
        {
            var lista = voci.ToList();
            if (lista.Count == 0) return AttendanceTrackingMessages.Get("ChatNobody");
            var testo = string.Join(", ", lista.Take(max));
            return lista.Count > max ? $"{testo} {AttendanceTrackingMessages.Get("ChatListMore", lista.Count - max)}" : testo;
        }

        // valore di uno slot data (già normalizzato "dd/MM/yyyy") oppure il suo default ("oggi", "ieri")
        public static DateTime ParseData(string? valore) =>
            SlotNormalizers.TryParseDate(valore, DateTime.Today, out var d) ? d.Date : DateTime.Today;

        // timbrature e giustificativi validi: diretti o approvati (non le richieste in attesa o rifiutate)
        public static bool StatoValido(StatoRichiesta stato) =>
            stato == StatoRichiesta.Diretta || stato == StatoRichiesta.Approvata;

        // ---------------------------------------------------------------------------
        // Dipendenti
        // ---------------------------------------------------------------------------

        public static async Task<List<Dip_AnagraficaModel>> Dipendenti(IDip_AnagraficaService service)
        {
            var res = await service.GetAll(new GenericRequest<Dip_Anagrafica_GetAll_InModel> { Data = new() }, true);
            return res.Success && res.Data != null ? res.Data.Dip_Anagrafica : new();
        }

        // id del rapporto di lavoro -> dipendente
        public static Dictionary<int, Dip_AnagraficaModel> PerRapporto(IEnumerable<Dip_AnagraficaModel> dipendenti) =>
            dipendenti.SelectMany(d => d.Dip_RapportoLavoro.Select(r => (r.Id, Dipendente: d)))
                      .GroupBy(x => x.Id)
                      .ToDictionary(g => g.Key, g => g.First().Dipendente);

        // valore dello slot dipendente: nome canonico (già validato) oppure ChatSelf.Token
        public static async Task<Dip_AnagraficaModel?> RisolviDipendente(string? valore, IDip_AnagraficaService service, IChatUserContext ctx)
        {
            if (string.IsNullOrWhiteSpace(valore)) return null;
            if (valore.Trim() == ChatSelf.Token)
                return (await Dipendenti(service)).FirstOrDefault(d => d.IdAspNetUsers == ctx.UserId);
            return DipendenteLookupHelper.GetDipendente(valore, service);
        }

        // copia di uno slot dipendente, facoltativo con il default indicato (es. ChatSelf.Token: "i miei dati")
        public static SlotDefinition Facoltativo(SlotDefinition slot, string defaultValue) => new()
        {
            Name = slot.Name,
            Type = slot.Type,
            Required = false,
            Default = defaultValue,
            PromptDescription = slot.PromptDescription,
            Question = slot.Question,
            Label = slot.Label,
            Validator = slot.Validator,
            HasRelevantContent = slot.HasRelevantContent,
            Normalizer = slot.Normalizer,
            IsPerson = slot.IsPerson,
            FindPersonInText = slot.FindPersonInText
        };

        // ---------------------------------------------------------------------------
        // Giustificativi
        // ---------------------------------------------------------------------------

        public static async Task<List<Par_GiustificativiModel>> Giustificativi(IPar_GiustificativiService service)
        {
            var res = await service.GetAll(new GenericRequest<Par_GiustificativiInModel> { Data = new() }, false);
            return res.Success && res.Data != null ? res.Data.Par_Giustificativi : new();
        }

        // per descrizione o codice: prima uguale, poi contenuto
        public static Par_GiustificativiModel? TrovaGiustificativo(IEnumerable<Par_GiustificativiModel> giustificativi, string nome)
        {
            var n = (nome ?? string.Empty).Trim();
            if (n.Length == 0) return null;
            var lista = giustificativi.ToList();
            return lista.FirstOrDefault(g => string.Equals(g.Descrizione, n, StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(g.Codice, n, StringComparison.OrdinalIgnoreCase))
                ?? lista.FirstOrDefault(g => (g.Descrizione?.Contains(n, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                             (g.Codice?.Contains(n, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // ---------------------------------------------------------------------------
        // Slot ricorrenti
        // ---------------------------------------------------------------------------

        // data: il LLM copia l'espressione, la converte SlotNormalizers.Date (tipo "dd/MM/yyyy")
        public static SlotDefinition SlotData(string name, bool required, string defaultValue, string question, string label, string? significato = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(significato)) sb.Append(significato).Append(". ");
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb);
            return new SlotDefinition
            {
                Name = name,
                Type = SlotNormalizers.DateFormat,
                Required = required,
                Default = defaultValue,
                PromptDescription = sb.ToString(),
                Question = question,
                Label = label,
                Validator = v => DateOnly.TryParseExact(v, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatInvalidDate", v), name),
                HasRelevantContent = ChatSlotWords.HasDateContent
            };
        }

        // orario: il LLM copia l'espressione, la converte SlotNormalizers.Time (tipo "HH:mm")
        public static SlotDefinition SlotOrario(string name, bool required, string question, string label, string? significato = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(significato)) sb.Append(significato).Append(". ");
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Orario(sb);
            return new SlotDefinition
            {
                Name = name,
                Type = SlotNormalizers.TimeFormat,
                Required = required,
                PromptDescription = sb.ToString(),
                Question = question,
                Label = label,
                Validator = v => TimeOnly.TryParseExact(v, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatInvalidTime", v), name)
            };
        }

        // mese: "ottobre", "ottobre 2025", "10/2025", "questo mese", "il mese scorso" -> "MM/yyyy"
        public static SlotDefinition SlotMese(string name, string question, string label) => new()
        {
            Name = name,
            Type = "MM/yyyy",
            Required = false,
            PromptDescription = "mese come detto dall'utente (es. 'ottobre', 'ottobre 2025', '10/2025', 'questo mese', 'il mese scorso'). Non convertirlo: lo converte il sistema.",
            Question = question,
            Label = label,
            Normalizer = NormalizzaMese,
            Validator = v => MonthSlotHelper.TryParse(v, out _)
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, question, name)
        };

        public static string? NormalizzaMese(string valore)
        {
            var t = (valore ?? string.Empty).Trim().ToLowerInvariant();
            foreach (var p in new[] { "il mese di ", "mese di ", "il ", "di ", "a ", "in " })
                if (t.StartsWith(p)) t = t[p.Length..];

            var oggi = DateTime.Today;
            DateTime? mese = t switch
            {
                "questo mese" or "mese corrente" or "this month" or "current month" => oggi,
                "mese scorso" or "il mese scorso" or "scorso mese" or "last month" or "previous month" => oggi.AddMonths(-1),
                "mese prossimo" or "prossimo mese" or "next month" => oggi.AddMonths(1),
                _ => null
            };
            if (mese != null) return mese.Value.ToString("MM/yyyy", CultureInfo.InvariantCulture);

            return MonthSlotHelper.TryParse(t, out var m) ? $"{m.Month:D2}/{m.Year}" : null;
        }

        // mese dello slot ("MM/yyyy"), altrimenti il mese corrente
        public static (int Anno, int Mese) ParseMese(string? valore) =>
            !string.IsNullOrEmpty(valore) && MonthSlotHelper.TryParse(valore, out var m)
                ? (m.Year, m.Month)
                : (DateTime.Today.Year, DateTime.Today.Month);
    }
}
