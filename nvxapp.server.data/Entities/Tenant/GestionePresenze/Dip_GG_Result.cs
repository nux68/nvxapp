using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Result : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        public TimeOnly HH_Teo { get; set; }
        public TimeOnly HH_Lav { get; set; }
        public GG_ResultStato Stato { get; set; }
    }


    [Flags]
    public enum GG_ResultStato
    {
        Init = 1 << 0,     // 1
        OK = 1 << 1,       // 2
        Locked = 1 << 2,   // 4
        Warning = 1 << 3,  // 8
        Err = 1 << 4,      // 16

        Err_1 = 1 << 5,
        Err_2 = 1 << 6,
        Err_3 = 1 << 7,

        Warning_1 = 1 << 8,
        Warning_2 = 1 << 9,
        Warning_3 = 1 << 10,

        STATE_MASK = Init | OK | Locked | Warning | Err
    }


    //public enum GG_ResultStato
    //{
    //    Init,
    //    Err,
    //    OK,
    //    Locked
    //}




}
