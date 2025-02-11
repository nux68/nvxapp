using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nvxapp.server.service.Mappers
{
    public class MyTableMapper : Profile
    {

        public MyTableMapper()
        {
            CreateMap<MyTable, MyTableModel>();
        }

    }
}
