using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_RepartoAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_Reparto { get; set; }
        public Az_Reparto? Az_RepartoNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }






        // Relazione ricorsiva: Reparti figli
        public ICollection<Az_Reparto>? Az_Reparto_Sub { get; set; }
        public ICollection<Par_ProfiloOrarioIntervalloHH>? Par_ProfiloOrarioIntervalloHH { get; set; }

    }



}
