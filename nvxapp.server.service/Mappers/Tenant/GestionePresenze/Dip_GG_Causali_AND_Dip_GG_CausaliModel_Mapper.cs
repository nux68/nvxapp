using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_GG_Causali_To_Dip_GG_CausaliModel_Mapper : Profile
    {
        public Dip_GG_Causali_To_Dip_GG_CausaliModel_Mapper()
        {
            CreateMap<Dip_GG_Causali, Dip_GG_CausaliModel>()
                .AfterMap((src, dest) =>
                    {
                        dest.Hash = dest.CalcolaHashOnAttribute();
                    });
        }
    }

    public class Dip_GG_CausaliModel_To_Dip_GG_Causali_Mapper : Profile
    {
        public Dip_GG_CausaliModel_To_Dip_GG_Causali_Mapper()
        {
            CreateMap<Dip_GG_CausaliModel, Dip_GG_Causali>();
        }
    }
}
