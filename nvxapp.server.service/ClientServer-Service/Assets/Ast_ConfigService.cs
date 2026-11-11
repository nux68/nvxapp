using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Assets._utility;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Assets
{
    /*
     Configurazione di Risorse aziendali (ANALISI_ASSETS_APPLICATIVO.md): la leggono gli amministratori dell'azienda,
     la modifica il CompanyPowerAdmin.
    */
    public class Ast_ConfigService : ServiceBase, IAst_ConfigService
    {
        private readonly IAst_CfgRepository _Ast_Cfg;
        private readonly IAstUtility _utility;

        public Ast_ConfigService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IAst_CfgRepository ast_CfgRepository,
                                 IAstUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Ast_Cfg = ast_CfgRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Ast_CfgGetOutModel>> Ast_CfgGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.Amministratore)
                    throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                var cfg = await _utility.CfgAsync(ctx);
                return new Ast_CfgGetOutModel
                {
                    Permessi = new Ast_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Cfg = CfgModel(cfg),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_CfgPutOutModel>> Ast_CfgPut(GenericRequest<Ast_CfgPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                var d = model.Data.Cfg;
                var cfg = await _Ast_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                cfg.GiorniPreavviso = Math.Clamp(d.GiorniPreavviso, 0, 365);
                cfg.NotificheAttive = d.NotificheAttive;
                cfg.Note = string.IsNullOrWhiteSpace(d.Note) ? null : d.Note.Trim()[..Math.Min(2000, d.Note.Trim().Length)];
                cfg.ModifiedDate = DateTime.Now;
                cfg.ChangeUser = ctx.IdUtente;
                await _Ast_Cfg.SaveChange();
                return new Ast_CfgPutOutModel { Cfg = CfgModel(cfg) };
            }, isSubProcess);
        }

        private static Ast_CfgModel CfgModel(Ast_Cfg c) => new()
        {
            GiorniPreavviso = c.GiorniPreavviso, NotificheAttive = c.NotificheAttive, Note = c.Note
        };
    }

    public interface IAst_ConfigService : IServiceBase
    {
        Task<GenericResult<Ast_CfgGetOutModel>> Ast_CfgGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_CfgPutOutModel>> Ast_CfgPut(GenericRequest<Ast_CfgPutInModel> model, Boolean isSubProcess);
    }
}
