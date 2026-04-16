using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_ExportCau_Causali_To_Par_ExportCau_CausaliModel_Mapper : Profile
    {
        public Par_ExportCau_Causali_To_Par_ExportCau_CausaliModel_Mapper()
        {
            CreateMap<Par_ExportCau_Causali, Par_ExportCau_CausaliModel>();
        }
    }

    public class Par_ExportCau_CausaliModel_To_Par_ExportCau_Causali_Mapper : Profile
    {
        public Par_ExportCau_CausaliModel_To_Par_ExportCau_Causali_Mapper()
        {
            CreateMap<Par_ExportCau_CausaliModel, Par_ExportCau_Causali>();
        }
    }
}
