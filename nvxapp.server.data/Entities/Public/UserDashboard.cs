using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Dashboard di un utente in un'azienda (ANALISI_WIDGET.md): i widget scelti, nell'ordine in cui compaiono.
        - IdAspNetUsers: utente
        - IdCompany    : azienda in cui l'utente lavora (la stessa persona puo' avere dashboard diverse per azienda)
        - Widgets      : chiavi dei widget in formato JSON (es. ["infrastructure.askAssistant","documents.storage"])
     Il server non conosce il catalogo dei widget (lo dichiara il client di ogni ramo): salva le chiavi cosi' come
     arrivano, dopo il controllo del formato. Nessuna riga = l'utente non ha ancora configurato: valgono i widget
     predefiniti del catalogo. Una riga con elenco vuoto = dashboard volutamente vuota.
     Senza navigation: i contesti degli applicativi mappano Company e AspNetUsers e non devono scoprire la tabella.
    */
    public class UserDashboard : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [Required]
        public int IdCompany { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Widgets { get; set; } = "[]";
    }
}
