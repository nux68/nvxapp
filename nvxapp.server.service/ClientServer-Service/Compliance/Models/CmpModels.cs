using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Compliance.Models
{
    // ------------------------------------------------------------------------------------------------ comuni
    public class Cmp_EmptyInModel { }

    // cosa puo' fare l'utente corrente (il server lo ricontrolla in ogni servizio)
    public class Cmp_PermessiModel
    {
        public bool PowerAdmin { get; set; }
        public bool Admin { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ configurazione
    public class Cmp_CfgModel
    {
        public int GiorniPreavviso { get; set; }
        public bool NotificheAttive { get; set; }
        public string? Note { get; set; }
        // CMP-81.2: utenti abilitati ai dati sanitari delle visite
        public List<string> UtentiDatiSanitari { get; set; } = new();
    }

    public class Cmp_UtenteModel
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

    public class Cmp_CfgGetOutModel : ModelResult
    {
        public Cmp_PermessiModel Permessi { get; set; } = new();
        public Cmp_CfgModel Cfg { get; set; } = new();
        // utenti dell'azienda (scelta degli abilitati ai dati sanitari)
        public List<Cmp_UtenteModel> Utenti { get; set; } = new();
    }

    public class Cmp_CfgPutInModel { public Cmp_CfgModel Cfg { get; set; } = new(); }
    public class Cmp_CfgPutOutModel : ModelResult { public Cmp_CfgModel Cfg { get; set; } = new(); }
}
