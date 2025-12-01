using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Orario_To_Par_OrarioModel_Mapper : Profile
    {
        public Par_Orario_To_Par_OrarioModel_Mapper()
        {
            CreateMap<Par_Orario, Par_OrarioModel>();
        }
    }

    

    public class Par_OrarioModel_To_Par_Orario_Mapper : Profile
    {
        public Par_OrarioModel_To_Par_Orario_Mapper()
        {
            CreateMap<Par_OrarioModel, Par_Orario>();
        }
    }
}
