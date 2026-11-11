using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService
{
    /*
     Regole del lavoro agile (AT-SW, ANALISI_ATTENDANCETRACKING_SMARTWORKING.md §4):
       - giustificativo dedicato (Par_Giustificativi.LavoroAgile; creato al primo uso con la causale SW, segno neutro)
       - richiesta di un giorno: accordo valido e firmato, giorno della settimana consentito, limite settimanale (lun-dom,
         contano i giorni in attesa e approvati)
       - giorno di lavoro agile approvato: la timbratura evoluta non applica il geofence (AT-TIMB)
    */
    public class LavoroAgileUtility : ILavoroAgileGiorno, ILavoroAgileUtility
    {
        public const string FontePresaVisione = "at.accordo-sw";

        private static readonly StatoRichiesta[] StatiApprovati = { StatoRichiesta.Diretta, StatoRichiesta.Approvata };
        private static readonly StatoRichiesta[] StatiValidi = { StatoRichiesta.Diretta, StatoRichiesta.Approvata, StatoRichiesta.Immessa,
                                                                 StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata };

        private readonly IDip_AccordoSWRepository _dip_AccordoSWRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IAcknowledgementService _acknowledgementService;

        public LavoroAgileUtility(IDip_AccordoSWRepository dip_AccordoSWRepository,
                                  IPar_GiustificativiRepository par_GiustificativiRepository,
                                  IPar_CausaliRepository par_CausaliRepository,
                                  IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository,
                                  IAcknowledgementService acknowledgementService)
        {
            _dip_AccordoSWRepository = dip_AccordoSWRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _par_CausaliRepository = par_CausaliRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _acknowledgementService = acknowledgementService;
        }

        public bool IsGiornoLavoroAgile(int idDip_RapportoLavoro, DateTime giorno)
        {
            var ids = _par_GiustificativiRepository.FindAll(g => g.LavoroAgile).Select(g => g.Id).ToList();
            if (ids.Count == 0)
                return false;
            var d = giorno.Date;
            return _dip_GG_GiustificativiRepository.FindAll(g => g.IdDip_RapportoLavoro == idDip_RapportoLavoro && ids.Contains(g.IdPar_Giustificativi) &&
                                                                 g.Data >= d && g.Data < d.AddDays(1) && StatiApprovati.Contains(g.RichiestaStato))
                                                   .Any();
        }

        public bool IsGiustificativoLavoroAgile(int idPar_Giustificativi)
            => _par_GiustificativiRepository.FindAll(g => g.Id == idPar_Giustificativi && g.LavoroAgile).Any();

        // giustificativo del lavoro agile dell'azienda (il primo): per le aziende create prima di AT-SW si crea al primo uso
        public async Task<Par_Giustificativi> GetOrCreateGiustificativoAsync(int idAz_Anagrafica)
        {
            var g = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == idAz_Anagrafica && x.LavoroAgile).OrderBy(x => x.Id).FirstOrDefault();
            if (g != null)
                return g;

            var causale = _par_CausaliRepository.FindAll(c => c.IdAz_Anagrafica == idAz_Anagrafica && c.Codice == "SW").FirstOrDefault()
                          ?? await _par_CausaliRepository.UpsertAsync(new Par_Causali { IdAz_Anagrafica = idAz_Anagrafica, Codice = "SW", Descrizione = "Lavoro agile" });
            var codice = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == idAz_Anagrafica && x.Codice == "SW").Any() ? "SWA" : "SW";
            return await _par_GiustificativiRepository.UpsertAsync(new Par_Giustificativi
            {
                IdAz_Anagrafica = idAz_Anagrafica,
                Codice = codice,
                Descrizione = "Lavoro agile",
                BackgroundColor = "#3dc2ff",
                TextColor = "#000000",
                TipoInput = JustTipoInput.InteraGiornate,
                IdCausale = causale.Id,
                Segno = SignWithNeutral.Neutral,
                VisualizzaInPianoFerie = true,
                TipoContatore = TipoContatore.NoContatore,
                LavoroAgile = true
            });
        }

        // accordo firmato (presa visione confermata); aggiorna DataFirma
        public async Task<bool> FirmatoAsync(int idCompany, Dip_AccordoSW accordo)
        {
            if (accordo.DataFirma != null)
                return true;
            var stato = await _acknowledgementService.StatusAsync(idCompany, ApplicationType.AttendanceTracking, FontePresaVisione, accordo.Id);
            var conferma = stato.Where(s => s.DataConferma != null).Select(s => s.DataConferma).FirstOrDefault();
            if (conferma == null)
                return false;
            accordo.DataFirma = conferma;
            await _dip_AccordoSWRepository.UpdateAsync(accordo);
            return true;
        }

        public static DateTime Lunedi(DateTime d) => d.Date.AddDays(-(((int)d.DayOfWeek + 6) % 7));

        public static bool GiornoConsentito(string? maschera, DateTime d)
        {
            var m = maschera is { Length: 7 } ? maschera : "1111100";
            return m[((int)d.DayOfWeek + 6) % 7] == '1';
        }

        // giorni di lavoro agile del rapporto (in attesa o approvati) nel periodo [dal, al)
        public List<(DateTime Data, StatoRichiesta Stato, int? IdDip_GG_Richiesta)> Giorni(int idDip_RapportoLavoro, DateTime dal, DateTime al)
        {
            var idSW = _par_GiustificativiRepository.FindAll(g => g.LavoroAgile).Select(g => g.Id).ToList();
            return _dip_GG_GiustificativiRepository.FindAll(g => g.IdDip_RapportoLavoro == idDip_RapportoLavoro && idSW.Contains(g.IdPar_Giustificativi) &&
                                                                 g.Data >= dal && g.Data < al && StatiValidi.Contains(g.RichiestaStato))
                                                   .Select(g => new { g.Data, g.RichiestaStato, g.IdDip_GG_Richiesta }).ToList()
                                                   .Select(g => (g.Data.Date, g.RichiestaStato, g.IdDip_GG_Richiesta)).ToList();
        }

        // null = i giorni si possono richiedere; altrimenti il motivo (testo per l'utente)
        public async Task<string?> ValidaRichiestaAsync(int idCompany, int idDip_RapportoLavoro, IEnumerable<DateTime> giorni)
        {
            var richiesti = giorni.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
            if (richiesti.Count == 0)
                return null;

            var accordi = _dip_AccordoSWRepository.FindAll(a => a.IdDip_RapportoLavoro == idDip_RapportoLavoro).ToList();
            foreach (var d in richiesti)
            {
                var accordo = accordi.FirstOrDefault(a => a.Dal.Date <= d && a.Al.Date >= d);
                if (accordo == null)
                    return AttendanceTrackingMessages.Get("SwNessunAccordo", d);
                if (!await FirmatoAsync(idCompany, accordo))
                    return AttendanceTrackingMessages.Get("SwAccordoNonFirmato", d);
                if (!GiornoConsentito(accordo.GiorniConsentiti, d))
                    return AttendanceTrackingMessages.Get("SwGiornoNonConsentito", d);
            }

            var esistenti = Giorni(idDip_RapportoLavoro, Lunedi(richiesti.First()), Lunedi(richiesti.Last()).AddDays(7)).Select(g => g.Data).ToList();
            var doppio = richiesti.FirstOrDefault(d => esistenti.Contains(d));
            if (doppio != default)
                return AttendanceTrackingMessages.Get("SwGiaRichiesto", doppio);

            foreach (var settimana in richiesti.GroupBy(Lunedi))
            {
                var accordo = accordi.First(a => a.Dal.Date <= settimana.First() && a.Al.Date >= settimana.First());
                int usati = esistenti.Count(d => Lunedi(d) == settimana.Key);
                if (usati + settimana.Count() > accordo.MaxGiorniSettimana)
                    return AttendanceTrackingMessages.Get("SwOltreLimite", accordo.MaxGiorniSettimana, settimana.Key, usati);
            }
            return null;
        }
    }

    public interface ILavoroAgileUtility
    {
        bool IsGiustificativoLavoroAgile(int idPar_Giustificativi);
        Task<Par_Giustificativi> GetOrCreateGiustificativoAsync(int idAz_Anagrafica);
        Task<bool> FirmatoAsync(int idCompany, Dip_AccordoSW accordo);
        List<(DateTime Data, StatoRichiesta Stato, int? IdDip_GG_Richiesta)> Giorni(int idDip_RapportoLavoro, DateTime dal, DateTime al);
        Task<string?> ValidaRichiestaAsync(int idCompany, int idDip_RapportoLavoro, IEnumerable<DateTime> giorni);
    }
}
