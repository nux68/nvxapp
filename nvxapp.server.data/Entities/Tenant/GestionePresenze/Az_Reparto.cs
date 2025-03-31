﻿using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_Reparto : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        // Relazione ricorsiva: Reparto padre
        public int? IdAz_Reparto { get; set; }
        public Az_Reparto? Az_RepartoNavigation { get; set; }



        // Relazione ricorsiva: Reparti figli
        public ICollection<Az_Reparto>? Az_Reparto_Sub { get; set; }
        
        public ICollection<Az_RepartoAttivita>? Az_RepartoAttivita { get; set; }

    }



}
