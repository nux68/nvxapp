using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class JustificationType : BaseEntity
    {

        [Required]
        [MaxLength(50)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Code { get; set; }



    }



}
