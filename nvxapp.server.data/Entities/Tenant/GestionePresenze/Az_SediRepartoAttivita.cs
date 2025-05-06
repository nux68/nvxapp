using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_SediRepartoAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }






        // Relazione ricorsiva: Reparti figli
        //public ICollection<Az_Reparto>? Az_Reparto_Sub { get; set; }
        //public ICollection<Par_OrarioIntervalloHH>? Par_ProfiloOrarioIntervalloHH { get; set; }

    }



}
