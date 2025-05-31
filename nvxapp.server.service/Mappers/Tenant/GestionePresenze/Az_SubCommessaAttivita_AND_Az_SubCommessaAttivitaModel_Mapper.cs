using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessaAttivita_To_Az_SubCommessaAttivitaModel_Mapper : Profile
    {
        public Az_SubCommessaAttivita_To_Az_SubCommessaAttivitaModel_Mapper()
        {
            CreateMap<Az_SubCommessaAttivita, Az_SubCommessaAttivitaModel>();
        }
    }

    public class Az_SubCommessaAttivitaModel_To_Az_SubCommessaAttivita_Mapper : Profile
    {
        public Az_SubCommessaAttivitaModel_To_Az_SubCommessaAttivita_Mapper()
        {
            CreateMap<Az_SubCommessaAttivitaModel, Az_SubCommessaAttivita>();
        }
    }
}
