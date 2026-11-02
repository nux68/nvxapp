using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models
{
    /*
     Elemento da eliminare:
        - Dealer / FinancialAdvisor / Company: Id del nodo
        - UserDealer / UserFinancialAdvisor / UserCompany: Id del collegamento (l'utente si elimina se non ha altri collegamenti)
        - User: IdAspNetUsers (utente con tutti i collegamenti, solo SuperUser)
    */
    public class DeletionTargetModel
    {
        public DeletionNodeType NodeType { get; set; }
        public int? Id { get; set; }
        public string? IdAspNetUsers { get; set; }
    }

    // riga di riepilogo: descrizione gia' nella lingua dell'utente e conteggio
    public class DeletionCountModel
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }


    // anteprima: cosa verra' eliminato, senza eliminare nulla
    public class DeletionPreviewInModel
    {
        public DeletionTargetModel Target { get; set; } = new DeletionTargetModel();
    }

    public class DeletionPreviewOutModel : ModelResult
    {
        // nome da digitare per confermare (centro, studio, azienda o nome utente)
        public string Descrizione { get; set; } = string.Empty;
        public List<DeletionCountModel> Counts { get; set; } = new List<DeletionCountModel>();
        // utenti che perdono il collegamento ma restano, perche' collegati altrove
        public List<string> UsersKept { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        // false: l'eliminazione non e' possibile (BlockReason spiega perche')
        public bool CanDelete { get; set; }
        public string? BlockReason { get; set; }
    }


    // esecuzione: ConfirmText deve coincidere con il nome dell'anteprima
    public class DeletionExecuteInModel
    {
        public DeletionTargetModel Target { get; set; } = new DeletionTargetModel();
        public string ConfirmText { get; set; } = string.Empty;
    }

    public class DeletionExecuteOutModel : ModelResult
    {
        public List<DeletionCountModel> Counts { get; set; } = new List<DeletionCountModel>();
    }
}
