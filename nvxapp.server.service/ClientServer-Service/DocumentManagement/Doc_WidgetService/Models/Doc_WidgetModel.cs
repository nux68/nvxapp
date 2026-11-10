using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_WidgetService.Models
{
    // spazio occupato per tipo documento (widget "Spazio per tipo documento", ANALISI_WIDGET_DOCUMENTALE.md)
    public class Doc_SpazioPerTipoGetInModel
    {
    }

    public class Doc_SpazioPerTipoModel
    {
        public int IdDoc_Par_Tipo { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        // byte di tutte le versioni dei documenti del tipo, cestino escluso
        public long Bytes { get; set; }
        public int Documenti { get; set; }
    }

    public class Doc_SpazioPerTipoGetOutModel : ModelResult
    {
        // tipi con almeno un byte, dal piu' grande
        public List<Doc_SpazioPerTipoModel> Tipi { get; set; } = new List<Doc_SpazioPerTipoModel>();
        // byte dei documenti nel cestino (contano nello spazio finche' il cestino non si svuota)
        public long CestinoBytes { get; set; }
        // totale = somma dei tipi + cestino (coincide con lo spazio occupato dopo il ricalcolo notturno)
        public long TotaleBytes { get; set; }
    }
}
