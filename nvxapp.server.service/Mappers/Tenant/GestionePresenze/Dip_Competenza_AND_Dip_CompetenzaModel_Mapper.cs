using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_Competenza_To_Dip_CompetenzaModel_Mapper : Profile
    {
        public Dip_Competenza_To_Dip_CompetenzaModel_Mapper()
        {
            CreateMap<Dip_Competenza, Dip_CompetenzaModel>();
        }
    }

    public class Dip_CompetenzaModel_To_Dip_Competenza_Mapper : Profile
    {
        public Dip_CompetenzaModel_To_Dip_Competenza_Mapper()
        {
            CreateMap<Dip_CompetenzaModel, Dip_Competenza>();
        }
    }
}
