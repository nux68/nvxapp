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

    public class Par_ProfiloOrarioGG : BaseEntity
    {

        [Required]
        public required int IdPar_ProfiloOrario { get; set; }
        public Par_ProfiloOrario? Par_ProfiloOrarioNavigation { get; set; }

        

        public ICollection<Par_ProfiloOrarioIntervalloHH>? Par_ProfiloOrarioIntervalloHH { get; set; }




    }





}
