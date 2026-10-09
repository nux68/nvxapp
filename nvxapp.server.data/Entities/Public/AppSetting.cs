using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Impostazioni dell'applicazione salvate nel database (schema public).
     Es. la modalita' multi-tenant, decisa al primo avvio e non piu' modificabile.
    */
    public class AppSetting
    {
        public const string Key_TenancyMode = "TenancyMode";

        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Value { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
