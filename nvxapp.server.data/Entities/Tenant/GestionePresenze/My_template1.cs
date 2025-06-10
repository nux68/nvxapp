using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class My_Template1 : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
        
    }
}
