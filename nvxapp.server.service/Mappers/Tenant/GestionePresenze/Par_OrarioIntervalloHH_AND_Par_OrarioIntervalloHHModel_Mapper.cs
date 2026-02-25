using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_OrarioIntervalloHH_To_Par_OrarioIntervalloHHModel_Mapper : Profile
    {
        public Par_OrarioIntervalloHH_To_Par_OrarioIntervalloHHModel_Mapper()
        {
            CreateMap<Par_OrarioIntervalloHH, Par_OrarioIntervalloHHModel>();
        }
    }

    public class Par_OrarioIntervalloHHModel_To_Par_OrarioIntervalloHH_Mapper : Profile
    {
        public Par_OrarioIntervalloHHModel_To_Par_OrarioIntervalloHH_Mapper()
        {
            CreateMap<Par_OrarioIntervalloHHModel, Par_OrarioIntervalloHH>();
        }
    }
}