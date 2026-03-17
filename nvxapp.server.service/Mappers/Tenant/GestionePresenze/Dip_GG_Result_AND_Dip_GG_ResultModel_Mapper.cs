using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;


namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_GG_Result_To_Dip_GG_ResultModel_Mapper : Profile
    {

        public Dip_GG_Result_To_Dip_GG_ResultModel_Mapper()
        {
            CreateMap<Dip_GG_Result, Dip_GG_ResultModel>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data.ToString("dd/MM/yyyy")));
        }

    }

    public class Dip_GG_ResultModel_To_Dip_GG_Result_Mapper : Profile
    {

        public Dip_GG_ResultModel_To_Dip_GG_Result_Mapper()
        {
            CreateMap<Dip_GG_ResultModel, Dip_GG_Result>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DateTime.ParseExact(src.Data, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));
        }

    }
}
