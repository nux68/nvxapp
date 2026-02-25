using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    
    //[Table("Par_OrarioIntervalloHH")]
    public class Par_OrarioIntervalloHH : BaseEntity
    {

        [Required]
        public required int IdPar_Orario { get; set; }
        [ForeignKey("IdPar_Orario")]
        public virtual Par_Orario? Par_OrarioNavigation { get; set; }

        public TimeOnly? Dalle { get; set; }
        public TimeOnly? Alle { get; set; }

        public TimeOnly? Dalle_Limite_SX { get; set; }
        public TimeOnly? Dalle_Limite_DX { get; set; }
        public TimeOnly? Alle_Limite_SX { get; set; }
        public TimeOnly? Alle_Limite_DX { get; set; }


        public int NumCoppia { get; set; }  = 1; // relazione con Par_Orario->NumeroCoppie
        
        

    }





}
