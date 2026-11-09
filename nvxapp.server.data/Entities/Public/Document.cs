using nvxapp.server.data.Infrastructure.Tenancy;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    // uso del documento: i valori finiscono nel database, non vanno cambiati
    public enum DocumentSection
    {
        TicketAi = 0,        // documentazione per le risposte automatiche AI ai ticket
    }

    /*
     Documento caricato a un livello della gerarchia (vedi nvxapp.server/docs/ANALISI_DOCUMENTAZIONE_AI.md [branch DocumentManagement]).
        - Section          : a cosa serve il documento
        - Level            : SuperUser, Dealer, FinancialAdvisor o Company
        - IdDealer / IdFinancialAdvisor / IdCompany: nodo proprietario, solo quello del livello
                             (tutti null per il SuperUser; check constraint nel contesto)
        - Contenuto        : testo del documento, scritto o estratto dal file; e' quello che si passa all'AI
        - FileName / ContentType / Size / File: file originale facoltativo (si legge solo dal download)
        - ApplicationType  : applicativo a cui si riferisce (null = Generale)
        - IncludeSubLevels : il documento vale anche per i nodi sotto (es. il manuale caricato dal SuperUser)
        - Ordine           : priorita' quando i documenti superano il testo massimo per la chiamata AI
     Nessuna relazione verso Dealer, FinancialAdvisor e Company: i contesti degli applicativi mappano Company
     e scoprirebbero la tabella seguendo le relazioni (le chiavi esterne sono definite nel contesto).
    */
    public class Document : BaseEntity
    {
        public DocumentSection Section { get; set; }

        public TicketLevel Level { get; set; }

        public int? IdDealer { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        public int? IdCompany { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descrizione { get; set; }

        [Required]
        public string Contenuto { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long Size { get; set; }

        public byte[]? File { get; set; }

        public ApplicationType? ApplicationType { get; set; }

        public bool IncludeSubLevels { get; set; }

        public int Ordine { get; set; }

        public bool Active { get; set; }

        public ICollection<DocumentTicketType>? DocumentTicketType { get; set; }
    }
}
