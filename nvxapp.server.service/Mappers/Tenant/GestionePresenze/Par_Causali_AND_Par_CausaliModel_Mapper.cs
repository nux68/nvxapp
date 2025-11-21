using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Causali_To_Par_CausaliModel_Mapper : Profile
    {
        public Par_Causali_To_Par_CausaliModel_Mapper()
        {
            CreateMap<Par_Causali, Par_CausaliModel>();
        }
    }

    public class Par_CausaliModel_To_Par_Causali_Mapper : Profile
    {
        public Par_CausaliModel_To_Par_Causali_Mapper()
        {
            CreateMap<Par_CausaliModel, Par_Causali>();
        }
    }
}