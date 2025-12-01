using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_ProfiloOrario_To_Par_ProfiloOrarioModel_Mapper : Profile
    {
        public Par_ProfiloOrario_To_Par_ProfiloOrarioModel_Mapper()
        {
            CreateMap<Par_ProfiloOrario, Par_ProfiloOrarioModel>();
        }
    }

    public class Par_ProfiloOrarioModel_To_Par_ProfiloOrario_Mapper : Profile
    {
        public Par_ProfiloOrarioModel_To_Par_ProfiloOrario_Mapper()
        {
            CreateMap<Par_ProfiloOrarioModel, Par_ProfiloOrario>();
        }
    }
}
