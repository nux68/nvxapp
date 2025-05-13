using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Az_Sedi_To_Az_SediModel_Mapper : Profile
    {
        
        public Az_Sedi_To_Az_SediModel_Mapper()
        {
            CreateMap<Az_Sedi, Az_SediModel>();
        }

    }

    public class Az_SediModel_To_Az_Sedi_Mapper : Profile
    {

        public Az_SediModel_To_Az_Sedi_Mapper()
        {
            CreateMap<Az_SediModel, Az_Sedi>();
        }

    }
}
