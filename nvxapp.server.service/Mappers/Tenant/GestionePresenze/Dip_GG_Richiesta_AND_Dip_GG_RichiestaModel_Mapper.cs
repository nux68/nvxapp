using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    
    public class Dip_GG_Richiesta_To_Dip_GG_RichiestaModel_Mapper : Profile
    {
        public Dip_GG_Richiesta_To_Dip_GG_RichiestaModel_Mapper()
        {
            CreateMap<Dip_GG_Richiesta, Dip_GG_RichiestaModel>();
        }
    }

    public class Dip_GG_RichiestaModel_To_Dip_GG_Richiesta_Mapper : Profile
    {
        public Dip_GG_RichiestaModel_To_Dip_GG_Richiesta_Mapper()
        {
            CreateMap<Dip_GG_RichiestaModel, Dip_GG_Richiesta>();
        }

    }
}
