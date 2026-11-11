using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Dipendente dell'azienda (schema Common): condiviso tra presenze e documentale.
        - IdAz_Anagrafica : azienda del dipendente (in modalita' singola distingue le aziende)
        - IdAspNetUsers   : utente collegato, FACOLTATIVO: il documentale gestisce anche dipendenti senza
                            account; se l'utente viene eliminato il dipendente resta (documenti da conservare)
        - Matricola, CodiceFiscale: facoltativi, unici tra i dipendenti dell'azienda (controllo nei servizi)
        - Email           : email personale, per avvisi a chi non ha account
        - DataAssunzione  : data di assunzione (D6, INF-COM.4): le presenze la tengono allineata col PRIMO rapporto di lavoro (AT-COM.3)
        - DataCessazione, Attivo: il dipendente uscito resta per la conservazione dei suoi documenti;
                            le presenze tengono DataCessazione allineata con l'ULTIMO rapporto di lavoro (AT-COM.3)
        - Dip_Competenza  : competenze del dipendente (tabella comune, INF-COM.2)

     Classe partial: ogni applicativo aggiunge nel proprio ramo le navigazioni verso le sue tabelle
     (es. presenze: Dip_RapportoLavoro). CommonDbContext ignora le navigazioni che
     non portano a tabelle comuni o di public.
    */
    public partial class Dip_Anagrafica : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public string? IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public string? Cognome { get; set; }
        public string? Nome { get; set; }

        [MaxLength(20)]
        public string? Matricola { get; set; }
        [MaxLength(16)]
        public string? CodiceFiscale { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public DateTime? DataAssunzione { get; set; }
        public DateTime? DataCessazione { get; set; }

        public Boolean Attivo { get; set; } = true;

        // tabella comune: la collection sta qui (non nel partial delle presenze) perche' CommonDbContext la mappa
        public ICollection<Dip_Competenza>? Dip_Competenza { get; set; }
    }
}
