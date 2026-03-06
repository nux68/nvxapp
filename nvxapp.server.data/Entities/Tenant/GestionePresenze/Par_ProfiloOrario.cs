using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{



    public class Par_ProfiloOrario : BaseEntity
    {

        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public int NumGiorniCiclo { get; set; } = 0; // stabilisce quante righe di Par_ProfiloOrarioGG ci devono essere

        public TipoProfilo TipoProfilo { get; set; } = TipoProfilo.Settimanale;

        public TimeOnly? StraoSogliaHHFullTime { get; set; }
        public StraoTipoConteggio StraoTipoConteggio { get; set; } = StraoTipoConteggio.Giornaliero;

        public StraoTipoConteggio SupplTipoConteggio { get; set; } = StraoTipoConteggio.Giornaliero;


        [Required]
        public required int IdPar_Orario_Festivo { get; set; }
        [ForeignKey("IdPar_Orario_Festivo")]
        public virtual Par_Orario? Par_Orario_FestivoNavigation { get; set; }

        public ICollection<Par_ProfiloOrarioGG>? Par_ProfiloOrarioGG { get; set; }
        public ICollection<Dip_ProfiloOrario>? Dip_ProfiloOrario { get; set; }

    }

    public enum TipoProfilo
    {
        Settimanale,
        Ciclico
    }


    public enum StraoTipoConteggio
    {
        Giornaliero,
        Settimanale,
        Mensile
    }


}
