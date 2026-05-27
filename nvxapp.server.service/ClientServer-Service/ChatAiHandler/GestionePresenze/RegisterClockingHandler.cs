using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di una timbratura entrata/uscita.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome -> EmployeeId.
    public class RegisterClockingHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public RegisterClockingHandler(IDip_AnagraficaService dip_AnagraficaService,
                                       IDip_GG_TimbraturaService dip_GG_TimbraturaService
                                       ) : base(dip_AnagraficaService)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "RegisterClocking",
                DisplayName = "Timbratura",
                Description = @"registra una timbratura di entrata o uscita.",
                Keywords = new() { "timbratura", "timbra","timbrare" ,
                                       "entrata","entra", "uscita", "esce",
                                       "orario", "clocking"
                                    },
                Slots = BuildSlots(
                EmployeeNameSlot,
                new()
                {
                    Name = "time",
                    Type = "HH:mm",
                    Required = true,
                    PromptDescription = @"Orario nel formato HH:mm.
                                         Se l'utente dice 'alle 9' restituisci '09:00'.
                                         Accetta anche varianti come '9', '9.00', '9:0' e normalizzale.
                                         Se l'utente dice 'dieci' restituisci '10:00'.
                                         Se l'utente dice 'dieci e mezza' restituisci '10:30'.
                                         Non dedurre la risposta dagli esempi, al massimo puoi trasformare un
                                         un numero espresso in forma letterale trasformandolo un cifra numerica come nell' esempio fornito",
                    //PromptDescription = @"Orario nel formato HH:mm, se presente nel testo  ",
                    //PromptDescription = @"(HH:mm, obbligatorio)",

                    Question = "A che orario? (es. 09:00)",
                    Label = "Orario",
                    Validator = v =>
                    {
                        // 1. Controllo formato base HH:mm tramite TimeOnly.TryParse
                        if (TimeOnly.TryParse(v, out _))
                            return null;

                        // 2. Se non è valido, restituisco un errore dettagliato
                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            $"'{v}' non è un orario valido. Usa il formato HH:mm (es. 09:00).",
                            "time"
                        );
                    },
                    HasRelevantContent = msg =>
                    {
                        if (msg.Any(char.IsDigit)) return true;
                        var numericWords = new[]
                        {
                            "uno","due","tre","quattro","cinque","sei","sette","otto","nove","dieci",
                            "undici","dodici","tredici","quattordici","quindici","sedici","diciassette",
                            "diciotto","diciannove","venti","ventuno","ventidue","ventitre"
                        };
                        var retVal = numericWords.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        return retVal;
                    }
                },
                new()
                {
                    Name = "date",
                    Type = "yyyy-MM-dd",
                    Required = false,
                    Default = "oggi",
                    //PromptDescription = @"data nel formato yyyy-MM-dd. 
                    //                      Se dice 'oggi' normalizza alla data odierna.",

                    PromptDescription = @"(yyyy-MM-dd, opzionale)",
                    Question = "Per quale data?",
                    Label = "Data",
                    Validator = v =>
                    {
                        // 1. Verifica se la stringa può essere interpretata come data valida
                        if (DateOnly.TryParse(v, out _))
                            return null;

                        // 2. In caso di errore, restituisce un messaggio dettagliato
                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            $"'{v}' non è una data valida. Usa il formato gg/mm/aaaa.",
                            "date"
                        );
                    },
                    //HasRelevantContent = msg =>
                    //{
                    //    return true;
                    //}
                },
                new()
                {
                    Name = "direction",
                    Type = "IN/OUT",
                    Required = false,
                    Default = "IN",
                    PromptDescription = "valore IN oppure OUT. Se dice 'entrata/entra/inizia/inizio' restituisci IN, se dice 'uscita/esce/fine/finisce' restituisci OUT.",
                    //PromptDescription = "(IN/OUT, opzionale)",
                    Question = "Entrata o uscita?",
                    Label = "Tipo",
                    //Validator = v =>
                    //{
                    //        return null;
                    //},
                    HasRelevantContent = msg =>
                    {
                        if (msg.Any(char.IsDigit)) return true;
                        var Words = new[]
                        {
                            "IN","OUT"
                        };
                        var retVal = Words.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        return retVal;
                    }
                }
            )
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {

            var employeeName = slots.GetValueOrDefault("employeeName", "-");
            var time = slots.GetValueOrDefault("time", "-");
            var date = slots.GetValueOrDefault("date", DateTime.Today.ToString("dd/MM/yyyy"));
            var direction = slots.GetValueOrDefault("direction", "IN");
            var directionLabel = direction.Equals("OUT", StringComparison.OrdinalIgnoreCase) ? "uscita" : "entrata";

            //// Carica tutti i dipendenti
            //var req = new GenericRequest<Dip_Anagrafica_GetAll_InModel>();
            //var res = _dip_AnagraficaService.GetAll(req, true).Result;
            //if (!res.Success || res.Data == null)
            //{
            //}

            if (!string.IsNullOrEmpty(employeeName))
            {
                Dip_AnagraficaModel Dip_Anagrafica = Get_Dip_Anagrafica(employeeName);
                if (Dip_Anagrafica != null)
                {

                    //Dip_GG_TimbraturaPutInModel item = new Dip_GG_TimbraturaPutInModel(){ 
                    //                                                                        IdDip_RapportoLavoro = Dip_Anagrafica.Dip_RapportoLavoro[0].Id
                    //                                                                    };

                    // Costruisce il DateTime di timbratura combinando date + time dagli slot.
                    // date può essere "oggi" (default) oppure "yyyy-MM-dd"; time è sempre "HH:mm".
                    var dateSlot = slots.GetValueOrDefault("date", "oggi");
                    var timeSlot = slots.GetValueOrDefault("time", "00:00");

                    DateTime competenzaDate = dateSlot.Equals("oggi", StringComparison.OrdinalIgnoreCase)
                        ? DateTime.Today
                        : (DateOnly.TryParse(dateSlot, out var parsedDate)
                            ? parsedDate.ToDateTime(TimeOnly.MinValue)
                            : DateTime.Today);

                    DateTime timbraturaDateTime = TimeOnly.TryParse(timeSlot, out var parsedTime)
                        ? competenzaDate.Add(parsedTime.ToTimeSpan())
                        : competenzaDate;

                    TipoTimbratura tipoTimbratura = direction.Equals("OUT", StringComparison.OrdinalIgnoreCase)
                        ? TipoTimbratura.Uscita
                        : TipoTimbratura.Entrata;
                    tipoTimbratura = TipoTimbratura.SenzaVerso;

                    Dip_GG_TimbraturaModel Dip_GG_Timbratura = new Dip_GG_TimbraturaModel()
                    {
                        IdDip_RapportoLavoro      = Dip_Anagrafica.Dip_RapportoLavoro[0].Id,
                        Timbratura                = timbraturaDateTime,
                        TimbraturaOriginale       = timbraturaDateTime,
                        GiornoCompetenza          = competenzaDate.Date,
                        TimbraturaTipo            = tipoTimbratura,
                        RichiestaStato            = StatoRichiesta.Diretta,
                        IdAz_SubCommessaAttivita  = Dip_Anagrafica.Dip_RapportoLavoro[0].IdAz_SubCommessaAttivita
                    };
                    

                    var req_1 = new GenericRequest<Dip_GG_TimbraturaPutInModel>()
                    {
                        Data = new Dip_GG_TimbraturaPutInModel()
                        {
                            IdDip_RapportoLavoro = Dip_Anagrafica.Dip_RapportoLavoro[0].Id,
                            Dip_GG_Timbratura    = Dip_GG_Timbratura,
                            ExcludeRicalc        = false
                        }
                    };
                    var c = _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(req_1, true).Result;
                }
            }






            return Task.FromResult(CommandResult.Ok(
                $"Timbratura di {directionLabel} registrata: {employeeName} alle {time} del {date}."));
        }

    }
}
