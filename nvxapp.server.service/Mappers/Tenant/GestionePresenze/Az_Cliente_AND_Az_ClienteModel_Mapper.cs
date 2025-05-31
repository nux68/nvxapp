using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Cliente_To_Az_ClienteModel_Mapper : Profile
    {
        public Az_Cliente_To_Az_ClienteModel_Mapper()
        {
            CreateMap<Az_Cliente, Az_ClienteModel>();
        }
    }

    public class Az_ClienteModel_To_Az_Cliente_Mapper : Profile
    {
        public Az_ClienteModel_To_Az_Cliente_Mapper()
        {
            CreateMap<Az_ClienteModel, Az_Cliente>();
        }
    }
}
