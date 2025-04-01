using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

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

        public ICollection<Par_OrarioIntervalloHH>? Par_OrarioIntervalloHH { get; set; }

        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }


    }





}
