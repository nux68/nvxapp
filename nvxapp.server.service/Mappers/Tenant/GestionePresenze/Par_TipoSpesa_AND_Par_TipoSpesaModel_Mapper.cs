using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_TipoSpesa_To_Par_TipoSpesaModel_Mapper : Profile
    {
        public Par_TipoSpesa_To_Par_TipoSpesaModel_Mapper()
        {
            CreateMap<Par_TipoSpesa, Par_TipoSpesaModel>().ReverseMap();
        }
    }
}
