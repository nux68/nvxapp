using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models;


namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class My_Template1_To_My_Template1Model_Mapper : Profile
    {

        public My_Template1_To_My_Template1Model_Mapper()
        {
            CreateMap<My_Template1, My_Template1Model>();
        }

    }

    public class My_Template1Model_To_My_Template1_Mapper : Profile
    {

        public My_Template1Model_To_My_Template1_Mapper()
        {
            CreateMap<My_Template1Model, My_Template1>();
        }

    }
}
