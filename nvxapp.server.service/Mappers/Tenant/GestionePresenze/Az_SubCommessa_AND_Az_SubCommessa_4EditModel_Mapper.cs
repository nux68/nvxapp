using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessa_To_Az_SubCommessa_4EditModel_Mapper : Profile
    {
        public Az_SubCommessa_To_Az_SubCommessa_4EditModel_Mapper()
        {
            CreateMap<Az_SubCommessa, Az_SubCommessa_4EditModel>()
                .ForMember(dest => dest.Az_SubCommessaUser, opt => opt.Ignore())
                .ForMember(dest => dest.Az_SubCommessaAttivita, opt => opt.Ignore())
                .ForMember(dest => dest.Az_SubCommessaSediReparto, opt => opt.Ignore())
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => src.DataA.ToString("dd/MM/yyyy")));
        }
    }

    public class Az_SubCommessa_4EditModel_To_Az_SubCommessa_Mapper : Profile
    {
        public Az_SubCommessa_4EditModel_To_Az_SubCommessa_Mapper()
        {
            CreateMap<Az_SubCommessa_4EditModel, Az_SubCommessa>()
                .ForMember(dest => dest.Az_SubCommessaUser, opt => opt.Ignore())
                .ForMember(dest => dest.Az_SubCommessaAttivita, opt => opt.Ignore())
                .ForMember(dest => dest.Az_SubCommessaSediReparto, opt => opt.Ignore())
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DateTime.ParseExact(src.Data, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => DateTime.ParseExact(src.DataA, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));
        }
    }
}
