using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_SediReparto_To_Az_SediRepartoModel_Mapper : Profile
    {
        
        public Az_SediReparto_To_Az_SediRepartoModel_Mapper()
        {
            CreateMap<Az_SediReparto, Az_SediRepartoModel>();
        }

    }

    public class Az_SediRepartoModel_To_Az_SediReparto_Mapper : Profile
    {

        public Az_SediRepartoModel_To_Az_SediReparto_Mapper()
        {
            CreateMap<Az_SediRepartoModel, Az_SediReparto>();
        }

    }
}
