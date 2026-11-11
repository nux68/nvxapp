using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Assets.Models
{
    // ------------------------------------------------------------------------------------------------ comuni
    public class Ast_EmptyInModel { }

    // cosa puo' fare l'utente corrente (il server lo ricontrolla in ogni servizio)
    public class Ast_PermessiModel
    {
        public bool PowerAdmin { get; set; }
        public bool Admin { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ configurazione
    public class Ast_CfgModel
    {
        public int GiorniPreavviso { get; set; }
        public bool NotificheAttive { get; set; }
        public int PrenotazioneDurataMaxOre { get; set; }
        public int PrenotazioneAnticipoMaxGiorni { get; set; }
        public string? Note { get; set; }
    }

    public class Ast_CfgGetOutModel : ModelResult
    {
        public Ast_PermessiModel Permessi { get; set; } = new();
        public Ast_CfgModel Cfg { get; set; } = new();
    }

    public class Ast_CfgPutInModel { public Ast_CfgModel Cfg { get; set; } = new(); }
    public class Ast_CfgPutOutModel : ModelResult { public Ast_CfgModel Cfg { get; set; } = new(); }
}
