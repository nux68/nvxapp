using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
 public class Dip_Anagrafica_To_Dip_Anagrafica4EditModel_Mapper : Profile
    {
        public Dip_Anagrafica_To_Dip_Anagrafica4EditModel_Mapper()
        {
            CreateMap<Dip_Anagrafica, Dip_Anagrafica4EditModel>()
                 .ForMember(dest => dest.Dip_RapportoLavoro, opt => opt.Ignore());
        }
    }

    public class Dip_Anagrafica4EditModel_To_Dip_Anagrafica_Mapper : Profile
    {
        public Dip_Anagrafica4EditModel_To_Dip_Anagrafica_Mapper()
        {
            CreateMap<Dip_Anagrafica4EditModel, Dip_Anagrafica>()
                .ForMember(dest => dest.Dip_RapportoLavoro, opt => opt.Ignore())
                .ForMember(dest => dest.Dip_Competenza, opt => opt.Ignore());
        }
    }
}
