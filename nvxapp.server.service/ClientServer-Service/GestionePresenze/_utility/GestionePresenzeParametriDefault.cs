using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // Parametri presenze di partenza per una nuova azienda (ricavati dalla configurazione di tenant_1_2):
    // causali, giustificativi, orari DIURNO8H (09-13 / 14-18) e RIPOSO, profilo settimanale DAY7H8
    // (lun-ven DIURNO8H, sab-dom RIPOSO, festivi RIPOSO, straordinario/supplementare giornaliero),
    // calendario delle festivita' nazionali, export.
    // Gli Id NON sono fissi: le chiavi esterne vengono collegate ai record appena creati,
    // cosi' funziona sia in multi-tenant sia in modalita' singola (tabelle condivise tra aziende).
    // Idempotente: se l'azienda ha gia' delle causali non fa nulla.
    public class GestionePresenzeParametriDefault : IGestionePresenzeParametriDefault
    {
        // Codice del profilo orario assegnato di default ai nuovi rapporti di lavoro
        public const string CodiceProfiloOrarioDefault = "DAY7H8";

        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IPar_ExportCauRepository _par_ExportCauRepository;
        private readonly IPar_ExportCau_CausaliRepository _par_ExportCau_CausaliRepository;
        private readonly IPar_FestivitaRepository _par_FestivitaRepository;

        public GestionePresenzeParametriDefault(IPar_CausaliRepository par_CausaliRepository,
                                                IPar_GiustificativiRepository par_GiustificativiRepository,
                                                IPar_OrarioRepository par_OrarioRepository,
                                                IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository,
                                                IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                                IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository,
                                                IPar_ExportCauRepository par_ExportCauRepository,
                                                IPar_ExportCau_CausaliRepository par_ExportCau_CausaliRepository,
                                                IPar_FestivitaRepository par_FestivitaRepository)
        {
            _par_CausaliRepository = par_CausaliRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _par_ExportCauRepository = par_ExportCauRepository;
            _par_ExportCau_CausaliRepository = par_ExportCau_CausaliRepository;
            _par_FestivitaRepository = par_FestivitaRepository;
        }

        public async Task InitParametri(int IdAz_Anagrafica)
        {
            if (_par_CausaliRepository.FindAll(x => x.IdAz_Anagrafica == IdAz_Anagrafica).Any())
                return;

            // Causali
            var causali = new Dictionary<string, Par_Causali>();
            foreach (var (codice, descrizione) in new[]
            {
                ("ORD1",  "Ordinarie 1° fascia"),
                ("STRAO", "Straordinario"),
                ("STRNOT", "Straordinario notturno"),
                ("STRFES", "Straordinario festivo"),
                ("STRFN", "Straordinario festivo notturno"),
                ("SUPPL", "Supplementare"),
                ("SUPEC", "Supplementare oltre limite"),
                ("ECCOR", "Ore eccedenti non straord."),
                ("BOACC", "Accredito banca ore"),
                ("BO",    "Recupero banca ore"),
                ("ASS",   "Assenza ingiustificata"),
                ("FERIE", "Ferie"),
                ("ORDI2", "Ordinarie 2° fascia"),
                ("ORDI3", "Ordinarie 3° fascia"),
                ("ROL",   "ROL"),
                ("MAL",   "Malattia"),
            })
            {
                causali[codice] = await _par_CausaliRepository.UpsertAsync(new Par_Causali
                {
                    IdAz_Anagrafica = IdAz_Anagrafica,
                    Codice = codice,
                    Descrizione = descrizione
                });
            }

            // Giustificativi
            var giustificativi = new Dictionary<string, Par_Giustificativi>();
            foreach (var g in new[]
            {
                new { Codice = "MAL",   Descrizione = "Malattia",        Bg = (string?)"#ff0000", Tx = (string?)"#ffffff", Causale = "MAL",   Segno = SignWithNeutral.Down, Contatore = TipoContatore.NoContatore,         PianoFerie = false },
                new { Codice = "ROL",   Descrizione = "ROL",             Bg = (string?)"#7fff00", Tx = (string?)"#e7f524", Causale = "ROL",   Segno = SignWithNeutral.Down, Contatore = TipoContatore.ContatoreConBlocco,  PianoFerie = true  },
                new { Codice = "FE",    Descrizione = "Ferie",           Bg = (string?)"#ff8c00", Tx = (string?)"#ffffff", Causale = "FERIE", Segno = SignWithNeutral.Down, Contatore = TipoContatore.ContatoreConAvviso,  PianoFerie = true  },
                new { Codice = "ASS",   Descrizione = "Assenza ingiust", Bg = (string?)null,      Tx = (string?)"#f3eded", Causale = "ASS",   Segno = SignWithNeutral.Down, Contatore = TipoContatore.NoContatore         ,  PianoFerie = false },
                // straordinario e supplementare li calcola il motore: niente piu' giustificativo STRAO manuale
                new { Codice = "BO",    Descrizione = "Recupero banca ore", Bg = (string?)"#2228e2", Tx = (string?)"#f8f2f2", Causale = "BO", Segno = SignWithNeutral.Down, Contatore = TipoContatore.ContatoreConBlocco, PianoFerie = true },
                // autorizzazione alle ore in piu' (profili con "solo se autorizzate"): nessuna causale, non cambia il dovuto
                new { Codice = "AUTS",  Descrizione = "Autorizzazione straordinario", Bg = (string?)"#ffc409", Tx = (string?)"#000000", Causale = (string)null!, Segno = SignWithNeutral.Neutral, Contatore = TipoContatore.NoContatore, PianoFerie = false },
            })
            {
                giustificativi[g.Codice] = await _par_GiustificativiRepository.UpsertAsync(new Par_Giustificativi
                {
                    IdAz_Anagrafica = IdAz_Anagrafica,
                    Codice = g.Codice,
                    Descrizione = g.Descrizione,
                    BackgroundColor = g.Bg,
                    TextColor = g.Tx,
                    TipoInput = JustTipoInput.InteraGiornate,
                    IdCausale = g.Causale != null ? causali[g.Causale].Id : null,
                    Segno = g.Segno,
                    VisualizzaInPianoFerie = g.PianoFerie,
                    TipoContatore = g.Contatore,
                    BancaOre = g.Codice == "BO",
                    AutorizzaStraordinario = g.Codice == "AUTS"
                });
            }

            // Orario DIURNO8H: due coppie 09-13 e 14-18, tolleranza +/-15 min, arrotondamento 15 min
            var orario = await _par_OrarioRepository.UpsertAsync(new Par_Orario
            {
                IdAz_Anagrafica = IdAz_Anagrafica,
                Codice = "DIURNO8H",
                Descrizione = "Diurno 8H",
                NumeroCoppie = 2,
                TimbratureTipo = OrarioTimbratureTipo.IntervalloOrario,
                Hh_Teo_MonteOre = new TimeOnly(0, 0)
            });

            foreach (var (numCoppia, dalle, alle) in new[] { (1, new TimeOnly(9, 0), new TimeOnly(13, 0)),
                                                              (2, new TimeOnly(14, 0), new TimeOnly(18, 0)) })
            {
                await _par_OrarioIntervalloHHRepository.UpsertAsync(new Par_OrarioIntervalloHH
                {
                    IdPar_Orario = orario.Id,
                    NumCoppia = numCoppia,
                    Dalle = dalle,
                    Dalle_Limite_SX = dalle.AddMinutes(-15),
                    Dalle_Limite_DX = dalle.AddMinutes(15),
                    Dalle_Arrotondamento = TimeRoundInterval.Min15,
                    Dalle_Arrotondamento_Verso = RoundDirection.Up,
                    Dalle_Use_4_Match = true,
                    Alle = alle,
                    Alle_Limite_SX = alle.AddMinutes(-15),
                    Alle_Limite_DX = alle.AddMinutes(15),
                    Alle_Arrotondamento = TimeRoundInterval.Min15,
                    Alle_Arrotondamento_Verso = RoundDirection.Up,
                    Alle_Use_4_Match = true,
                    IdCausale_HH_Lav = causali["ORD1"].Id
                });
            }

            // Orario RIPOSO: nessuna coppia, 0 ore teoriche (giorno non lavorativo)
            var orarioRiposo = await _par_OrarioRepository.UpsertAsync(new Par_Orario
            {
                IdAz_Anagrafica = IdAz_Anagrafica,
                Codice = "RIPOSO",
                Descrizione = "Riposo",
                NumeroCoppie = 0,
                TimbratureTipo = OrarioTimbratureTipo.IntervalloOrario,
                Hh_Teo_MonteOre = new TimeOnly(0, 0)
            });

            // Profilo orario settimanale: DIURNO8H dal lunedi' al venerdi', RIPOSO sabato e domenica
            var profilo = await _par_ProfiloOrarioRepository.UpsertAsync(new Par_ProfiloOrario
            {
                IdAz_Anagrafica = IdAz_Anagrafica,
                Codice = CodiceProfiloOrarioDefault,
                Descrizione = "Settimanale 8H",
                NumGiorniCiclo = 7,
                TipoProfilo = TipoProfilo.Settimanale,
                EccedenzeAttive = true,
                TipoConteggio = StraoTipoConteggio.Giornaliero,
                StraoSogliaHHFullTime = new TimeOnly(8, 0),
                SogliaSettimanaleFT = 40,
                SupplLimitePerc = 25,
                EccArrotondamento = TimeRoundInterval.Min15,
                EccFranchigiaMin = 0,
                AssenzeContanoSoglia = false,
                StraoSoloAutorizzato = false,
                DomenicaFestiva = true,
                NotturnoDalle = new TimeOnly(22, 0),
                NotturnoAlle = new TimeOnly(6, 0),
                IdPar_Orario_Festivo = orarioRiposo.Id,
                IdCausale_Lavoro_Strao = causali["STRAO"].Id,
                IdCausale_Lavoro_StraoNott = causali["STRNOT"].Id,
                IdCausale_Lavoro_StraoFest = causali["STRFES"].Id,
                IdCausale_Lavoro_StraoFestNott = causali["STRFN"].Id,
                IdCausale_Lavoro_Suppl = causali["SUPPL"].Id,
                IdCausale_Lavoro_SupplEcc = causali["SUPEC"].Id,
                IdCausale_Lavoro_EccOrd = causali["ECCOR"].Id,
                IdCausale_BancaOre = causali["BOACC"].Id,
                IdGiustificativo_Assenza_Ingiust = giustificativi["ASS"].Id
            });

            for (int numGiorno = 1; numGiorno <= profilo.NumGiorniCiclo; numGiorno++)
            {
                await _par_ProfiloOrarioGGRepository.UpsertAsync(new Par_ProfiloOrarioGG
                {
                    IdPar_ProfiloOrario = profilo.Id,
                    NumGiorno = numGiorno,
                    ZOrder = 1,
                    IdPar_Orario = numGiorno <= 5 ? orario.Id : orarioRiposo.Id   // 1=Lun .. 6=Sab, 7=Dom
                });
            }

            // Festivita' nazionali (l'anno delle date ricorrenti non conta)
            foreach (var (descrizione, tipo, data) in new[]
            {
                ("Capodanno",                TipoFestivita.DataRicorrente, new DateTime(2000, 1, 1)),
                ("Epifania",                 TipoFestivita.DataRicorrente, new DateTime(2000, 1, 6)),
                ("Lunedi' dell'Angelo",      TipoFestivita.Pasquetta,      new DateTime(2000, 1, 1)),
                ("Festa della Liberazione",  TipoFestivita.DataRicorrente, new DateTime(2000, 4, 25)),
                ("Festa del Lavoro",         TipoFestivita.DataRicorrente, new DateTime(2000, 5, 1)),
                ("Festa della Repubblica",   TipoFestivita.DataRicorrente, new DateTime(2000, 6, 2)),
                ("Ferragosto",               TipoFestivita.DataRicorrente, new DateTime(2000, 8, 15)),
                ("Ognissanti",               TipoFestivita.DataRicorrente, new DateTime(2000, 11, 1)),
                ("Immacolata Concezione",    TipoFestivita.DataRicorrente, new DateTime(2000, 12, 8)),
                ("Natale",                   TipoFestivita.DataRicorrente, new DateTime(2000, 12, 25)),
                ("Santo Stefano",            TipoFestivita.DataRicorrente, new DateTime(2000, 12, 26)),
            })
            {
                await _par_FestivitaRepository.UpsertAsync(new Par_Festivita
                {
                    IdAz_Anagrafica = IdAz_Anagrafica,
                    Descrizione = descrizione,
                    TipoFestivita = tipo,
                    Data = data
                });
            }

            // Export
            var exportCau = await _par_ExportCauRepository.UpsertAsync(new Par_ExportCau
            {
                IdAz_Anagrafica = IdAz_Anagrafica,
                Codice = "0001",
                Descrizione = "ExportProva",
                TipoFile = Par_Export_TipoFile.CSV
            });

            // tutte le causali nel tracciato di prova, con il loro codice (senza causali l'export non produce file);
            // elaborazione e unita' come quando si aggiunge una causale dalla pagina del tracciato
            foreach (var causale in causali.Values)
            {
                await _par_ExportCau_CausaliRepository.UpsertAsync(new Par_ExportCau_Causali
                {
                    IdPar_ExportCau = exportCau.Id,
                    IdCausale = causale.Id,
                    Codice = causale.Codice,
                    TipoElaborazione = Par_Export_TipoElaborazione.Gionaliera,
                    TipoUnita = Par_Export_TipoUnita.Ore
                });
            }
        }

        // Profilo orario da assegnare ai nuovi rapporti di lavoro (quello di default, altrimenti il primo)
        public Par_ProfiloOrario? GetProfiloOrarioDefault(int IdAz_Anagrafica)
        {
            var profili = _par_ProfiloOrarioRepository.FindAll(x => x.IdAz_Anagrafica == IdAz_Anagrafica)
                                                      .OrderBy(x => x.Id)
                                                      .ToList();
            return profili.FirstOrDefault(x => x.Codice == CodiceProfiloOrarioDefault) ?? profili.FirstOrDefault();
        }
    }

    public interface IGestionePresenzeParametriDefault
    {
        Task InitParametri(int IdAz_Anagrafica);
        Par_ProfiloOrario? GetProfiloOrarioDefault(int IdAz_Anagrafica);
    }
}
