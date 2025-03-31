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

    public class Par_ProfiloOrarioIntervalloHH : BaseEntity
    {

        [Required]
        public required int IdPar_ProfiloOrarioGG { get; set; }
        public Par_ProfiloOrarioGG? Par_ProfiloOrarioGGNavigation { get; set; }

        public TimeOnly? Dalle { get; set; }
        public TimeOnly? Alle { get; set; }


        public required int IdAz_RepartoAttivita { get; set; }
        public Az_RepartoAttivita? Az_RepartoAttivitaNavigation { get; set; }

    }





}
