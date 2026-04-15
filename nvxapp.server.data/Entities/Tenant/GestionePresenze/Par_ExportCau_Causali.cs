using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    
    
    public class Par_ExportCau_Causali : BaseEntity
    {

        [Required]
        public required int IPar_ExportCau { get; set; }
        [ForeignKey("IdPar_ExportCau")]
        public virtual Par_ExportCau? Par_ExportCauNavigation { get; set; }


        [Required]
        public required int IdCausale { get; set; }
        [ForeignKey("IdCausale")]
        public virtual Par_Causali? CausaleNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        public Par_Export_TipoElaborazione TipoElaborazione { get; set; }
        public Par_Export_TipoUnita TipoUnita { get; set; }

    }


    public enum Par_Export_TipoElaborazione
    {
        Gionaliera,
        Mensile
    }

    public enum Par_Export_TipoUnita
    {
        Ore,
        Giorni,
        Importo
    }

    public enum Par_Export_Verso
    {
        Sommatoria,
        Sottrazione
    }


}
