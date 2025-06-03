using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Commessa_To_Az_CommessaModel_Mapper : Profile
    {
        public Az_Commessa_To_Az_CommessaModel_Mapper()
        {
            CreateMap<Az_Commessa, Az_CommessaModel>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => src.DataA.ToString("dd/MM/yyyy")));
        }
    }

    public class Az_CommessaModel_To_Az_Commessa_Mapper : Profile
    {
        public Az_CommessaModel_To_Az_Commessa_Mapper()
        {
            CreateMap<Az_CommessaModel, Az_Commessa>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DateTime.ParseExact(src.Data, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => DateTime.ParseExact(src.DataA, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));
        }
    }
}
