﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class UserCompany : BaseEntity
    {

        [Required]
        public int IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }
    }
}
