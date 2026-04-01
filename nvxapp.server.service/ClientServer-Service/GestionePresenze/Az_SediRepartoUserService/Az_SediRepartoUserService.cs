using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.Base;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService
{

    public class Az_SediRepartoUserService : ServiceBase, IAz_SediRepartoUserService
    {
        private readonly IAz_SediRepartoUserRepository _az_RepartoUserRepository;
        private readonly IDip_AnagraficaRepository     _dip_AnagraficaRepository;

   

        public Az_SediRepartoUserService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,

                                  IAz_SediRepartoUserRepository az_RepartoUserRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoUserRepository = az_RepartoUserRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
        }

        public virtual async Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SediRepartoUser_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoUser_GetAll_OutModel retVal = new Az_SediRepartoUser_GetAll_OutModel();

                var RepUser =  _az_RepartoUserRepository.FindAll(x=> x.IdAz_SediReparto == model.Data.IdAz_SediReparto).ToList();
                retVal.Az_RepartoUser = _mapper.Map<List<Az_SediRepartoUserModel>>(RepUser);
                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAllPeriod(GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoUser_GetAll_OutModel retVal = new Az_SediRepartoUser_GetAll_OutModel();

                var RepUser = _az_RepartoUserRepository.FindAll(x => model.Data.IdAz_SediReparto.Contains( x.IdAz_SediReparto ) ).ToList();


                

                   retVal.Az_RepartoUser = OrdinaPerCognomeNome(
                    _mapper.Map<List<Az_SediRepartoUserModel>>(RepUser));
                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


             private List<Az_SediRepartoUserModel> OrdinaPerCognomeNome(List<Az_SediRepartoUserModel> lista)
        {
            if (lista.Count == 0)
                return lista;

            // recupera le anagrafiche dei soli utenti presenti nella lista
            var idAspNetUsers = lista.Select(x => x.IdAspNetUsers).Distinct().ToList();

            var anagrafiche = _dip_AnagraficaRepository
                .FindAll(a => idAspNetUsers.Contains(a.IdAspNetUsers))
                .Select(a => new { a.IdAspNetUsers, a.Cognome, a.Nome })
                .ToList();

            return lista
                .OrderBy(u =>
                {
                    var ana = anagrafiche.FirstOrDefault(a => a.IdAspNetUsers == u.IdAspNetUsers);
                    return ana?.Cognome ?? string.Empty;
                })
                .ThenBy(u =>
                {
                    var ana = anagrafiche.FirstOrDefault(a => a.IdAspNetUsers == u.IdAspNetUsers);
                    return ana?.Nome ?? string.Empty;
                })
                .ToList();
        }

    }

    public interface IAz_SediRepartoUserService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAll( GenericRequest<Az_SediRepartoUser_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAllPeriod(GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> model, Boolean isSubProcess);
    }
}
