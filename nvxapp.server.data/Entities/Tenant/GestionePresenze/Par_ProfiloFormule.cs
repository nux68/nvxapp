using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Profilo formule: elenco ordinato di formule, collegato ai rapporti di lavoro (Dip_ProfiloFormule)
    public class Par_ProfiloFormule : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(20)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Descrizione { get; set; }

        public ICollection<Par_ProfiloFormuleDett>? Par_ProfiloFormuleDett { get; set; }
        public ICollection<Dip_ProfiloFormule>? Dip_ProfiloFormule { get; set; }
    }

    // Riga del profilo: una formula, con ordine di esecuzione, periodo di validita' e fase
    public class Par_ProfiloFormuleDett : BaseEntity
    {
        [Required]
        public required int IdPar_ProfiloFormule { get; set; }
        [ForeignKey("IdPar_ProfiloFormule")]
        public virtual Par_ProfiloFormule? Par_ProfiloFormuleNavigation { get; set; }

        [Required]
        public required int IdPar_Formule { get; set; }
        [ForeignKey("IdPar_Formule")]
        public virtual Par_Formule? Par_FormuleNavigation { get; set; }

        public int Ordine { get; set; } = 1;

        [Column(TypeName = "date")]
        public DateTime Dal { get; set; }
        [Column(TypeName = "date")]
        public DateTime Al { get; set; }

        public bool Sospesa { get; set; }

        public FormulaFase Fase { get; set; } = FormulaFase.Finale;
    }
}
