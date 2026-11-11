using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Log delle formule eseguite nel calcolo di un giorno: a ogni ricalcolo del giorno si sostituisce
    public class Dip_GG_FormuleLog : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Column(TypeName = "date")]
        public DateTime Giorno { get; set; }

        public int? IdPar_Formule { get; set; }
        [ForeignKey("IdPar_Formule")]
        public virtual Par_Formule? Par_FormuleNavigation { get; set; }

        public FormulaLogTipo Tipo { get; set; } = FormulaLogTipo.Info;

        [MaxLength(500)]
        public string? Messaggio { get; set; }

        public DateTime Data { get; set; }
    }

    public enum FormulaLogTipo
    {
        Info = 0,
        Warning = 1,
        Errore = 2,
        Debug = 3
    }
}
