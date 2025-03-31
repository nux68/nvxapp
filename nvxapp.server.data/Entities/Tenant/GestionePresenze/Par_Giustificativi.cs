using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_Giustificativi : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }


        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }

    }



}
