using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
      public class Dip_ProfiloOrario_To_Dip_ProfiloOrarioModel_Mapper : Profile
    {
        public Dip_ProfiloOrario_To_Dip_ProfiloOrarioModel_Mapper()
        {
            CreateMap<Dip_ProfiloOrario, Dip_ProfiloOrarioModel>();
        }
    }

    public class Dip_ProfiloOrarioModel_To_Dip_ProfiloOrario_Mapper : Profile
    {
        public Dip_ProfiloOrarioModel_To_Dip_ProfiloOrario_Mapper()
        {
            CreateMap<Dip_ProfiloOrarioModel, Dip_ProfiloOrario>();
        }
    }
}
