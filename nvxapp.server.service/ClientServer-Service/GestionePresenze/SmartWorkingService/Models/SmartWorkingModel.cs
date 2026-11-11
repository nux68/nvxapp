using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService.Models
{
    // Smart working / lavoro agile (AT-SW, ANALISI_ATTENDANCETRACKING_SMARTWORKING.md)

    public class Dip_AccordoSWModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public string? IdAspNetUsers { get; set; }
        public string? Nominativo { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public int MaxGiorniSettimana { get; set; } = 2;
        public string GiorniConsentiti { get; set; } = "1111100";
        public string? Note { get; set; }
        public DateTime? DataRichiestaFirma { get; set; }
        public DateTime? DataFirma { get; set; }
        public bool Firmato { get; set; }
    }

    public class Dip_AccordoSW_GetAllInModel
    {
        // vuoto = tutti i dipendenti (HR)
        public string? IdAspNetUsers { get; set; }
    }
    public class Dip_AccordoSW_GetAllOutModel : ModelResult
    {
        public List<Dip_AccordoSWModel> Dip_AccordoSW { get; set; } = new();
        // dipendenti dell'azienda (per la scelta nella modifica)
        public List<TurnoDipendenteModel> Dipendenti { get; set; } = new();
    }

    public class Dip_AccordoSW_GetInModel { public int Id { get; set; } }
    public class Dip_AccordoSW_GetOutModel : ModelResult
    {
        public Dip_AccordoSWModel Dip_AccordoSW { get; set; } = new();
        public List<TurnoDipendenteModel> Dipendenti { get; set; } = new();
    }

    public class Dip_AccordoSW_PutInModel { public Dip_AccordoSWModel Dip_AccordoSW { get; set; } = new(); }
    public class Dip_AccordoSW_PutOutModel : ModelResult
    {
        public Dip_AccordoSWModel Dip_AccordoSW { get; set; } = new();
    }

    public class Dip_AccordoSW_DeleteInModel { public int Id { get; set; } }
    public class Dip_AccordoSW_DeleteOutModel : ModelResult { }

    public class LavoroAgileGiornoModel
    {
        public DateTime Data { get; set; }
        // giorno coperto da un accordo firmato e consentito dall'accordo
        public bool Consentito { get; set; }
        // giorno lavorativo (con ore teoriche)
        public bool Lavorativo { get; set; }
        // stato del giorno di lavoro agile (null = non richiesto)
        public StatoRichiesta? Stato { get; set; }
        public int? IdDip_GG_Richiesta { get; set; }
    }

    public class LavoroAgileSettimanaModel
    {
        public DateTime Lunedi { get; set; }
        public int Usati { get; set; }
        public int Max { get; set; }
    }

    public class Dip_AccordoSW_CalendarioInModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
    public class Dip_AccordoSW_CalendarioOutModel : ModelResult
    {
        public int IdPar_Giustificativi { get; set; }
        public List<Dip_AccordoSWModel> Accordi { get; set; } = new();
        public List<LavoroAgileGiornoModel> Giorni { get; set; } = new();
        public List<LavoroAgileSettimanaModel> Settimane { get; set; } = new();
    }
}
