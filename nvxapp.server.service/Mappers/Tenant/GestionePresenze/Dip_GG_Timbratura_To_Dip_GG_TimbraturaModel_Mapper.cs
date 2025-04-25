using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.Service.Infrastructure.MyTableService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_GG_Timbratura_To_Dip_GG_TimbraturaModel_Mapper : Profile
    {

        public Dip_GG_Timbratura_To_Dip_GG_TimbraturaModel_Mapper()
        {
            CreateMap<Dip_GG_Timbratura, Dip_GG_TimbraturaModel>();
        }

    }
}
