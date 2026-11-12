using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Assets.Models
{
    // ------------------------------------------------------------------------------------------------ widget della dashboard (ANALISI_ASSETS_WIDGET.md)

    // scadenza della flotta mostrata nel widget (scadenza del veicolo o del contratto di noleggio/leasing)
    public class Ast_WidgetScadenzaModel
    {
        public int IdAst_Veicolo { get; set; }
        public string Targa { get; set; } = string.Empty;
        public string Veicolo { get; set; } = string.Empty;
        /// <summary>bollo, assicurazione, revisione... o "Contratto di noleggio"</summary>
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataScadenza { get; set; }
        public bool Scaduta { get; set; }
        public bool Contratto { get; set; }
    }

    public class Ast_WidgetFlottaOutModel : ModelResult
    {
        /// <summary>finestra delle scadenze in arrivo (Ast_Cfg.GiorniPreavviso)</summary>
        public int GiorniPreavviso { get; set; }
        public int VeicoliAttivi { get; set; }
        /// <summary>scadenze non eseguite gia' passate</summary>
        public int Scadute { get; set; }
        /// <summary>scadenze (veicolo e contratti) da oggi entro il preavviso</summary>
        public int InArrivo { get; set; }
        /// <summary>multe registrate e non ancora notificate al conducente</summary>
        public int MulteDaNotificare { get; set; }
        /// <summary>le prime scadenze in ordine di data (scadute prima)</summary>
        public List<Ast_WidgetScadenzaModel> Scadenze { get; set; } = new();
    }
}
