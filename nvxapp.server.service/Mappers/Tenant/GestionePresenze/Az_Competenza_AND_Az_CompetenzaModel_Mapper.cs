using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Competenza_To_Az_CompetenzaModel_Mapper : Profile
    {
        public Az_Competenza_To_Az_CompetenzaModel_Mapper()
        {
            CreateMap<Az_Competenza, Az_CompetenzaModel>();
        }
    }

    public class Az_CompetenzaModel_To_Az_Competenza_Mapper : Profile
    {
        public Az_CompetenzaModel_To_Az_Competenza_Mapper()
        {
            CreateMap<Az_CompetenzaModel, Az_Competenza>();
        }
    }
}
