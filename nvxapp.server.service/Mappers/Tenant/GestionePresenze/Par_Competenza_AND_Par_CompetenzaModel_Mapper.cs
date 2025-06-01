using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Competenza_To_Par_CompetenzaModel_Mapper : Profile
    {
        public Par_Competenza_To_Par_CompetenzaModel_Mapper()
        {
            CreateMap<Par_Competenza, Par_CompetenzaModel>();
        }
    }

    public class Par_CompetenzaModel_To_Par_Competenza_Mapper : Profile
    {
        public Par_CompetenzaModel_To_Par_Competenza_Mapper()
        {
            CreateMap<Par_CompetenzaModel, Par_Competenza>();
        }
    }
}
