using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance._utility;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Compliance
{
    /*
     Configurazione di Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md): la leggono gli amministratori dell'azienda,
     la modifica il CompanyPowerAdmin.
    */
    public class Cmp_ConfigService : ServiceBase, ICmp_ConfigService
    {
        private readonly ICmp_CfgRepository _Cmp_Cfg;
        private readonly ICmpUtility _utility;
        private readonly IUserCompanyRepository _userCompanyRepository;

        public Cmp_ConfigService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 ICmp_CfgRepository cmp_CfgRepository,
                                 ICmpUtility utility,
                                 IUserCompanyRepository userCompanyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Cmp_Cfg = cmp_CfgRepository;
            _utility = utility;
            _userCompanyRepository = userCompanyRepository;
        }

        public virtual async Task<GenericResult<Cmp_CfgGetOutModel>> Cmp_CfgGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.Amministratore)
                    throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
                var cfg = await _utility.CfgAsync(ctx);
                return new Cmp_CfgGetOutModel
                {
                    Permessi = new Cmp_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Cfg = CfgModel(cfg),
                    Utenti = await UtentiAzienda(ctx.IdCompany),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_CfgPutOutModel>> Cmp_CfgPut(GenericRequest<Cmp_CfgPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
                var d = model.Data.Cfg;
                var cfg = await _Cmp_Cfg.FindAll(_ => true).AsTracking().FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
                cfg.GiorniPreavviso = Math.Clamp(d.GiorniPreavviso, 0, 365);
                cfg.NotificheAttive = d.NotificheAttive;
                cfg.Note = string.IsNullOrWhiteSpace(d.Note) ? null : d.Note.Trim()[..Math.Min(2000, d.Note.Trim().Length)];
                // abilitati ai dati sanitari: solo utenti dell'azienda
                var utenti = (await UtentiAzienda(ctx.IdCompany)).Select(x => x.Id).ToHashSet();
                var sanitari = (d.UtentiDatiSanitari ?? new()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (sanitari.Any(x => !utenti.Contains(x)))
                    throw new UserMessageException(ComplianceMessages.Get("SanitaryUserInvalid"));
                var elenco = string.Join(';', sanitari);
                if (elenco.Length > 2000)
                    throw new UserMessageException(ComplianceMessages.Get("SanitaryUserInvalid"));
                cfg.UtentiDatiSanitari = elenco.Length == 0 ? null : elenco;
                cfg.ModifiedDate = DateTime.Now;
                cfg.ChangeUser = ctx.IdUtente;
                await _Cmp_Cfg.SaveChange();
                return new Cmp_CfgPutOutModel { Cfg = CfgModel(cfg) };
            }, isSubProcess);
        }

        private static Cmp_CfgModel CfgModel(Cmp_Cfg c) => new()
        {
            GiorniPreavviso = c.GiorniPreavviso, NotificheAttive = c.NotificheAttive, Note = c.Note, UtentiDatiSanitari = CmpUtility.UtentiSanitari(c)
        };

        private async Task<List<Cmp_UtenteModel>> UtentiAzienda(int idCompany)
        {
            var utenti = await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            return await _aspNetUsersRepository.FindAll(x => utenti.Contains(x.Id)).OrderBy(x => x.UserName)
                                               .Select(x => new Cmp_UtenteModel { Id = x.Id, Nome = x.UserName ?? x.Email ?? x.Id }).ToListAsync();
        }
    }

    public interface ICmp_ConfigService : IServiceBase
    {
        Task<GenericResult<Cmp_CfgGetOutModel>> Cmp_CfgGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_CfgPutOutModel>> Cmp_CfgPut(GenericRequest<Cmp_CfgPutInModel> model, Boolean isSubProcess);
    }
}
