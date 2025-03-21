using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_Sedi : BaseEntity
    {
        [Required]
        public required string IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public ICollection<Az_Reparto>? Az_Reparto { get; set; }
    }



}
