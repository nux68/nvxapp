using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Compliance.Models
{
    // ------------------------------------------------------------------------------------------------ widget della dashboard (ANALISI_COMPLIANCE_WIDGET.md)

    /// <summary>scadenze aperte di un tipo (fonte dello scadenzario senza il prefisso "cmp.", es. "formazione")</summary>
    public class Cmp_WidgetScadenzaAreaModel
    {
        public string Fonte { get; set; } = string.Empty;
        public int Scadute { get; set; }
        public int InScadenza { get; set; }
    }

    public class Cmp_WidgetScadenzeGetOutModel : ModelResult
    {
        public List<Cmp_WidgetScadenzaAreaModel> Aree { get; set; } = new();
        public int Scadute { get; set; }
        public int InScadenza { get; set; }
        public int GiorniPreavviso { get; set; }
    }

    public class Cmp_WidgetGdprGetOutModel : ModelResult
    {
        public int TrattamentiAttivi { get; set; }
        public int TrattamentiDatiParticolari { get; set; }
        public int ResponsabiliScaduti { get; set; }
        public int ResponsabiliInScadenza { get; set; }
        /// <summary>violazioni da notificare al Garante (rischio > 0, non ancora notificate)</summary>
        public int ViolazioniDaNotificare { get; set; }
        /// <summary>di queste, oltre il termine delle 72 ore dalla scoperta</summary>
        public int ViolazioniOltreTermine { get; set; }
        /// <summary>violazioni scoperte negli ultimi 12 mesi</summary>
        public int Violazioni12Mesi { get; set; }
    }
}
