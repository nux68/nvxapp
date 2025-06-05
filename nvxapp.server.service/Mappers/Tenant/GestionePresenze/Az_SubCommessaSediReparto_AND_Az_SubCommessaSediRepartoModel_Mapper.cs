using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessaSediReparto_To_Az_SubCommessaSediRepartoModel_Mapper : Profile
    {
        public Az_SubCommessaSediReparto_To_Az_SubCommessaSediRepartoModel_Mapper()
        {
            CreateMap<Az_SubCommessaSediReparto, Az_SubCommessaSediRepartoModel>();
        }
    }

    public class Az_SubCommessaSediRepartoModel_To_Az_SubCommessaSediReparto_Mapper : Profile
    {
        public Az_SubCommessaSediRepartoModel_To_Az_SubCommessaSediReparto_Mapper()
        {
            CreateMap<Az_SubCommessaSediRepartoModel, Az_SubCommessaSediReparto>();
        }
    }
}
