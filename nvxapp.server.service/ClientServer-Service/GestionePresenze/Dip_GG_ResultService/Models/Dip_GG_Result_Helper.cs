using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models
{
    public static class Dip_GG_Result_Helper
    {
        // maschera di tutti i dettagli errore (Err_1 → Err_20)
        private const GG_ResultStato ALL_ERROR_DETAILS =
            GG_ResultStato.Err_TimbratureMancanti | GG_ResultStato.Err_2 | GG_ResultStato.Err_3 |
            GG_ResultStato.Err_4 | GG_ResultStato.Err_5 | GG_ResultStato.Err_6 |
            GG_ResultStato.Err_7 | GG_ResultStato.Err_8 | GG_ResultStato.Err_9 |
            GG_ResultStato.Err_10 | GG_ResultStato.Err_11 | GG_ResultStato.Err_12 |
            GG_ResultStato.Err_13 | GG_ResultStato.Err_14 | GG_ResultStato.Err_15 |
            GG_ResultStato.Err_16 | GG_ResultStato.Err_17 | GG_ResultStato.Err_18 |
            GG_ResultStato.Err_19 | GG_ResultStato.Err_20;

        // maschera di tutti i dettagli warning (Warning_1 → Warning_20)
        private const GG_ResultStato ALL_WARNING_DETAILS =
            GG_ResultStato.Warning_1 | GG_ResultStato.Warning_2 | GG_ResultStato.Warning_3 |
            GG_ResultStato.Warning_4 | GG_ResultStato.Warning_5 | GG_ResultStato.Warning_6 |
            GG_ResultStato.Warning_7 | GG_ResultStato.Warning_8 | GG_ResultStato.Warning_9 |
            GG_ResultStato.Warning_10 | GG_ResultStato.Warning_11 | GG_ResultStato.Warning_12 |
            GG_ResultStato.Warning_13 | GG_ResultStato.Warning_14 | GG_ResultStato.Warning_15 |
            GG_ResultStato.Warning_16 | GG_ResultStato.Warning_17 | GG_ResultStato.Warning_18 |
            GG_ResultStato.Warning_19 | GG_ResultStato.Warning_20;

        /// <summary>
        /// Imposta lo stato principale (Init/OK/Locked/Warning/Err) pulendo prima gli stati precedenti.
        /// </summary>
        public static void SetState(ref GG_ResultStato stato, GG_ResultStato state)
        {
            stato &= ~GG_ResultStato.STATE_MASK;
            stato |= state;
        }

        /// <summary>
        /// Aggiunge un dettaglio errore o warning e aggiorna automaticamente Err/Warning.
        /// Rimuove OK se presente.
        /// </summary>
        public static void AddDetail(ref GG_ResultStato stato, GG_ResultStato detail)
        {
            stato |= detail;

            if (IsErrorDetail(detail))
            {
                stato |= GG_ResultStato.Err;
                stato &= ~GG_ResultStato.OK;    // non può essere OK se c'è un errore
                stato &= ~GG_ResultStato.Init;  // ← rimuove Init
            }

            if (IsWarningDetail(detail))
            {
                stato |= GG_ResultStato.Warning;
                stato &= ~GG_ResultStato.OK;    // non può essere OK se c'è un warning
                stato &= ~GG_ResultStato.Init;  // ← rimuove Init
            }
        }

        /// <summary>
        /// Rimuove un dettaglio errore o warning e aggiorna automaticamente Err/Warning/OK.
        /// Se non ci sono più dettagli attivi, imposta OK.
        /// </summary>
        public static void RemoveDetail(ref GG_ResultStato stato, GG_ResultStato detail)
        {
            stato &= ~detail;

            if (IsErrorDetail(detail))
            {
                // rimuovi Err solo se non ci sono più dettagli di errore attivi
                if (!HasAnyErrorDetail(stato))
                    stato &= ~GG_ResultStato.Err;
            }

            if (IsWarningDetail(detail))
            {
                // rimuovi Warning solo se non ci sono più dettagli di warning attivi
                if (!HasAnyWarningDetail(stato))
                    stato &= ~GG_ResultStato.Warning;
            }

            // se non ci sono più né errori né warning → imposta OK
            if (!HasAnyErrorDetail(stato) && !HasAnyWarningDetail(stato))
            {
                stato &= ~GG_ResultStato.STATE_MASK;
                stato |= GG_ResultStato.OK;
            }
        }

        /// <summary>Controlla se un dettaglio specifico è attivo.</summary>
        public static bool HasDetail(GG_ResultStato stato, GG_ResultStato detail)
            => (stato & detail) != 0;

        /// <summary>Restituisce solo lo stato principale (maschera STATE_MASK).</summary>
        public static GG_ResultStato GetState(GG_ResultStato stato)
            => stato & GG_ResultStato.STATE_MASK;

                public static GG_ResultStato Combine(params GG_ResultStato[] sources)
        {
            GG_ResultStato result = 0;

            foreach (var source in sources)
                result |= source & (ALL_ERROR_DETAILS | ALL_WARNING_DETAILS);

            // imposta i flag principali in base ai dettagli combinati
            if (HasAnyErrorDetail(result))
            {
                result |= GG_ResultStato.Err;
                result &= ~GG_ResultStato.OK;
                result &= ~GG_ResultStato.Init;
            }
            else if (HasAnyWarningDetail(result))
            {
                result |= GG_ResultStato.Warning;
                result &= ~GG_ResultStato.OK;
                result &= ~GG_ResultStato.Init;
            }
            else
            {
                // nessun dettaglio → OK
                result |= GG_ResultStato.OK;
            }

            return result;
        }
     

        /// <summary>Converte lo stato in long (enum è long).</summary>
        public static long ToLong(GG_ResultStato stato)
            => (long)stato;

        // ── helper privati ──────────────────────────────────────────────────────

        private static bool IsErrorDetail(GG_ResultStato value)
            => (value & ALL_ERROR_DETAILS) != 0;

        private static bool IsWarningDetail(GG_ResultStato value)
            => (value & ALL_WARNING_DETAILS) != 0;

        private static bool HasAnyErrorDetail(GG_ResultStato stato)
            => (stato & ALL_ERROR_DETAILS) != 0;

        private static bool HasAnyWarningDetail(GG_ResultStato stato)
            => (stato & ALL_WARNING_DETAILS) != 0;
    }
}
