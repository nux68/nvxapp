using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Attivita_To_Par_AttivitaModel_Mapper : Profile
    {
        public Par_Attivita_To_Par_AttivitaModel_Mapper()
        {
            CreateMap<Par_Attivita, Par_AttivitaModel>();
        }
    }

    public class Par_AttivitaModel_To_Par_Attivita_Mapper : Profile
    {
        public Par_AttivitaModel_To_Par_Attivita_Mapper()
        {
            CreateMap<Par_AttivitaModel, Par_Attivita>();
        }
    }
}
