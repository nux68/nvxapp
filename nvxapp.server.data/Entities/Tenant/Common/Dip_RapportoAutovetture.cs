using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Autovettura del dipendente (schema Common, INF-COM.3, decisione D5): la usano le presenze per i rimborsi
     chilometrici delle note spese (Dip_GG_NotaSpesa.IdDip_RapportoAutovetture) e in futuro le risorse aziendali (flotta).

     Differenza rispetto alla versione delle presenze: li' era legata al rapporto di lavoro (IdDip_RapportoLavoro),
     tabella che resta nelle presenze e verso cui Common non puo' puntare. Qui e' legata al dipendente
     (IdDip_Anagrafica). Il ramo Presenze (AT-COM) converte i dati: IdDip_RapportoLavoro -> IdDip_Anagrafica del rapporto.
     Gli altri campi sono invariati.

     Classe partial: la collection Dip_GG_NotaSpesa la aggiunge il ramo Presenze in un partial; CommonDbContext la ignora.
    */
    public partial class Dip_RapportoAutovetture : BaseEntity
    {
        [Required]
        public required int IdDip_Anagrafica { get; set; }
        [ForeignKey("IdDip_Anagrafica")]
        public virtual Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        [MaxLength(15)]
        public string? Targa { get; set; }

        // rimborso per km
        [Column(TypeName = "numeric(10,4)")]
        public decimal RimborsoKM { get; set; }
    }
}
