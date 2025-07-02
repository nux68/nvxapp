using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessaAttivita_To_Az_SubCommessaAttivitaModel_Mapper : Profile
    {
        public Az_SubCommessaAttivita_To_Az_SubCommessaAttivitaModel_Mapper()
        {
            CreateMap<Az_SubCommessaAttivita, Az_SubCommessaAttivitaModel>()
                .ForMember(dest => dest.IdPar_Attivita, opt => opt.MapFrom(src => src.IdPar_Attivita));
            CreateMap<Az_SubCommessaAttivita, Az_SubCommessaAttivita4EditModel>()
                .ForMember(dest => dest.IdPar_Attivita, opt => opt.MapFrom(src => src.IdPar_Attivita));
        }
    }

    public class Az_SubCommessaAttivitaModel_To_Az_SubCommessaAttivita_Mapper : Profile
    {
        public Az_SubCommessaAttivitaModel_To_Az_SubCommessaAttivita_Mapper()
        {
            CreateMap<Az_SubCommessaAttivitaModel, Az_SubCommessaAttivita>()
                .ForMember(dest => dest.IdPar_Attivita, opt => opt.MapFrom(src => src.IdPar_Attivita));
            CreateMap<Az_SubCommessaAttivita4EditModel, Az_SubCommessaAttivita>()
                .ForMember(dest => dest.IdPar_Attivita, opt => opt.MapFrom(src => src.IdPar_Attivita));
        }
    }
}
