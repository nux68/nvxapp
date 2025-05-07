using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Cfg_To_Az_CfgModel_Mapper : Profile
    {
        
        public Az_Cfg_To_Az_CfgModel_Mapper()
        {
            CreateMap<Az_Cfg, Az_CfgModel>();
        }

    }

    public class Az_CfgModel_To_Az_Cfg_Mapper : Profile
    {

        public Az_CfgModel_To_Az_Cfg_Mapper()
        {
            CreateMap<Az_CfgModel, Az_Cfg>();
        }

    }
}
