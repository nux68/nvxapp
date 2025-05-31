using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessa_To_Az_SubCommessaModel_Mapper : Profile
    {
        public Az_SubCommessa_To_Az_SubCommessaModel_Mapper()
        {
            CreateMap<Az_SubCommessa, Az_SubCommessaModel>();
        }
    }

    public class Az_SubCommessaModel_To_Az_SubCommessa_Mapper : Profile
    {
        public Az_SubCommessaModel_To_Az_SubCommessa_Mapper()
        {
            CreateMap<Az_SubCommessaModel, Az_SubCommessa>();
        }
    }
}
