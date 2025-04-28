using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_ProfiloOrarioGG : BaseEntity
    {

        [Required]
        public required int IdPar_ProfiloOrario { get; set; }
        [ForeignKey("IdPar_ProfiloOrario")]
        public virtual Par_ProfiloOrario? Par_ProfiloOrarioNavigation { get; set; }



        public ICollection<Par_Orario>? Par_Orario { get; set; }




    }





}
