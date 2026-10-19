using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_NotaSpesa : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        
        
        public StatoRichiesta RichiestaStato { get; set; }
        //TODO UNIFORMARE
        public int? IdDip_GG_Richiesta { get; set; }
        [ForeignKey("IdDip_GG_Richiesta")]
        public virtual Dip_GG_Richiesta? Dip_GG_RichiestaNavigation { get; set; }

        // spesa: tipo, importo e descrizione; i documenti sono in Dip_GG_NotaSpesa_Allegato
        [Required]
        public required int IdPar_TipoSpesa { get; set; }
        [ForeignKey("IdPar_TipoSpesa")]
        public virtual Par_TipoSpesa? Par_TipoSpesaNavigation { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal Importo { get; set; }

        [MaxLength(200)]
        public string? Descrizione { get; set; }

        public ICollection<Dip_GG_NotaSpesa_Allegato>? Dip_GG_NotaSpesa_Allegato { get; set; }

    }
}