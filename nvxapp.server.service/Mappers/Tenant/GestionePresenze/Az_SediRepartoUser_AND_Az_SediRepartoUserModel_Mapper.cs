using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SediRepartoUser_To_Az_SediRepartoUserModel_Mapper : Profile
    {
        
        public Az_SediRepartoUser_To_Az_SediRepartoUserModel_Mapper()
        {
            CreateMap<Az_SediRepartoUser, Az_SediRepartoUserModel>();
        }

    }

    public class Az_SediRepartoUserModel_To_Az_SediRepartoUser_Mapper : Profile
    {

        public Az_SediRepartoUserModel_To_Az_SediRepartoUser_Mapper()
        {
            CreateMap<Az_SediRepartoUserModel, Az_SediRepartoUser>();
        }

    }
}
