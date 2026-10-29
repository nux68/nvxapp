using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{

    

    public class Par_ExportCau : BaseEntity
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


        // indicazioni libere dell'amministratore per l'AI (es. "separatore ;", "codifica ANSI")
        [MaxLength(1000)]
        public string? NoteAI { get; set; }

        // ricetta del formato prodotta dall'AI dalla documentazione (JSON) e impronta di documenti e note da cui e' nata:
        // si rigenera solo quando cambiano
        public string? RicettaAI { get; set; }
        [MaxLength(64)]
        public string? RicettaHash { get; set; }

        public ICollection<Par_ExportCau_Causali>? Par_ExportCau_Causali { get; set; }
        public ICollection<Par_ExportCau_Docs>? Par_ExportCau_Docs { get; set; }

        


    }






}
