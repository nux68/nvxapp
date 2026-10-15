using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    /*
     Calendario delle festivita' dell'azienda.
       - DataRicorrente: stesso giorno e mese ogni anno (es. 25/12), l'anno di Data e' ignorato
       - DataSingola:    solo nel giorno indicato (es. patrono spostato, ponte aziendale)
       - Pasquetta:      lunedi' dopo Pasqua, calcolato ogni anno (Data ignorata)
     Nei giorni festivi il motore applica l'orario festivo del profilo e qualifica come festive
     le ore di straordinario.
    */
    public class Par_Festivita : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public TipoFestivita TipoFestivita { get; set; } = TipoFestivita.DataRicorrente;

        public DateTime Data { get; set; }
    }

    public enum TipoFestivita
    {
        DataRicorrente,
        DataSingola,
        Pasquetta
    }
}
