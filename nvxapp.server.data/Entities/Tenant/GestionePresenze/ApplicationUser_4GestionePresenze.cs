using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    public partial class ApplicationUser
    {
        
        public ICollection<Az_SubCommessaUser>? Az_SubCommessaUser { get; set; }

        public ICollection<Az_SediRepartoUser>? Az_SediRepartoUser { get; set; }
    }
}