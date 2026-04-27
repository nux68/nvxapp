using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_SubCommessaAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_SubCommessa { get; set; }
        [ForeignKey("IdAz_SubCommessa")]
        public virtual Az_SubCommessa? Az_SubCommessaNavigation { get; set; }

        [Required]
        public required int IdPar_Attivita { get; set; }
        [ForeignKey("IdPar_Attivita")]
        public virtual Par_Attivita? Par_AttivitaNavigation { get; set; }
        public Boolean Default { get; set; }


        public ICollection<Dip_GG_Timbratura>? Dip_GG_Timbratura { get; set; }

        public ICollection<Par_Orario>? Par_Orario { get; set; }
        public ICollection<Par_OrarioIntervalloHH>? Par_OrarioIntervalloHH { get; set; }

    }
}
