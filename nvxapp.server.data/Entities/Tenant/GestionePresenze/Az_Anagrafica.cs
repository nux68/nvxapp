using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    /*
     dati anagrafici
     */
    public class Az_Anagrafica : BaseEntity
    {
        [Required]
        public required string IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        public ICollection<Az_Sedi>? Az_Sedi { get; set; }

        public ICollection<Par_Causali>? Par_Causali { get; set; }
        public ICollection<Par_Giustificativi>? Par_Giustificativi { get; set; }

    }



}
