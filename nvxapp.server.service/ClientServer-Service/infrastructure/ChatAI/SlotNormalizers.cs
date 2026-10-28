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

        // espressione di data -> "dd/MM/yyyy" (null se non riconosciuta)
        public static string? Date(string value) =>
            TryParseDate(value, DateTime.Today, out var d) ? d.ToString(DateFormat, CultureInfo.InvariantCulture) : null;

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

            return false;
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
