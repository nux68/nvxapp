using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloFormuleService
{
    public class Dip_ProfiloFormuleModel
    {
        public int Id { get; set; }                       // < 0 = nuovo (client)
        public int IdDip_RapportoLavoro { get; set; }     // < 0 = rapporto nuovo (client)
        public int IdPar_ProfiloFormule { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_ProfiloFormule_Get_InModel
    {
        public int Id { get; set; }  // = IdDip_RapportoLavoro
    }
    public class Dip_ProfiloFormule_Get_OutModel : ModelResult
    {
        public List<Dip_ProfiloFormuleModel> Dip_ProfiloFormule { get; set; } = new();
    }

    public class Dip_ProfiloFormule_Put_InModel
    {
        public int Id { get; set; }  // = IdDip_RapportoLavoro
        public List<Dip_ProfiloFormuleModel> Dip_ProfiloFormule { get; set; } = new();
    }
    public class Dip_ProfiloFormule_Put_OutModel : ModelResult
    {
        public List<Dip_ProfiloFormuleModel> Dip_ProfiloFormule { get; set; } = new();
    }

    /*
     Profilo formule dei rapporti di lavoro, storicizzato Dal/Al (come Dip_ProfiloOrario).
     Si salva con l'anagrafica del dipendente (Dip_AnagraficaService): niente controller proprio.
     I periodi dello stesso rapporto non si possono sovrapporre (il motore deve trovare un solo profilo per giorno).
    */
    public class Dip_ProfiloFormuleService : ServiceBase, IDip_ProfiloFormuleService
    {
        private readonly IDip_ProfiloFormuleRepository _dip_ProfiloFormuleRepository;

        public Dip_ProfiloFormuleService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,
                                         IDip_ProfiloFormuleRepository dip_ProfiloFormuleRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_ProfiloFormuleRepository = dip_ProfiloFormuleRepository;
        }

        private List<Dip_ProfiloFormuleModel> Leggi(int idRapporto)
            => _mapper.Map<List<Dip_ProfiloFormuleModel>>(
                _dip_ProfiloFormuleRepository.FindAll(x => x.IdDip_RapportoLavoro == idRapporto).OrderBy(x => x.Dal).ToList());

        public virtual async Task<GenericResult<Dip_ProfiloFormule_Get_OutModel>> Dip_ProfiloFormuleGet(GenericRequest<Dip_ProfiloFormule_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await Task.CompletedTask;
                return new Dip_ProfiloFormule_Get_OutModel { Dip_ProfiloFormule = Leggi(model.Data.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_ProfiloFormule_Put_OutModel>> Dip_ProfiloFormulePut(GenericRequest<Dip_ProfiloFormule_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var periodi = model.Data.Dip_ProfiloFormule.Where(p => p.IdPar_ProfiloFormule > 0).OrderBy(p => p.Dal).ToList();
                for (int i = 0; i < periodi.Count; i++)
                {
                    if (periodi[i].Al.Date < periodi[i].Dal.Date)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ProfiloFormuleRigaNonValida"));
                    if (i > 0 && periodi[i].Dal.Date <= periodi[i - 1].Al.Date)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ProfiloFormuleSovrapposti",
                            $"{periodi[i - 1].Dal:dd/MM/yyyy}-{periodi[i - 1].Al:dd/MM/yyyy}", $"{periodi[i].Dal:dd/MM/yyyy}-{periodi[i].Al:dd/MM/yyyy}"));
                }

                var idClient = periodi.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
                foreach (var vecchio in _dip_ProfiloFormuleRepository.FindAll(x => x.IdDip_RapportoLavoro == model.Data.Id).ToList())
                    if (!idClient.Contains(vecchio.Id))
                        await _dip_ProfiloFormuleRepository.DeleteAsync(vecchio);

                foreach (var p in periodi)
                {
                    var entity = _mapper.Map<Dip_ProfiloFormule>(p);
                    if (entity.Id < 0) entity.Id = 0;
                    entity.IdDip_RapportoLavoro = model.Data.Id;
                    entity.Dal = p.Dal.Date;
                    entity.Al = p.Al.Date;
                    await _dip_ProfiloFormuleRepository.UpsertAsync(entity);
                }

                return new Dip_ProfiloFormule_Put_OutModel { Dip_ProfiloFormule = Leggi(model.Data.Id) };
            }, isSubProcess);
        }
    }

    public interface IDip_ProfiloFormuleService : IServiceBase
    {
        Task<GenericResult<Dip_ProfiloFormule_Get_OutModel>> Dip_ProfiloFormuleGet(GenericRequest<Dip_ProfiloFormule_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_ProfiloFormule_Put_OutModel>> Dip_ProfiloFormulePut(GenericRequest<Dip_ProfiloFormule_Put_InModel> model, bool isSubProcess);
    }
}
