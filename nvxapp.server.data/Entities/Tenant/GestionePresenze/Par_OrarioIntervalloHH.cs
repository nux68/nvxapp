using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_OrarioIntervalloHH : BaseEntity
    {

        [Required]
        public required int IdPar_Orario { get; set; }
        public Par_Orario? Par_OrarioNavigation { get; set; }

        public TimeOnly? Dalle { get; set; }
        public TimeOnly? Alle { get; set; }

        

    }





}
