using nvxapp.server.service.ClientServer.Models;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.Service.MyTableService.Models;
using nvxapp.server.service.Service.WeatherForecast;
using nvxapp.server.service.Service.WeatherForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nvxapp.server.data.Repositories;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using nvxapp.server.data.Entities;


namespace nvxapp.server.service.Service.MyTableService
{
    
    public class MyTableService : ServiceBase, IMyTableService
    {
        private readonly IMyTableRepository _myTableRepository;

        public MyTableService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,

                              IMyTableRepository myTableRepository) : base(mapper , userManager  , aspNetUsersRepository)
        {
            _myTableRepository = myTableRepository;
        }

        public virtual async Task<GenericResult<MyTableOutModel>> GetAll(GenericRequest<MyTableInModel> model)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTableOutModel retVal = new MyTableOutModel();

                var myTableFromDB = await  _myTableRepository.FindAll();

                var myTableModel = _mapper.Map<List<MyTableModel>>(myTableFromDB);

                retVal.MyTable = myTableModel;

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(1);

                return retVal;
            });
        }

    }

    public interface IMyTableService : IServiceBase
    {
        public Task<GenericResult<MyTableOutModel>> GetAll(GenericRequest<MyTableInModel> model);
    }
}
