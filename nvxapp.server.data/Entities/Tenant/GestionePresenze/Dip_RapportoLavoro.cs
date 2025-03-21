using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    /*
     dati del rapporto di lavoro
     agganciare tutte le tabelle con dati sensibili al giorno a questa tabella
     */
    public class Dip_RapportoLavoro : BaseEntity
    {
        [Required]
        public required string IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public DateTime? DataAss { get; set; } // assunzione
        public DateTime? DataLic { get; set; } // lic


        public ICollection<Dip_GG_Richiesta>? Dip_GG_Richiesta { get; set; }
        public ICollection<Dip_GG_Timbratura>? Dip_GG_Timbratura { get; set; }
        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }
        public ICollection<Dip_GG_Causali>? Dip_GG_Causali { get; set; }
    }

}
