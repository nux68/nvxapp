using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SediAttivita_To_Az_SediAttivitaModel_Mapper : Profile
    {
        public Az_SediAttivita_To_Az_SediAttivitaModel_Mapper()
        {
            CreateMap<Az_SediAttivita, Az_SediAttivitaModel>();
        }
    }

    public class Az_SediAttivitaModel_To_Az_SediAttivita_Mapper : Profile
    {
        public Az_SediAttivitaModel_To_Az_SediAttivita_Mapper()
        {
            CreateMap<Az_SediAttivitaModel, Az_SediAttivita>();
        }
    }
}
