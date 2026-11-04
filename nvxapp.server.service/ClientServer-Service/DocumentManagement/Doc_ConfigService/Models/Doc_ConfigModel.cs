using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_ConfigService.Models
{
    // cosa puo' fare l'utente corrente nel documentale (il client mostra le funzioni di conseguenza)
    public class Doc_PermessiModel
    {
        public bool PowerAdmin { get; set; }
        public bool AmministraArchivio { get; set; }
        public bool VedePersonale { get; set; }
        public bool CaricaPersonale { get; set; }
        public bool GestisceAbilitazioni { get; set; }
        public bool Dipendente { get; set; }
    }

    public class Doc_CfgModel
    {
        public int GiorniCestino { get; set; } = 30;
        public int VersioniDaConservare { get; set; }
        public bool NotificheEmail { get; set; } = true;
    }

    public class Doc_Par_TipoModel
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Doc_Categoria Categoria { get; set; }
        public int? AnniConservazione { get; set; }
        public bool HaScadenza { get; set; }
        public bool PresaVisioneRichiesta { get; set; }
        public bool HaCompetenza { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; } = true;
        public int Documenti { get; set; }
    }

    public class Doc_CartellaModel
    {
        public int Id { get; set; }
        public int? IdDoc_Cartella { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        // percorso completo (es. "Personale / Contratti") per le scelte
        public string Percorso { get; set; } = string.Empty;
        public int Ordine { get; set; }
        public bool VisibileUtenti { get; set; }
        public bool DiSistema { get; set; }
        public int Documenti { get; set; }
    }

    public class Doc_AbilitazioneModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Ruoli { get; set; } = string.Empty;
        public bool VedePersonale { get; set; }
        public bool CaricaPersonale { get; set; }
    }

    // dipendente per le scelte (destinatario dei documenti del personale)
    public class Doc_DipendenteModel
    {
        public int Id { get; set; }
        public string Nominativo { get; set; } = string.Empty;
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? CodiceFiscale { get; set; }
        public string? Matricola { get; set; }
        public string? Email { get; set; }
        public DateTime? DataCessazione { get; set; }
        public bool Attivo { get; set; } = true;
        public bool HaUtente { get; set; }
        // nome utente dell'account collegato (accesso all'app)
        public string? UserName { get; set; }
    }

    public class Doc_DipendentePutInModel { public Doc_DipendenteModel Dipendente { get; set; } = new Doc_DipendenteModel(); }
    public class Doc_DipendentePutOutModel : ModelResult { public Doc_DipendenteModel Dipendente { get; set; } = new Doc_DipendenteModel(); }

    // ---------------------------------------------------------------- richieste e risposte
    public class Doc_SetupGetInModel { }
    public class Doc_SetupGetOutModel : ModelResult
    {
        public Doc_PermessiModel Permessi { get; set; } = new Doc_PermessiModel();
        public Doc_CfgModel Cfg { get; set; } = new Doc_CfgModel();
        public List<Doc_Par_TipoModel> Tipi { get; set; } = new List<Doc_Par_TipoModel>();
        public List<Doc_CartellaModel> Cartelle { get; set; } = new List<Doc_CartellaModel>();
        // dipendenti: solo per chi vede i documenti del personale
        public List<Doc_DipendenteModel> Dipendenti { get; set; } = new List<Doc_DipendenteModel>();
    }

    public class Doc_CfgPutInModel { public Doc_CfgModel Cfg { get; set; } = new Doc_CfgModel(); }
    public class Doc_CfgPutOutModel : ModelResult { public Doc_CfgModel Cfg { get; set; } = new Doc_CfgModel(); }

    public class Doc_TipoPutInModel { public Doc_Par_TipoModel Tipo { get; set; } = new Doc_Par_TipoModel(); }
    public class Doc_TipoPutOutModel : ModelResult { public Doc_Par_TipoModel Tipo { get; set; } = new Doc_Par_TipoModel(); }

    public class Doc_CartellaPutInModel { public Doc_CartellaModel Cartella { get; set; } = new Doc_CartellaModel(); }
    public class Doc_CartellaPutOutModel : ModelResult { public Doc_CartellaModel Cartella { get; set; } = new Doc_CartellaModel(); }

    public class Doc_DeleteInModel { public int Id { get; set; } }
    public class Doc_DeleteOutModel : ModelResult
    {
        // false = in uso, solo disattivato (tipologie)
        public bool Deleted { get; set; }
    }

    public class Doc_AbilitazioneListInModel { }
    public class Doc_AbilitazioneListOutModel : ModelResult
    {
        public List<Doc_AbilitazioneModel> Abilitazioni { get; set; } = new List<Doc_AbilitazioneModel>();
    }

    public class Doc_AbilitazionePutInModel { public Doc_AbilitazioneModel Abilitazione { get; set; } = new Doc_AbilitazioneModel(); }
    public class Doc_AbilitazionePutOutModel : ModelResult { public Doc_AbilitazioneModel Abilitazione { get; set; } = new Doc_AbilitazioneModel(); }
}
