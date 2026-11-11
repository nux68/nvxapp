using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /// <summary>
    /// Controlli al salvataggio di un orario (ANALISI_ATTENDANCETRACKING_PAUSE.md §3.3, §4.1):
    /// coerenza degli intervalli di riconoscimento delle coppie e regole delle pause.
    /// </summary>
    public static class GestionePresenzeOrarioValidazione
    {
        public static void ControllaIntervalli(IEnumerable<Par_OrarioIntervalloHHModel> coppie)
        {
            // le ancore si calcolano su un giorno qualsiasi: contano solo le differenze
            var ancore = GestionePresenzeSequenza.Ancore(new DateTime(2000, 1, 3), coppie);

            for (int i = 0; i < ancore.Count; i++)
            {
                var a = ancore[i];
                if (a.Al - a.Dal >= TimeSpan.FromHours(12))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("OrarioIntervalloNonValido", a.NumCoppia, a.Ora.ToString("HH:mm")));

                if (i > 0 && a.Dal < ancore[i - 1].Dal)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("OrarioIntervalliFuoriOrdine", a.NumCoppia));

                if (!a.Entrata && i > 0 && ancore[i - 1].Entrata && ancore[i - 1].NumCoppia == a.NumCoppia && ancore[i - 1].Al >= a.Dal)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("OrarioIntervalliSovrapposti", a.NumCoppia));
            }
        }

        public static void ControllaPause(OrarioTimbratureTipo tipo, IEnumerable<Par_OrarioIntervalloHHModel> coppie, IEnumerable<Par_OrarioPausaModel> pause)
        {
            var lista = pause.ToList();
            if (lista.Count == 0)
                return;

            if (tipo != OrarioTimbratureTipo.IntervalloOrario)
                throw new UserMessageException(AttendanceTrackingMessages.Get("PausaSoloIntervalloOrario"));
            if (lista.Count(p => p.Tipo == PausaTipo.Pranzo) > 1)
                throw new UserMessageException(AttendanceTrackingMessages.Get("PausaPranzoUnica"));
            if (lista.Count(p => p.Tipo == PausaTipo.Breve) > 1)
                throw new UserMessageException(AttendanceTrackingMessages.Get("PausaBreveUnica"));

            int numCoppie = coppie.Count(c => c.Dalle.HasValue && c.Alle.HasValue && c.Dalle.Value != c.Alle.Value);

            foreach (var p in lista)
            {
                var nome = string.IsNullOrWhiteSpace(p.Descrizione) ? p.Tipo.ToString() : p.Descrizione;

                if (p.DurataMin < 0 || p.RetribuitaMin < 0 || p.SogliaMin < 0 || p.DurataMaxMin < 0)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("PausaValoriNegativi", nome));
                if (p.Tipo == PausaTipo.Breve && p.Modalita == PausaModalita.Automatica)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("PausaAutomaticaSoloPranzo", nome));
                if (p.FinestraDalle.HasValue != p.FinestraAlle.HasValue)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("PausaFinestraIncompleta", nome));

                if (p.Tipo == PausaTipo.Pranzo)
                {
                    if (p.DurataMin <= 0)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("PausaDurataNonValida", nome));
                    if (p.RetribuitaMin > p.DurataMin)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("PausaRetribuitaNonValida", nome));
                    if (p.DurataMaxMin.HasValue && p.DurataMaxMin.Value < p.DurataMin)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("PausaDurataMaxNonValida", nome));
                    if (numCoppie <= 1 && !p.FinestraDalle.HasValue)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("PausaFinestraObbligatoria", nome));
                }
            }
        }
    }
}
