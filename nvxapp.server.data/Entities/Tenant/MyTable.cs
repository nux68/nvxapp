using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Tenant
{
    public class MyTable : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

    }
}
