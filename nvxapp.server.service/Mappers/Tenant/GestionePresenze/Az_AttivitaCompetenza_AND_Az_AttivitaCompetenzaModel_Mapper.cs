using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_AttivitaCompetenza_To_Az_AttivitaCompetenzaModel_Mapper : Profile
    {
        public Az_AttivitaCompetenza_To_Az_AttivitaCompetenzaModel_Mapper()
        {
            CreateMap<Az_AttivitaCompetenza, Az_AttivitaCompetenzaModel>();
        }
    }

    public class Az_AttivitaCompetenzaModel_To_Az_AttivitaCompetenza_Mapper : Profile
    {
        public Az_AttivitaCompetenzaModel_To_Az_AttivitaCompetenza_Mapper()
        {
            CreateMap<Az_AttivitaCompetenzaModel, Az_AttivitaCompetenza>();
        }
    }
}
