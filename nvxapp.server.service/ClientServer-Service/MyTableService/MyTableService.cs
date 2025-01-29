using nvxapp.server.service.ClientServer_Service.ModelsBase;
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

        public virtual async Task<GenericResult<MyTableOutModel>> GetAll(GenericRequest<MyTableInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTableOutModel retVal = new MyTableOutModel();
                //var i = 0;
                //i = 10 / i;

                var myTableFromDB = await  _myTableRepository.FindAll();

                var myTableModel = _mapper.Map<List<MyTableModel>>(myTableFromDB);

                retVal.MyTable = myTableModel;
                retVal.MyTable.Clear();

                if (retVal.MyTable.Count == 0)
                    retVal.Messages.Add(new Message("La tabella MyTable è vuota", MessageType.Information));



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IMyTableService : IServiceBase
    {
        public Task<GenericResult<MyTableOutModel>> GetAll( GenericRequest<MyTableInModel> model, Boolean isSubProcess);
    }
}
