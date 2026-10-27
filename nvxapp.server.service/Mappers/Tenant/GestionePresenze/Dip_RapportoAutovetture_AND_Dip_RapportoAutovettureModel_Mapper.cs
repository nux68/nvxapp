using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_RapportoAutovetture_To_Dip_RapportoAutovettureModel_Mapper : Profile
    {
        public Dip_RapportoAutovetture_To_Dip_RapportoAutovettureModel_Mapper()
        {
            CreateMap<Dip_RapportoAutovetture, Dip_RapportoAutovettureModel>();
        }
    }
}
