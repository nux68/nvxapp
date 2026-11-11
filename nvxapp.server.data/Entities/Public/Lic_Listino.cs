using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Listino delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §3.1, §4): una combinazione (proprietario, tipo soggetto, ambito).
        - IdDealer null       : listino globale del SuperUser
        - IdDealer valorizzato: listino del centro (Dealer con PuoGestireListini), ridefinisce quello globale
        - DisponibileSuCompany: solo per TipoSoggetto = FinancialAdvisor, le licenze dello studio coprono tutte le sue aziende (§4.3)
     Ha sempre tre piani, uno per livello (Free, Standard, Pro). Un listino usato da abbonamenti non si elimina: si disattiva.
    */
    public class Lic_Listino : BaseEntity
    {
        public int? IdDealer { get; set; }
        public Dealer? DealerNavigation { get; set; }

        public TipoSoggettoLicenza TipoSoggetto { get; set; }

        public AmbitoLicenza Ambito { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string Valuta { get; set; } = "EUR";

        public bool DisponibileSuCompany { get; set; }

        public bool Attivo { get; set; } = true;

        public ICollection<Lic_Piano>? Lic_Piano { get; set; }
    }
}
