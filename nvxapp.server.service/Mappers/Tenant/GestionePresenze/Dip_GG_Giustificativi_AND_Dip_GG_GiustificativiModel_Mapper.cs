using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;


namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_GG_Giustificativi_To_Dip_GG_GiustificativiModel_Mapper : Profile
    {

        public Dip_GG_Giustificativi_To_Dip_GG_GiustificativiModel_Mapper()
        {
            CreateMap<Dip_GG_Giustificativi, Dip_GG_GiustificativiModel>()
                .AfterMap((src, dest) =>
                    {
                        dest.Hash = dest.CalcolaHashOnAttribute();
                    });
        }

    }

    public class Dip_GG_GiustificativiModel_To_Dip_GG_Giustificativi_Mapper : Profile
    {

        public Dip_GG_GiustificativiModel_To_Dip_GG_Giustificativi_Mapper()
        {
            CreateMap<Dip_GG_GiustificativiModel, Dip_GG_Giustificativi>();
        }

    }
}
