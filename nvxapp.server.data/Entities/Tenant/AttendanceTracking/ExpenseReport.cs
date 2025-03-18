﻿using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class ExpenseReport : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Date { get; set; }
        public RequestState State { get; set; }

        public int? IdRequest { get; set; }
        public Request? RequestNavigation { get; set; }
    }



}
