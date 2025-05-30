using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Attivita_To_Az_AttivitaModel_Mapper : Profile
    {
        public Az_Attivita_To_Az_AttivitaModel_Mapper()
        {
            CreateMap<Az_Attivita, Az_AttivitaModel>();
        }
    }

    public class Az_AttivitaModel_To_Az_Attivita_Mapper : Profile
    {
        public Az_AttivitaModel_To_Az_Attivita_Mapper()
        {
            CreateMap<Az_AttivitaModel, Az_Attivita>();
        }
    }
}
