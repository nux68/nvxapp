using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService
{
    public class Az_SubCommessaSediRepartoService : ServiceBase, IAz_SubCommessaSediRepartoService
    {
        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;

        public Az_SubCommessaSediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;
        }

        public virtual async Task<GenericResult<Az_SubCommessaSediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaSediReparto_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaSediReparto_GetAll_OutModel retVal = new Az_SubCommessaSediReparto_GetAll_OutModel();
                var entities = await _az_SubCommessaSediRepartoRepository.FindAll();
                retVal.Az_SubCommessaSediReparto = _mapper.Map<List<Az_SubCommessaSediRepartoModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SubCommessaSediReparto_Get4SubCommessa_OutModel>> Get4SubCommessa(GenericRequest<Az_SubCommessaSediReparto_Get4SubCommessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaSediReparto_Get4SubCommessa_OutModel retVal = new Az_SubCommessaSediReparto_Get4SubCommessa_OutModel();
                var entities = _az_SubCommessaSediRepartoRepository.FindAll(x => x.IdAz_SubCommessa == model.Data.IdAz_SubCommessa).ToList();
                retVal.Az_SubCommessaSediReparto = _mapper.Map<List<Az_SubCommessaSediReparto4EditModel>>(entities);
                
                foreach (var item in retVal.Az_SubCommessaSediReparto)
                    item.Checked = true;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SubCommessaSediReparto_Put4SubCommessa_OutModel>> Put4SubCommessa(GenericRequest<Az_SubCommessaSediReparto_Put4SubCommessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaSediReparto_Put4SubCommessa_OutModel retVal = new Az_SubCommessaSediReparto_Put4SubCommessa_OutModel();
                var req = new GenericRequest<Az_SubCommessaSediReparto_Get4SubCommessa_InModel>();
                req.Data.IdAz_SubCommessa = model.Data.IdAz_SubCommessa;
                var res = await Get4SubCommessa(req, true);
                if (res.Success && res.Data != null)
                {
                    foreach (var item in res.Data.Az_SubCommessaSediReparto)
                    {
                        var orig = _az_SubCommessaSediRepartoRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                        if (orig != null)
                        {
                            var orig_TMP = model.Data.Az_SubCommessaSediReparto.Where(x => x.Id == item.Id && x.Checked == true).FirstOrDefault();
                            if (orig_TMP == null)
                            {
                                await _az_SubCommessaSediRepartoRepository.DeleteAsync(orig);
                            }
                        }
                    }
                    foreach (var item in model.Data.Az_SubCommessaSediReparto.Where(x => x.Checked == true).ToList())
                    {
                        var orig = _az_SubCommessaSediRepartoRepository.FindAll(x => x.IdAz_SubCommessa == item.IdAz_SubCommessa && x.IdAz_SediReparto == item.IdAz_SediReparto).FirstOrDefault();
                        if (orig == null)
                        {
                            orig = _mapper.Map<nvxapp.server.data.Entities.Tenant.GestionePresenze.Az_SubCommessaSediReparto>(item);
                            orig.IdAz_SubCommessa = model.Data.IdAz_SubCommessa;
                            orig.IdAz_SediReparto = item.IdAz_SediReparto;
                        }
                        else
                        {
                            orig = _mapper.Map<nvxapp.server.data.Entities.Tenant.GestionePresenze.Az_SubCommessaSediReparto>(item);
                        }
                        orig = await _az_SubCommessaSediRepartoRepository.UpsertAsync(orig);
                    }
                }
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaSediRepartoService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaSediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaSediReparto_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaSediReparto_Get4SubCommessa_OutModel>> Get4SubCommessa(GenericRequest<Az_SubCommessaSediReparto_Get4SubCommessa_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaSediReparto_Put4SubCommessa_OutModel>> Put4SubCommessa(GenericRequest<Az_SubCommessaSediReparto_Put4SubCommessa_InModel> model, bool isSubProcess);
    }
}
