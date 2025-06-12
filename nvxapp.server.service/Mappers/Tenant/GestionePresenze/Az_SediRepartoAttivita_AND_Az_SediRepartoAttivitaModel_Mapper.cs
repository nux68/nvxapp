using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SediRepartoAttivita_To_Az_SediRepartoAttivitaModel_Mapper : Profile
    {
        public Az_SediRepartoAttivita_To_Az_SediRepartoAttivitaModel_Mapper()
        {
            CreateMap<Az_SediRepartoAttivita, Az_SediRepartoAttivitaModel>();
        }
    }

    public class Az_SediRepartoAttivitaModel_To_Az_SediRepartoAttivita_Mapper : Profile
    {
        public Az_SediRepartoAttivitaModel_To_Az_SediRepartoAttivita_Mapper()
        {
            CreateMap<Az_SediRepartoAttivitaModel, Az_SediRepartoAttivita>();
        }
    }
}
