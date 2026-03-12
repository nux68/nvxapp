using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_Anagrafica_To_Dip_AnagraficaModel_Mapper : Profile
    {
        public Dip_Anagrafica_To_Dip_AnagraficaModel_Mapper()
        {
            CreateMap<Dip_Anagrafica, Dip_AnagraficaModel>()
                .ForMember(dest => dest.UserName,          opt => opt.Ignore())
                .ForMember(dest => dest.RoleCode,          opt => opt.Ignore())
                .ForMember(dest => dest.Dip_RapportoLavoro, opt => opt.Ignore());
        }
    }

    public class Dip_AnagraficaModel_To_Dip_Anagrafica_Mapper : Profile
    {
        public Dip_AnagraficaModel_To_Dip_Anagrafica_Mapper()
        {
            CreateMap<Dip_AnagraficaModel, Dip_Anagrafica>()
                .ForMember(dest => dest.Dip_RapportoLavoro, opt => opt.Ignore())
                .ForMember(dest => dest.Dip_Competenza,     opt => opt.Ignore());
        }
    }
}
