using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_AttivitaCompetenza_To_Par_AttivitaCompetenzaModel_Mapper : Profile
    {
        public Par_AttivitaCompetenza_To_Par_AttivitaCompetenzaModel_Mapper()
        {
            CreateMap<Par_AttivitaCompetenza, Par_AttivitaCompetenzaModel>();
        }
    }

    public class Par_AttivitaCompetenzaModel_To_Par_AttivitaCompetenza_Mapper : Profile
    {
        public Par_AttivitaCompetenzaModel_To_Par_AttivitaCompetenza_Mapper()
        {
            CreateMap<Par_AttivitaCompetenzaModel, Par_AttivitaCompetenza>();
        }
    }
}
