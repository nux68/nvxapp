using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Navigazioni delle presenze sulla richiesta comune (Dip_GG_Richiesta sta nello schema Common, vedi
     Entities/Tenant/Common/Dip_GG_Richiesta.cs, ANALISI_INFRASTRUCTURE_RICHIESTE.md RICH-AT.6). CommonDbContext le ignora;
     le usa solo AttendanceTrackingDbContext: rapporto di lavoro (IdDip_RapportoLavoro, senza FK nel DB perche' la tabella
     delle presenze non puo' essere puntata da Common) e dettagli creati dalla richiesta (giustificativi, timbrature, note
     spese), che invece hanno la FK verso la tabella comune.
    */
    public partial class Dip_GG_Richiesta
    {
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public ICollection<Dip_GG_Giustificativi>? Dip_GG_Giustificativi { get; set; }
        public ICollection<Dip_GG_Timbratura>? Dip_GG_Timbratura { get; set; }
        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }
}
