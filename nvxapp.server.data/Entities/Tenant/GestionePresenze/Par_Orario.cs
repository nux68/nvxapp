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

    public class Par_Orario : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        

    }


    


}
