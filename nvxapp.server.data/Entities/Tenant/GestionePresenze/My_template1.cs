using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class My_Template1 : BaseEntity
    {
        // Esempio di proprietà, aggiungi le tue secondo necessità
        [Required]
        public int IdDip_RapportoLavoro { get; set; }
        
    }
}
