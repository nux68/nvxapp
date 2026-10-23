using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Tipi di richiesta dei ticket, configurati dal SuperUser: applicativo, descrizione (una sola,
     come gli altri dati delle tabelle), livelli abilitati (almeno due: uno che apre e uno che riceve), risoluzione automatica AI.
     I tipi non si cancellano: si disattivano (restano sui ticket esistenti).
    */
    public class TicketTypeService : ServiceBase, ITicketTypeService
    {
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly ITicketTypeLevelRepository _ticketTypeLevelRepository;

        public TicketTypeService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,

                                 ITicketTypeRepository ticketTypeRepository,
                                 ITicketTypeLevelRepository ticketTypeLevelRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _ticketTypeRepository = ticketTypeRepository;
            _ticketTypeLevelRepository = ticketTypeLevelRepository;
        }

        public virtual async Task<GenericResult<TicketTypeListOutModel>> TicketTypeList(GenericRequest<TicketTypeListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypeListOutModel();
                await RequireSuperUser();

                var types = _ticketTypeRepository.FindAll(x => true).OrderBy(x => x.Ordine).ThenBy(x => x.Codice).ToList();
                LoadChildren(types);

                retVal.TicketType = _mapper.Map<List<TicketTypeListModel>>(types);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketTypeGetOutModel>> TicketTypeGet(GenericRequest<TicketTypeGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypeGetOutModel();
                await RequireSuperUser();

                var type = await _ticketTypeRepository.FindByIdAsync(model.Data.Id);
                if (type != null)
                {
                    LoadChildren(new List<TicketType> { type });
                    retVal.TicketType = _mapper.Map<TicketTypeEditModel>(type);
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketTypePutOutModel>> TicketTypePut(GenericRequest<TicketTypePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypePutOutModel();
                await RequireSuperUser();

                var edit = model.Data.TicketType;
                edit.Codice = (edit.Codice ?? string.Empty).Trim().ToUpperInvariant();
                edit.Descrizione = (edit.Descrizione ?? string.Empty).Trim();
                var levels = (edit.Level ?? new List<TicketLevel>()).Where(l => Enum.IsDefined(l)).Distinct().ToList();

                if (edit.Codice.Length == 0 || edit.Codice.Length > 20 || edit.Descrizione.Length == 0 || edit.Descrizione.Length > 100)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeDataNotValid"));
                if (edit.ApplicationType != null && !Enum.IsDefined(edit.ApplicationType.Value))
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeDataNotValid"));
                // almeno un livello che apre e uno che riceve
                if (levels.Count < 2)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeLevelsTooFew"));
                if (_ticketTypeRepository.FindAll(x => x.Codice == edit.Codice && x.Id != edit.Id).Any())
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeCodeDuplicate", edit.Codice));

                TicketType? existing = edit.Id != 0 ? await _ticketTypeRepository.FindByIdAsync(edit.Id) : null;
                if (edit.Id != 0 && existing == null)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeDataNotValid"));

                var type = _mapper.Map<TicketType>(edit);
                if (existing != null)
                    type.CreationDate = existing.CreationDate;
                type = await _ticketTypeRepository.UpsertAsync(type);

                // livelli: si sostituiscono
                var oldLevels = _ticketTypeLevelRepository.FindAll(x => x.IdTicketType == type.Id).ToList();
                if (oldLevels.Count > 0)
                    await _ticketTypeLevelRepository.DeleteRangeAsync(oldLevels);

                foreach (var level in levels.OrderBy(l => l))
                    await _ticketTypeLevelRepository.CreateAsync(new TicketTypeLevel { IdTicketType = type.Id, Level = level }, false);
                await _ticketTypeLevelRepository.SaveChange();

                LoadChildren(new List<TicketType> { type });
                retVal.TicketType = _mapper.Map<TicketTypeEditModel>(type);

                return retVal;
            }, isSubProcess);
        }

        // l'utente del login deve essere SuperUser (anche mentre sta impersonando qualcun altro), come in CompanyMove
        private async Task RequireSuperUser()
        {
            var user = await _userManager.FindByIdAsync(this.UserIdFirstConnection)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));
            if (!await _userManager.IsInRoleAsync(user, RoleCode.SuperUser.ToString()))
                throw new UserMessageException(InfrastructureMessages.Get("OperationReservedToSuperUser"));
        }

        // livelli abilitati (le letture sono senza tracking: la collezione si valorizza a mano)
        private void LoadChildren(List<TicketType> types)
        {
            var ids = types.Select(t => t.Id).ToList();
            var levels = _ticketTypeLevelRepository.FindAll(x => ids.Contains(x.IdTicketType)).ToList();
            foreach (var t in types)
            {
                t.TicketTypeLevel = levels.Where(x => x.IdTicketType == t.Id).ToList();
            }
        }
    }

    public interface ITicketTypeService : IServiceBase
    {
        Task<GenericResult<TicketTypeListOutModel>> TicketTypeList(GenericRequest<TicketTypeListInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketTypeGetOutModel>> TicketTypeGet(GenericRequest<TicketTypeGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketTypePutOutModel>> TicketTypePut(GenericRequest<TicketTypePutInModel> model, Boolean isSubProcess);
    }
}
