using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Par_Giustificativi_To_Par_GiustificativiModel_Mapper : Profile
    {

        public Par_Giustificativi_To_Par_GiustificativiModel_Mapper()
        {
            CreateMap<Par_Giustificativi, Par_GiustificativiModel>();
        }

    }

    public class Par_GiustificativiModel_To_Par_Giustificativi_Mapper : Profile
    {

        public Par_GiustificativiModel_To_Par_Giustificativi_Mapper()
        {
            CreateMap<Par_GiustificativiModel, Par_Giustificativi>();
        }

    }
}
