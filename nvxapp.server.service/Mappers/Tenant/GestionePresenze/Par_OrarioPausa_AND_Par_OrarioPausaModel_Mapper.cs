using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_OrarioPausa_To_Par_OrarioPausaModel_Mapper : Profile
    {
        public Par_OrarioPausa_To_Par_OrarioPausaModel_Mapper()
        {
            CreateMap<Par_OrarioPausa, Par_OrarioPausaModel>();
        }
    }

    public class Par_OrarioPausaModel_To_Par_OrarioPausa_Mapper : Profile
    {
        public Par_OrarioPausaModel_To_Par_OrarioPausa_Mapper()
        {
            CreateMap<Par_OrarioPausaModel, Par_OrarioPausa>();
        }
    }
}
