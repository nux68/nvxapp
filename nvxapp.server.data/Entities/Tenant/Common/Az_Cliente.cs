using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Cliente (o fornitore) dell'azienda (schema Common): condiviso tra gli applicativi (presenze, CRM, documentale).
 Il cliente predefinito delle presenze non sta qui ma in Az_Cfg.IdAz_ClienteDefault (dato delle sole presenze).
     Regola: qui solo dati validi per tutti; i dati di un solo applicativo vanno in una sua tabella
     di estensione 1-1 (es. Crm_ClienteDati), nel suo schema.
    */
    public class Az_Cliente : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        // ragione sociale
        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        // Cliente / Fornitore / Entrambi: le scelte "cliente" (commesse, opportunita') propongono Cliente ed Entrambi
        public TipoCliente Tipo { get; set; } = TipoCliente.Cliente;

        // disattivato: resta nello storico (commesse, opportunita', documenti) ma non si propone piu' nelle scelte
        public Boolean Attivo { get; set; } = true;

        // dati fiscali
        [MaxLength(11)]
        public string? PartitaIva { get; set; }
        [MaxLength(16)]
        public string? CodiceFiscale { get; set; }

        // sede
        [MaxLength(100)]
        public string? Indirizzo { get; set; }
        [MaxLength(10)]
        public string? Cap { get; set; }
        [MaxLength(50)]
        public string? Citta { get; set; }
        [MaxLength(2)]
        public string? Provincia { get; set; }
        [MaxLength(2)]
        public string? Nazione { get; set; }

        // contatti
        [MaxLength(30)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(100)]
        public string? Pec { get; set; }
        [MaxLength(100)]
        public string? Referente { get; set; }

        // fatturazione elettronica
        [MaxLength(7)]
        public string? CodiceDestinatario { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public ICollection<Az_ClienteContatto>? Az_ClienteContatto { get; set; }
    }

    // i valori finiscono nel database: non vanno cambiati
    public enum TipoCliente
    {
        Cliente = 0,
        Fornitore = 1,
        Entrambi = 2,
    }
}
