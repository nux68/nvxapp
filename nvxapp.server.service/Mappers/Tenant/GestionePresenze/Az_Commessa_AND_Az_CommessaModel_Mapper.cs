using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Commessa_To_Az_CommessaModel_Mapper : Profile
    {
        public Az_Commessa_To_Az_CommessaModel_Mapper()
        {
            CreateMap<Az_Commessa, Az_CommessaModel>();
        }
    }

    public class Az_CommessaModel_To_Az_Commessa_Mapper : Profile
    {
        public Az_CommessaModel_To_Az_Commessa_Mapper()
        {
            CreateMap<Az_CommessaModel, Az_Commessa>();
        }
    }
}
