using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    /*
     in questa tabella se definisce il comportamento di default degli arrotondamenti
     se si volesse procedere a delle personalizzazioni, queste andranno fatte
     serializzando n righe di questa tabella nel apposito campo dell'orario (ancora non definito)
     se presenti queste righe, si andrà a sovrascrivere il comportamento di dafault
     */

    public class Par_Arrotondamenti : BaseEntity
    {

        [Required]
        public required string IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public TimeOnly? Dalle { get; set; }
        public TimeOnly? Alle { get; set; }

        public TipoArrotondamento? ArrotondamentoTipo { get; set; }
        public ValoreArrotondamento? ArrotondamentoValore { get; set; }
        public VersoArrotondamento? ArrotondamentoVerso { get; set; }


        

    }


    public enum TipoArrotondamento
    {
        Timbratura,
        SommatoriaOre
    }
    public enum ValoreArrotondamento
    {
        Minuto_1,
        Minuto_5,
        Minuto_10,
        Minuto_15,
        Minuto_30,
        Minuto_60,
    }
    public enum VersoArrotondamento
    {
        Somma,
        Sottrazione
    }


}
