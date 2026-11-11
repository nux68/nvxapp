using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Registro delle email di riepilogo inviate ai centri (ANALISI_PAGAMENTI_CENTRI.md §5).
     Serve allo storico e a non inviare due volte lo stesso riepilogo automatico.
     Mese null = riepilogo annuale.
    */
    public class Pag_InvioEmail : BaseEntity
    {
        public int IdDealer { get; set; }

        public int Anno { get; set; }
        public int? Mese { get; set; }

        public TipoRiepilogoPagamenti Tipo { get; set; }

        public int? IdRendiconto { get; set; }

        /// <summary>true = dalla manutenzione, false = inviata da un utente</summary>
        public bool Automatico { get; set; }

        [MaxLength(1000)]
        public string? Destinatari { get; set; }

        public bool Esito { get; set; }

        [MaxLength(500)]
        public string? Errore { get; set; }
    }
}
