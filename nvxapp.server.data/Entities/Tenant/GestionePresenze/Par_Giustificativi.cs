using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_Giustificativi : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }


        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [MaxLength(7)]
        public string? BackgroundColor { get; set; }
        [MaxLength(7)]
        public string? TextColor { get; set; }

        public JustTipoInput TipoInput { get; set; }

        // Importo: si inserisce un solo importo (Dip_GG_Giustificativi.Importo) al posto delle ore; la causale collegata
        // deve essere dello stesso tipo e non ha effetti su ore, contatori, banca ore e piano ferie
        public TipoValore Tipo { get; set; } = TipoValore.Orario;

        
        public int? IdCausale { get; set; }
        [ForeignKey("IdCausale")]
        public virtual Par_Causali? Causale_Navigation { get; set; }


        public SignWithNeutral Segno { get; set; }

        public Boolean VisualizzaInPianoFerie { get; set; }

        public TipoContatore TipoContatore { get; set; }

        // recupero di banca ore: il maturato del contatore sono le ore accreditate nei mesi approvati
        public Boolean BancaOre { get; set; }

        // autorizzazione alle ore in piu': le ore del giustificativo approvato sono il massimo di ore in piu'
        // riconosciute nel giorno (profili con "solo se autorizzate"); non cambia il dovuto e non ha causale
        public Boolean AutorizzaStraordinario { get; set; }

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }
        public ICollection<Dip_Rapporto_Giustificativi_Maturazione>? Dip_Rapporto_Giustificativi_Maturazione { get; set; }

    }

    public enum JustTipoInput
    {
        InteraGiornate,
        Intervallo,
        Tutti
    }

    public enum TipoContatore
    {
        NoContatore = 0,
        Contatore = 1,
        ContatoreConAvviso = 2,
        ContatoreConBlocco = 3
    }
}
