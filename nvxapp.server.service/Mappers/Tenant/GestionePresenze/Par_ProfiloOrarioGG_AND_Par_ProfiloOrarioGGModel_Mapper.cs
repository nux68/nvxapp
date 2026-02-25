using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_ProfiloOrarioGG_To_Par_ProfiloOrarioGGModel_Mapper : Profile
    {
        public Par_ProfiloOrarioGG_To_Par_ProfiloOrarioGGModel_Mapper()
        {
            CreateMap<Par_ProfiloOrarioGG, Par_ProfiloOrarioGGModel>();
        }
    }

    public class Par_ProfiloOrarioGGModel_To_Par_ProfiloOrarioGG_Mapper : Profile
    {
        public Par_ProfiloOrarioGGModel_To_Par_ProfiloOrarioGG_Mapper()
        {
            CreateMap<Par_ProfiloOrarioGGModel, Par_ProfiloOrarioGG>();
        }
    }
}