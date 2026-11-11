using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    // anagrafica del dipendente (tabella comune Dip_Anagrafica): i campi che servono a Risorse Umane
    public class Hr_DipendenteModel
    {
        public int Id { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        public string? Email { get; set; }
        public DateTime? DataAssunzione { get; set; }
        public DateTime? DataCessazione { get; set; }
        public bool Attivo { get; set; } = true;
        public Hr_AccountModel Account { get; set; } = new();
    }

    public class Hr_DipendenteListInModel { public bool SoloAttivi { get; set; } }
    public class Hr_DipendenteListOutModel : ModelResult { public List<Hr_DipendenteModel> Dipendenti { get; set; } = new(); }
    public class Hr_DipendenteGetOutModel : ModelResult { public Hr_DipendenteModel Dipendente { get; set; } = new(); }
    public class Hr_DipendentePutInModel { public Hr_DipendenteModel Dipendente { get; set; } = new(); }
    public class Hr_DipendentePutOutModel : ModelResult { public Hr_DipendenteModel Dipendente { get; set; } = new(); }

    // HR-ONB.3: account del dipendente con i servizi esistenti degli utenti dell'azienda (Account/UserCompanyPut)
    public class Hr_AccountCreaInModel
    {
        public int IdDip_Anagrafica { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Mail { get; set; }
        public string Password { get; set; } = string.Empty;
        // voce di checklist da completare insieme (facoltativa)
        public int? IdHr_ChecklistVoce { get; set; }
    }

    public class Hr_AccountDisattivaInModel
    {
        public int IdDip_Anagrafica { get; set; }
        public int? IdHr_ChecklistVoce { get; set; }
    }

    public class Hr_AccountOutModel : ModelResult { public Hr_AccountModel Account { get; set; } = new(); }
}
