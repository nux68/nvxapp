using System.Globalization;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Normalizzatori deterministici dei valori degli slot.
    // Il LLM copia l'espressione dell'utente ("lunedì scorso", "il 15 maggio", "alle 9 e mezza")
    // e la conversione la fa il codice, con regole prevedibili e indipendenti dalla lingua dell'interfaccia.
    // Come le parole chiave, riconoscono insieme italiano e inglese.
    public static class SlotNormalizers
    {
        public const string DateFormat = "dd/MM/yyyy";
        public const string TimeFormat = "HH:mm";

        // Normalizzatore associato al tipo dello slot (SlotDefinition.Type), null se il tipo non ne ha.
        public static Func<string, string?>? ForType(string? slotType) => slotType switch
        {
            DateFormat => Date,
            TimeFormat => Time,
            _ => null
        };

        // espressione di data -> "dd/MM/yyyy" (null se non riconosciuta); un periodo ("ottobre", "la prima settimana
        // di ottobre") vale il primo giorno
        public static string? Date(string value) =>
            TryParseDate(value, DateTime.Today, out var d) ? d.ToString(DateFormat, CultureInfo.InvariantCulture) : null;

        // fine di un periodo: un mese o una settimana ("fino a ottobre") vale l'ultimo giorno, il resto come Date
        public static string? DateEnd(string value) =>
            TryParsePeriod(value, DateTime.Today, out _, out var end) ? end.ToString(DateFormat, CultureInfo.InvariantCulture) : Date(value);

        // espressione di orario -> "HH:mm" (null se non riconosciuta)
        public static string? Time(string value) =>
            TryParseTime(value, DateTime.Now, out var t) ? t.ToString(TimeFormat, CultureInfo.InvariantCulture) : null;

        // ---------------------------------------------------------------------------
        // Date
        // ---------------------------------------------------------------------------

        private static readonly Dictionary<string, DayOfWeek> _weekdays = new()
        {
            ["lunedì"] = DayOfWeek.Monday, ["lunedi"] = DayOfWeek.Monday, ["monday"] = DayOfWeek.Monday,
            ["martedì"] = DayOfWeek.Tuesday, ["martedi"] = DayOfWeek.Tuesday, ["tuesday"] = DayOfWeek.Tuesday,
            ["mercoledì"] = DayOfWeek.Wednesday, ["mercoledi"] = DayOfWeek.Wednesday, ["wednesday"] = DayOfWeek.Wednesday,
            ["giovedì"] = DayOfWeek.Thursday, ["giovedi"] = DayOfWeek.Thursday, ["thursday"] = DayOfWeek.Thursday,
            ["venerdì"] = DayOfWeek.Friday, ["venerdi"] = DayOfWeek.Friday, ["friday"] = DayOfWeek.Friday,
            ["sabato"] = DayOfWeek.Saturday, ["saturday"] = DayOfWeek.Saturday,
            ["domenica"] = DayOfWeek.Sunday, ["sunday"] = DayOfWeek.Sunday,
        };

        private static readonly Dictionary<string, int> _months = new()
        {
            ["gennaio"] = 1, ["febbraio"] = 2, ["marzo"] = 3, ["aprile"] = 4, ["maggio"] = 5, ["giugno"] = 6,
            ["luglio"] = 7, ["agosto"] = 8, ["settembre"] = 9, ["ottobre"] = 10, ["novembre"] = 11, ["dicembre"] = 12,
            ["january"] = 1, ["february"] = 2, ["march"] = 3, ["april"] = 4, ["may"] = 5, ["june"] = 6,
            ["july"] = 7, ["august"] = 8, ["september"] = 9, ["october"] = 10, ["november"] = 11, ["december"] = 12,
        };

        // numeri in lettere (giorni del mese e ore)
        private static readonly Dictionary<string, int> _numbers = new()
        {
            ["zero"] = 0, ["primo"] = 1, ["uno"] = 1, ["una"] = 1, ["due"] = 2, ["tre"] = 3, ["quattro"] = 4, ["cinque"] = 5,
            ["sei"] = 6, ["sette"] = 7, ["otto"] = 8, ["nove"] = 9, ["dieci"] = 10, ["undici"] = 11, ["dodici"] = 12,
            ["tredici"] = 13, ["quattordici"] = 14, ["quindici"] = 15, ["sedici"] = 16, ["diciassette"] = 17,
            ["diciotto"] = 18, ["diciannove"] = 19, ["venti"] = 20, ["ventuno"] = 21, ["ventidue"] = 22,
            ["ventitre"] = 23, ["ventitré"] = 23, ["ventiquattro"] = 24, ["venticinque"] = 25, ["ventisei"] = 26,
            ["ventisette"] = 27, ["ventotto"] = 28, ["ventinove"] = 29, ["trenta"] = 30, ["trentuno"] = 31,
            ["first"] = 1, ["one"] = 1, ["two"] = 2, ["three"] = 3, ["four"] = 4, ["five"] = 5, ["six"] = 6,
            ["seven"] = 7, ["eight"] = 8, ["nine"] = 9, ["ten"] = 10, ["eleven"] = 11, ["twelve"] = 12,
        };

        // parole davanti alla data che non ne cambiano il valore ("il 15", "del 15 maggio", "dal 3", "on May 15")
        private static readonly string[] _datePrefixes =
            { "il ", "lo ", "l'", "del ", "dello ", "di ", "per ", "giorno ", "in data ", "a partire dal ", "dal ", "fino al ", "al ",
              "on ", "the ", "for ", "from ", "until ", "to " };

        private static readonly string[] _dateFormats =
            { "d/M/yyyy", "d-M-yyyy", "d.M.yyyy", "yyyy-M-d", "d/M/yy", "d-M-yy", "d.M.yy" };

        public static bool TryParseDate(string? text, DateTime today, out DateTime result)
        {
            result = default;
            var t = StripPrefixes(Clean(text), _datePrefixes);
            if (t.Length == 0) return false;

            // formati numerici completi
            if (DateTime.TryParseExact(t, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;

            // giorno/mese senza anno -> anno corrente
            var dm = Regex.Match(t, @"^(\d{1,2})[/\-.](\d{1,2})$");
            if (dm.Success)
                return TryDate(today.Year, int.Parse(dm.Groups[2].Value), int.Parse(dm.Groups[1].Value), out result);

            // giorni relativi
            switch (t)
            {
                case "oggi": case "today": result = today; return true;
                case "domani": case "tomorrow": result = today.AddDays(1); return true;
                case "ieri": case "yesterday": result = today.AddDays(-1); return true;
                case "dopodomani": case "day after tomorrow": case "the day after tomorrow": result = today.AddDays(2); return true;
                case "altroieri": case "l'altroieri": case "altro ieri": case "l'altro ieri":
                case "day before yesterday": case "the day before yesterday":
                    result = today.AddDays(-2); return true;

                // settimane: il lunedì della settimana (l'inizio di un periodo di 7 giorni)
                case "questa settimana": case "settimana corrente": case "this week":
                    result = ResolveWeekday(today, DayOfWeek.Monday, string.Empty); return true;
                case "la prossima settimana": case "prossima settimana": case "settimana prossima": case "la settimana prossima": case "next week":
                    result = ResolveWeekday(today, DayOfWeek.Monday, string.Empty).AddDays(7); return true;
                case "la settimana scorsa": case "settimana scorsa": case "la scorsa settimana": case "scorsa settimana": case "last week":
                    result = ResolveWeekday(today, DayOfWeek.Monday, string.Empty).AddDays(-7); return true;
            }

            // giorno della settimana: "lunedì", "lunedì scorso", "venerdì prossimo", "last monday", "next friday"
            var wd = Regex.Match(t, @"^(?:(?<pre>last|next|this|questo|quest')\s*)?(?<wd>\p{L}+)(?:\s+(?<post>scorso|scorsa|passato|passata|prossimo|prossima))?$");
            if (wd.Success && _weekdays.TryGetValue(wd.Groups["wd"].Value, out var dow))
            {
                var q = wd.Groups["pre"].Success ? wd.Groups["pre"].Value : wd.Groups["post"].Value;
                result = ResolveWeekday(today, dow, q);
                return true;
            }

            // "15 maggio", "15 maggio 2026", "primo maggio", "1° maggio", "15th of May 2026"
            var dmy = Regex.Match(t, @"^(?<d>\d{1,2}|\p{L}+)(?:°|º|st|nd|rd|th)?\s+(?:di\s+|of\s+)?(?<m>\p{L}+)(?:\s+(?<y>\d{4}))?$");
            if (dmy.Success && TryDayNumber(dmy.Groups["d"].Value, out var d1) && _months.TryGetValue(dmy.Groups["m"].Value, out var m1))
            {
                var y1 = dmy.Groups["y"].Success ? int.Parse(dmy.Groups["y"].Value) : today.Year;
                return TryDate(y1, m1, d1, out result);
            }

            // "May 15", "May 15th, 2026"
            var mdy = Regex.Match(t, @"^(?<m>\p{L}+)\s+(?<d>\d{1,2})(?:st|nd|rd|th)?(?:,?\s+(?<y>\d{4}))?$");
            if (mdy.Success && _months.TryGetValue(mdy.Groups["m"].Value, out var m2))
            {
                var y2 = mdy.Groups["y"].Success ? int.Parse(mdy.Groups["y"].Value) : today.Year;
                return TryDate(y2, m2, int.Parse(mdy.Groups["d"].Value), out result);
            }

            // solo il giorno: "15", "15°", "15th" -> mese corrente
            var d = Regex.Match(t, @"^(?<d>\d{1,2})(?:°|º|st|nd|rd|th)?$");
            if (d.Success)
                return TryDate(today.Year, today.Month, int.Parse(d.Groups["d"].Value), out result);

            // un periodo ("ottobre", "il mese scorso", "la prima settimana di ottobre"): il primo giorno
            if (TryParsePeriod(text, today, out result, out _))
                return true;

            return false;
        }

        // parole davanti a un mese che non ne cambiano il valore ("di ottobre", "a ottobre", "nel mese di ottobre")
        private static readonly string[] _monthPrefixes =
            { "nel mese di ", "il mese di ", "mese di ", "durante il mese di ", "durante ", "per il mese di ", "per tutto ",
              "a partire da ", "fino a ", "fino ad ", "entro ", "da ", "di ", "del ", "per ", "a ", "ad ", "in ", "nel ",
              "in the month of ", "the month of ", "during ", "for ", "in ", "from ", "until ", "till ", "through ", "to " };

        // Un mese intero: "ottobre", "ottobre 2026", "di ottobre", "questo mese", "il mese scorso", "next month".
        // Mese senza anno: quello dell'anno corrente.
        public static bool TryParseMonth(string? text, DateTime today, out DateTime start, out DateTime end)
        {
            start = end = default;
            var t = StripPrefixes(Clean(text), _monthPrefixes);
            if (t.Length == 0) return false;

            var first = new DateTime(today.Year, today.Month, 1);
            DateTime? month = t switch
            {
                "questo mese" or "il mese corrente" or "mese corrente" or "quest mese" or "this month" or "the current month" => first,
                "il mese prossimo" or "mese prossimo" or "il prossimo mese" or "prossimo mese" or "next month" => first.AddMonths(1),
                "il mese scorso" or "mese scorso" or "lo scorso mese" or "scorso mese" or "il mese passato" or "mese passato"
                    or "last month" or "the previous month" or "previous month" => first.AddMonths(-1),
                _ => null
            };

            if (month == null)
            {
                var m = Regex.Match(t, @"^(?:il\s+mese\s+di\s+|the\s+)?(?<m>\p{L}+)(?:\s+(?:del\s+|of\s+)?(?<y>\d{4}))?$");
                if (!m.Success || !_months.TryGetValue(m.Groups["m"].Value, out var mm)) return false;
                var y = m.Groups["y"].Success ? int.Parse(m.Groups["y"].Value) : today.Year;
                month = new DateTime(y, mm, 1);
            }

            start = month.Value;
            end = start.AddMonths(1).AddDays(-1);
            return true;
        }

        // Un periodo di piu' giorni: un mese (TryParseMonth) o una settimana (TryParseWeek)
        public static bool TryParsePeriod(string? text, DateTime today, out DateTime start, out DateTime end) =>
            TryParseWeek(text, today, out start, out end) || TryParseMonth(text, today, out start, out end);

        private static readonly string[] _weekPrefixes =
 { "durante la ", "per tutta la ", "per la ", "nella ", "la ", "l'", "a partire dalla ", "dalla ", "fino alla ", "entro la ",
              "during the ", "for the ", "in the ", "the ", "from the ", "until the ", "through the ", "by the " };

        private static readonly Dictionary<string, int> _weekOrdinals = new()
        {
            ["prima"] = 1, ["1a"] = 1, ["1ª"] = 1, ["seconda"] = 2, ["2a"] = 2, ["2ª"] = 2, ["terza"] = 3, ["3a"] = 3, ["3ª"] = 3,
            ["quarta"] = 4, ["4a"] = 4, ["4ª"] = 4, ["quinta"] = 5, ["5a"] = 5, ["5ª"] = 5, ["ultima"] = -1,
            ["first"] = 1, ["1st"] = 1, ["second"] = 2, ["2nd"] = 2, ["third"] = 3, ["3rd"] = 3, ["fourth"] = 4, ["4th"] = 4,
            ["fifth"] = 5, ["5th"] = 5, ["last"] = -1,
        };

        // Una settimana, da lunedi' a domenica:
        // - "questa settimana", "la prossima settimana", "la settimana scorsa" (this/next/last week);
        // - "la prima/seconda/.../ultima settimana di ottobre [2026]" (first ... last week of October): settimane del
        //   calendario tagliate ai confini del mese. Come per le settimane ISO, una settimana e' del mese in cui cade
        //   il suo giovedi': la prima di novembre 2026 (1 novembre domenica) e' 2-8 novembre, la prima di ottobre 2026
        //   (1 ottobre giovedi') e' 1-4 ottobre. Il mese e' quello di TryParseMonth ("di ottobre", "del mese prossimo").
        public static bool TryParseWeek(string? text, DateTime today, out DateTime start, out DateTime end)
        {
            start = end = default;
            var t = StripPrefixes(Clean(text), _weekPrefixes);
            if (t.Length == 0) return false;

            var lunedi = ResolveWeekday(today, DayOfWeek.Monday, string.Empty);
            DateTime? settimana = t switch
            {
                "questa settimana" or "settimana corrente" or "this week" => lunedi,
                "prossima settimana" or "settimana prossima" or "next week" => lunedi.AddDays(7),
                "settimana scorsa" or "scorsa settimana" or "settimana passata" or "last week" or "previous week" => lunedi.AddDays(-7),
                _ => null
            };
            if (settimana != null)
            {
                start = settimana.Value;
                end = start.AddDays(6);
                return true;
            }

            var w = Regex.Match(t, @"^(?<n>\S+)\s+(?:settimana|week)\s+(?:of\s+)?(?<mese>.+)$");
            if (!w.Success || !_weekOrdinals.TryGetValue(w.Groups["n"].Value, out var n)) return false;
            if (!TryParseMonth(w.Groups["mese"].Value, today, out var inizioMese, out var fineMese)) return false;

            // lunedi' della prima e dell'ultima settimana del mese (quelle con il giovedi' nel mese)
            static DateTime Lunedi(DateTime g) => g.AddDays(-(((int)g.DayOfWeek + 6) % 7));
            var primo = Lunedi(inizioMese);
            if (primo.AddDays(3) < inizioMese) primo = primo.AddDays(7);
            var ultimo = Lunedi(fineMese);
            if (ultimo.AddDays(3) > fineMese) ultimo = ultimo.AddDays(-7);

            var da = n > 0 ? primo.AddDays(7 * (n - 1)) : ultimo;
            if (da > ultimo) return false;   // "quinta settimana" di un mese che ne ha quattro

            start = da < inizioMese ? inizioMese : da;
            end = da.AddDays(6) > fineMese ? fineMese : da.AddDays(6);
            return true;
        }

        // Giorno della settimana senza qualificatore: quello della settimana corrente (lunedì-domenica).
        // "scorso"/"last": l'ultimo prima di oggi; "prossimo"/"next": il primo dopo oggi.
        private static DateTime ResolveWeekday(DateTime today, DayOfWeek dow, string qualifier)
        {
            switch (qualifier)
            {
                case "scorso": case "scorsa": case "passato": case "passata": case "last":
                {
                    var back = ((int)today.DayOfWeek - (int)dow + 7) % 7;
                    return today.AddDays(back == 0 ? -7 : -back);
                }
                case "prossimo": case "prossima": case "next":
                {
                    var forward = ((int)dow - (int)today.DayOfWeek + 7) % 7;
                    return today.AddDays(forward == 0 ? 7 : forward);
                }
                default:
                {
                    var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
                    return monday.AddDays(((int)dow + 6) % 7);
                }
            }
        }

        private static bool TryDayNumber(string value, out int day)
        {
            if (int.TryParse(value, out day)) return true;
            return _numbers.TryGetValue(value, out day);
        }

        private static bool TryDate(int year, int month, int day, out DateTime result)
        {
            result = default;
            if (month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month)) return false;
            result = new DateTime(year, month, day);
            return true;
        }

        // ---------------------------------------------------------------------------
        // Orari
        // ---------------------------------------------------------------------------

        // parole davanti all'orario che non ne cambiano il valore ("alle 9", "delle 17", "dalle 8", "at 9")
        private static readonly string[] _timePrefixes =
            { "alle ", "all'", "delle ", "dell'", "dalle ", "dall'", "della ", "ore ", "le ", "verso le ", "per le ",
              "at ", "around ", "from ", "to " };

        // frazioni dopo "e": "9 e mezza", "9 e un quarto", "9 e 20"
        private static readonly Dictionary<string, int> _minuteWords = new()
        {
            ["mezza"] = 30, ["mezzo"] = 30, ["un quarto"] = 15, ["quarto"] = 15, ["tre quarti"] = 45,
            ["cinque"] = 5, ["dieci"] = 10, ["quindici"] = 15, ["venti"] = 20, ["venticinque"] = 25,
            ["trenta"] = 30, ["trentacinque"] = 35, ["quaranta"] = 40, ["quarantacinque"] = 45,
            ["cinquanta"] = 50, ["cinquantacinque"] = 55,
        };

        public static bool TryParseTime(string? text, DateTime now, out TimeOnly result)
        {
            result = default;
            var t = StripPrefixes(Clean(text), _timePrefixes);
            if (t.Length == 0) return false;

            switch (t)
            {
                case "adesso": case "ora": case "subito": case "in questo momento": case "now": case "right now":
                    result = new TimeOnly(now.Hour, now.Minute); return true;
                case "mezzogiorno": case "noon": case "midday":
                    result = new TimeOnly(12, 0); return true;
                case "mezzanotte": case "midnight":
                    result = new TimeOnly(0, 0); return true;
            }

            // parte del giorno: "le 3 del pomeriggio", "8 di sera", "9 di mattina"
            bool pm = false;
            foreach (var suffix in new[] { " del pomeriggio", " di pomeriggio", " di sera", " della sera", " in the afternoon", " in the evening" })
                if (t.EndsWith(suffix)) { pm = true; t = t[..^suffix.Length].Trim(); }
            foreach (var suffix in new[] { " di mattina", " del mattino", " della mattina", " in the morning" })
                if (t.EndsWith(suffix)) t = t[..^suffix.Length].Trim();

            // "2 pm", "9:30am", "11 a.m."
            var ap = Regex.Match(t, @"^(?<h>\d{1,2})(?:[:.](?<m>\d{2}))?\s*(?<ap>am|pm|a\.m\.?|p\.m\.?)$");
            if (ap.Success)
            {
                var h = int.Parse(ap.Groups["h"].Value);
                var m = ap.Groups["m"].Success ? int.Parse(ap.Groups["m"].Value) : 0;
                if (h < 1 || h > 12) return false;
                if (ap.Groups["ap"].Value.StartsWith('p')) { if (h < 12) h += 12; }
                else if (h == 12) h = 0;
                return TryTime(h, m, false, out result);
            }

            // "9", "9:30", "09.30", "9,30", "14h30"
            var num = Regex.Match(t, @"^(?<h>\d{1,2})(?:[:.,h](?<m>\d{1,2}))?$");
            if (num.Success)
                return TryTime(int.Parse(num.Groups["h"].Value),
                               num.Groups["m"].Success ? int.Parse(num.Groups["m"].Value) : 0, pm, out result);

            // inglese: "half past ten", "quarter past nine", "quarter to ten"
            var en = Regex.Match(t, @"^(?<f>half|quarter)\s+(?<rel>past|to)\s+(?<h>\d{1,2}|\p{L}+)$");
            if (en.Success && TryHour(en.Groups["h"].Value, out var eh))
            {
                var minutes = en.Groups["f"].Value == "half" ? 30 : 15;
                if (en.Groups["rel"].Value == "to") { eh -= 1; minutes = 60 - minutes; }
                return TryTime(eh, minutes, pm, out result);
            }

            // italiano: "nove", "nove e mezza", "9 e un quarto", "10 meno un quarto", "dieci meno cinque"
            var it = Regex.Match(t, @"^(?<h>\d{1,2}|\p{L}+)(?:\s+e\s+(?<plus>\d{1,2}|un quarto|tre quarti|\p{L}+))?(?:\s+meno\s+(?<minus>\d{1,2}|un quarto|\p{L}+))?$");
            if (it.Success && TryHour(it.Groups["h"].Value, out var ih))
            {
                int im = 0;
                if (it.Groups["plus"].Success && !TryMinutes(it.Groups["plus"].Value, out im)) return false;
                if (it.Groups["minus"].Success)
                {
                    if (!TryMinutes(it.Groups["minus"].Value, out var minus)) return false;
                    ih -= 1; im = 60 - minus;
                }
                return TryTime(ih, im, pm, out result);
            }

            return false;
        }

        private static bool TryHour(string value, out int hour)
        {
            if (int.TryParse(value, out hour)) return true;
            return _numbers.TryGetValue(value, out hour) && value != "primo" && value != "first";
        }

        private static bool TryMinutes(string value, out int minutes)
        {
            if (int.TryParse(value, out minutes)) return true;
            return _minuteWords.TryGetValue(value, out minutes);
        }

        private static bool TryTime(int hour, int minute, bool pm, out TimeOnly result)
        {
            result = default;
            if (pm && hour < 12) hour += 12;
            if (hour == 24) hour = 0;
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59) return false;
            result = new TimeOnly(hour, minute);
            return true;
        }

        // ---------------------------------------------------------------------------
        // Supporto
        // ---------------------------------------------------------------------------

        // minuscolo, apostrofo tipografico -> semplice, senza punteggiatura finale, spazi singoli
        private static string Clean(string? text)
        {
            var t = (text ?? string.Empty).ToLowerInvariant().Replace('’', '\'').Trim().TrimEnd('.', ',', ';', '!', '?').Trim();
            return string.Join(' ', t.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string StripPrefixes(string text, string[] prefixes)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var p in prefixes)
                    if (text.StartsWith(p) && text.Length > p.Length)
                    {
                        text = text[p.Length..].TrimStart();
                        changed = true;
                    }
            }
            return text;
        }
    }
}
