using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Configurazione di Risorse Umane (ANALISI_HUMANRESOURCES_APPLICATIVO.md): la leggono gli amministratori dell'azienda,
     la modifica il CompanyPowerAdmin.
    */
    public class Hr_ConfigService : ServiceBase, IHr_ConfigService
    {
        private readonly IHr_CfgRepository _Hr_Cfg;
        private readonly IHrUtility _utility;

        public Hr_ConfigService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IHr_CfgRepository hr_CfgRepository,
                                 IHrUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Hr_Cfg = hr_CfgRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Hr_CfgGetOutModel>> Hr_CfgGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.Amministratore)
                    throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
                var cfg = await _utility.CfgAsync(ctx);
                return new Hr_CfgGetOutModel
                {
                    Permessi = new Hr_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Cfg = CfgModel(cfg),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CfgPutOutModel>> Hr_CfgPut(GenericRequest<Hr_CfgPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
                var d = model.Data.Cfg;
                var cfg = await _Hr_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                cfg.GiorniPreavviso = Math.Clamp(d.GiorniPreavviso, 0, 365);
                cfg.NotificheAttive = d.NotificheAttive;
                cfg.Note = string.IsNullOrWhiteSpace(d.Note) ? null : d.Note.Trim()[..Math.Min(2000, d.Note.Trim().Length)];
                cfg.ModifiedDate = DateTime.Now;
                cfg.ChangeUser = ctx.IdUtente;
                await _Hr_Cfg.SaveChange();
                return new Hr_CfgPutOutModel { Cfg = CfgModel(cfg) };
            }, isSubProcess);
        }

        private static Hr_CfgModel CfgModel(Hr_Cfg c) => new()
        {
            GiorniPreavviso = c.GiorniPreavviso, NotificheAttive = c.NotificheAttive, Note = c.Note
        };
    }

    public interface IHr_ConfigService : IServiceBase
    {
        Task<GenericResult<Hr_CfgGetOutModel>> Hr_CfgGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CfgPutOutModel>> Hr_CfgPut(GenericRequest<Hr_CfgPutInModel> model, Boolean isSubProcess);
    }
}
