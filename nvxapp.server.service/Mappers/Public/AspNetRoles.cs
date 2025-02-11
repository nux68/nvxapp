using AutoMapper;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.Mappers.Public
{


    public class AspNetRoles : Profile
    {

        public AspNetRoles()
        {
            CreateMap<ApplicationRole, AspNetRolesModel>()
                .ForMember(x=> x.Name,ops=> ops.MapFrom(s=> s.Name));
        }

    }

}
