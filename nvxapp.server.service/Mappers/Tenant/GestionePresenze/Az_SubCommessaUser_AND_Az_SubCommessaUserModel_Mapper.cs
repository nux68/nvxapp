using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SubCommessaUser_To_Az_SubCommessaUserModel_Mapper : Profile
    {
        public Az_SubCommessaUser_To_Az_SubCommessaUserModel_Mapper()
        {
            CreateMap<Az_SubCommessaUser, Az_SubCommessaUserModel>();
        }
    }

    public class Az_SubCommessaUserModel_To_Az_SubCommessaUser_Mapper : Profile
    {
        public Az_SubCommessaUserModel_To_Az_SubCommessaUser_Mapper()
        {
            CreateMap<Az_SubCommessaUserModel, Az_SubCommessaUser>();
        }
    }
}