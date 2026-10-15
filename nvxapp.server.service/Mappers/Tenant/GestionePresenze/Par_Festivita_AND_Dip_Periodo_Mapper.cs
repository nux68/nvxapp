using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Festivita_To_Par_FestivitaModel_Mapper : Profile
    {
        public Par_Festivita_To_Par_FestivitaModel_Mapper()
        {
            CreateMap<Par_Festivita, Par_FestivitaModel>().ReverseMap();
        }
    }

    public class Dip_Periodo_To_Dip_PeriodoModel_Mapper : Profile
    {
        public Dip_Periodo_To_Dip_PeriodoModel_Mapper()
        {
            CreateMap<Dip_Periodo, Dip_PeriodoModel>();
        }
    }
}
