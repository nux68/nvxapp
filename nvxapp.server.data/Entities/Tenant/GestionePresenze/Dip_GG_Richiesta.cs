using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Richiesta : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        public DateTime DataA { get; set; }

        public TipoRichiesta RichiestaTipo { get; set; }
        // Campo per oggetto JSON
        public required string Dati { get; set; } = string.Empty;   // contiene oggetti di tipo  Dip_GG_Richiesta_Body_Timbratura,Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Body_NotaSpesa


        public StatoRichiesta RichiestaStato { get; set; }
        public List<Dip_GG_Richiesta_Stato_Cronology> RichiestaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();


        // servono per gestire l'annullamento di una richiesta approvata
        public StatoRichiesta? RevocaStato { get; set; }
        public List<Dip_GG_Richiesta_Stato_Cronology> RevocaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();
        

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }
        public ICollection<Dip_GG_Timbratura>? Dip_GG_Timbratura { get; set; }
        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }

    public class Dip_GG_Richiesta_Stato_Cronology
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public StatoRichiesta RichiestaStato { get; set; }
        public DateTime Data { get; set; }
    }


    public enum TipoRichiesta
    {
        Timbratura,
        Giustificativo,
        NotaSpesa,
        ApprovazioneStraordinario
    }

    public enum StatoRichiesta
    {
        Diretta,

        Immessa,
        Cancellata,
        Rifiutata,
        ApprovazioneInCorso,
        ParzialmenteApprovata,
        
        Approvata,
    }

    


}
