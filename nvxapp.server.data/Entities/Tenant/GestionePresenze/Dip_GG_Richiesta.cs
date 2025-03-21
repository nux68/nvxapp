using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Richiesta : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        public DateTime DataA { get; set; }

        public TipoRichiesta RichiestaTipo { get; set; }
        public StatoRichiesta RichiestaStato { get; set; }

        // Campo per oggetto JSON
        public string? Dati { get; set; }

        // Campo per oggetto JSON
        public string? CronologiaApprovazione { get; set; }

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }
        public ICollection<Dip_GG_Timbratura>? Dip_GG_Timbratura { get; set; }
        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }


    public enum TipoRichiesta
    {
        Timbratura,
        Giustificativo,
        NotaSpesa
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
