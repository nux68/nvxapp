using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.HumanResources
{
    /*
     Risorse Umane (ANALISI_HUMANRESOURCES_APPLICATIVO.md), schema tenant_<IdAzienda>_5 (o public in modalita' singola). Tabelle con prefisso Hr_.
     Le tabelle comuni (Az_Anagrafica...) stanno nello schema Common e sono mappate in sola lettura in HumanResourcesDbContext.
    */

    // configurazione del modulo per l'azienda (una riga per Az_Anagrafica): la crea l'attivazione
    public class Hr_Cfg : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        // giorni di preavviso predefiniti delle scadenze che il modulo registra nello scadenzario comune
        public int GiorniPreavviso { get; set; } = 30;

        // il modulo invia notifiche e promemoria (servizio comune delle notifiche, preferenze per utente)
        public bool NotificheAttive { get; set; } = true;

        // note interne della configurazione
        [MaxLength(2000)]
        public string? Note { get; set; }
    }
}
