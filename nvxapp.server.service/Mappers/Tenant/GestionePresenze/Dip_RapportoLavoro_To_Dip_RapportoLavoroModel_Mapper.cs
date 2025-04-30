using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;


namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_RapportoLavoro_To_Dip_RapportoLavoroModel_Mapper : Profile
    {

        public Dip_RapportoLavoro_To_Dip_RapportoLavoroModel_Mapper()
        {
            CreateMap<Dip_RapportoLavoro, Dip_RapportoLavoroModel>();
        }

    }

    public class Dip_RapportoLavoroModel_To_Dip_RapportoLavoro_Mapper : Profile
    {

        public Dip_RapportoLavoroModel_To_Dip_RapportoLavoro_Mapper()
        {
            CreateMap<Dip_RapportoLavoroModel, Dip_RapportoLavoro>();
        }

    }
}
