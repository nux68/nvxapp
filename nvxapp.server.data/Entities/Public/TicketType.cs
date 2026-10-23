using nvxapp.server.data.Infrastructure.Tenancy;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Tipo di richiesta dei ticket di assistenza, configurato dal SuperUser.
        - ApplicationType: applicativo a cui si riferisce (null = Generale)
        - Codice         : univoco
        - Descrizione    : una sola, mostrata uguale in tutte le lingue (come gli altri dati delle tabelle)
        - AiResolution   : a ogni passaggio verso l'alto si tenta la risoluzione automatica AI
        - Ordine         : ordinamento nelle liste
        - Active         : i tipi disattivati non si propongono in apertura ma restano sui ticket esistenti
     Livelli in cui il tipo e' abilitato in TicketTypeLevel.
    */
    public class TicketType : BaseEntity
    {
        public ApplicationType? ApplicationType { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codice { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public bool AiResolution { get; set; }

        public int Ordine { get; set; }

        public bool Active { get; set; }

        public ICollection<TicketTypeLevel>? TicketTypeLevel { get; set; }
    }
}
