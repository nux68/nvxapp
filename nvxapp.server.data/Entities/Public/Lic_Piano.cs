using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Piano di un listino (ANALISI_INFRASTRUCTURE_LICENZE.md §3.1, §5): uno per livello (Free, Standard, Pro).
        - Codice, Descrizione: etichette modificabili da chi gestisce il listino
        - PrezzoMensile, PrezzoAnnuale: Free sempre 0
        - limiti del piano, null = illimitati (§5.1): MaxUtenti (Presenze), MaxClienti (CRM), MaxSpazioMB (Documentale);
          il Cumulativo li usa tutti e tre, un listino di una sola funzione solo il suo
    */
    public class Lic_Piano : BaseEntity
    {
        public int IdListino { get; set; }
        public Lic_Listino? ListinoNavigation { get; set; }

        public LivelloLicenza Livello { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codice { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Descrizione { get; set; }

        public int Ordine { get; set; }

        public decimal PrezzoMensile { get; set; }

        public decimal PrezzoAnnuale { get; set; }

        public int? MaxUtenti { get; set; }

        public int? MaxClienti { get; set; }

        public int? MaxSpazioMB { get; set; }

        public bool Attivo { get; set; } = true;
    }
}
