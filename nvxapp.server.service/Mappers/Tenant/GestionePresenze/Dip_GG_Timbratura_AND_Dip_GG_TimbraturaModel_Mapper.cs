using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;


namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_GG_Timbratura_To_Dip_GG_TimbraturaModel_Mapper : Profile
    {

        public Dip_GG_Timbratura_To_Dip_GG_TimbraturaModel_Mapper()
        {
            CreateMap<Dip_GG_Timbratura, Dip_GG_TimbraturaModel>()
                .AfterMap((src, dest) =>
                    {
                        dest.Hash = dest.CalcolaHashOnAttribute();
                    });
        }

    }

    public class Dip_GG_TimbraturaModel_To_Dip_GG_Timbratura_Mapper : Profile
    {

        public Dip_GG_TimbraturaModel_To_Dip_GG_Timbratura_Mapper()
        {
            CreateMap<Dip_GG_TimbraturaModel, Dip_GG_Timbratura>();
        }

    }
}
