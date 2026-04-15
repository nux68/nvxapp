using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_ExportCau_To_Par_ExportCauModel_Mapper : Profile
    {
        public Par_ExportCau_To_Par_ExportCauModel_Mapper()
        {
            CreateMap<Par_ExportCau, Par_ExportCauModel>();
        }
    }

    public class Par_ExportCauModel_To_Par_ExportCau_Mapper : Profile
    {
        public Par_ExportCauModel_To_Par_ExportCau_Mapper()
        {
            CreateMap<Par_ExportCauModel, Par_ExportCau>();
        }
    }
}
