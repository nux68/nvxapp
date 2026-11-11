using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    // ------------------------------------------------------------------------------------------------ comuni
    public class Hr_EmptyInModel { }

    // cosa puo' fare l'utente corrente (il server lo ricontrolla in ogni servizio)
    public class Hr_PermessiModel
    {
        public bool PowerAdmin { get; set; }
        public bool Admin { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ configurazione
    public class Hr_CfgModel
    {
        public int GiorniPreavviso { get; set; }
        public bool NotificheAttive { get; set; }
        public string? Note { get; set; }
    }

    public class Hr_CfgGetOutModel : ModelResult
    {
        public Hr_PermessiModel Permessi { get; set; } = new();
        public Hr_CfgModel Cfg { get; set; } = new();
    }

    public class Hr_CfgPutInModel { public Hr_CfgModel Cfg { get; set; } = new(); }
    public class Hr_CfgPutOutModel : ModelResult { public Hr_CfgModel Cfg { get; set; } = new(); }
}
