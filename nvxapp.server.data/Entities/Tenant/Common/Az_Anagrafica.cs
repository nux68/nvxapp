using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Dati comuni dell'azienda (schema Common: tenant_<IdAzienda>_0, o public in modalita' singola).
     Una riga per azienda: e' l'ancora a cui si agganciano le tabelle di tutti gli applicativi
     (in modalita' singola distingue le aziende, che stanno tutte in public).

     Nessuna collection verso le tabelle degli applicativi: Infrastructure non le conosce.
     Le relazioni si configurano nel contesto di ogni applicativo con .WithMany() senza navigazione.
    */
    public class Az_Anagrafica : BaseEntity
    {
        [Required]
        [ForeignKey(nameof(CompanyNavigation))]
        public required int IdCompany { get; set; }
        [ForeignKey("IdCompany")]
        public virtual Company? CompanyNavigation { get; set; }

        public ICollection<Az_Cliente>? Az_Cliente { get; set; }
    }
}
