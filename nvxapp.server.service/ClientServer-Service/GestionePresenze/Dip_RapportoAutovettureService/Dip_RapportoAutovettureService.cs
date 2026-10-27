using nvxapp.server.data.Localization;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService
{
    /*
     Autovetture del rapporto di lavoro (sezione "Spesa" dell'anagrafica dipendente): servono alle spese
     di rimborso chilometrico, dove l'importo e' km x rimborso per km dell'autovettura scelta.
    */
    public class Dip_RapportoAutovettureService : ServiceBase, IDip_RapportoAutovettureService
    {
        private readonly IDip_RapportoAutovettureRepository _dip_RapportoAutovettureRepository;
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Dip_RapportoAutovettureService(IMapper mapper,
                                          UserManager<ApplicationUser> userManager,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IOptions<JwtParameter> jwtParameter,
                                          IHttpContextAccessor httpContextAccessor,
                                          IConfiguration configuration,
                                          IDip_RapportoAutovettureRepository dip_RapportoAutovettureRepository,
                                          IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository,
                                          IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_RapportoAutovettureRepository = dip_RapportoAutovettureRepository;
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Dip_RapportoAutovetture_Get_OutModel>> Dip_RapportoAutovettureGet(GenericRequest<Dip_RapportoAutovetture_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_RapportoAutovetture_Get_OutModel();

                var entity = _dip_RapportoAutovettureRepository.FindAll(x => x.IdDip_RapportoLavoro == model.Data.Id).OrderBy(x => x.Descrizione).ToList();
                retVal.Dip_RapportoAutovetture = _mapper.Map<List<Dip_RapportoAutovettureModel>>(entity);

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // allinea le autovetture del rapporto a quelle arrivate dal client (nuove con id negativo, mancanti cancellate)
        public virtual async Task<GenericResult<Dip_RapportoAutovetture_Put_OutModel>> Dip_RapportoAutovetturePut(GenericRequest<Dip_RapportoAutovetture_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_RapportoAutovetture_Put_OutModel();
                retVal.Id = model.Data.Id;

                foreach (var item in model.Data.Dip_RapportoAutovetture)
                {
                    if (string.IsNullOrWhiteSpace(item.Descrizione) || item.Descrizione.Trim().Length > 50)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("CarInvalidDescription"));
                    if ((item.Targa ?? string.Empty).Trim().Length > 15)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("CarInvalidPlate", item.Descrizione.Trim()));
                    if (item.RimborsoKM < 0 || item.RimborsoKM > 999999)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("CarInvalidMileageRate", item.Descrizione.Trim()));
                }

                var salvate = _dip_RapportoAutovettureRepository.FindAll(x => x.IdDip_RapportoLavoro == model.Data.Id).ToList();
                var idTenute = model.Data.Dip_RapportoAutovetture.Where(x => x.Id > 0).Select(x => x.Id).ToList();

                // un'autovettura usata in qualche spesa resta: le spese devono continuare a dire con che auto sono state fatte
                foreach (var tolta in salvate.Where(x => !idTenute.Contains(x.Id)))
                {
                    int usata = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_RapportoAutovetture == tolta.Id).Count();
                    if (usata > 0)
                        throw new UserMessageException(AttendanceTrackingMessages.Get("CarInUse", tolta.Descrizione ?? string.Empty, usata));
                    await _dip_RapportoAutovettureRepository.DeleteAsync(tolta);
                }

                foreach (var item in model.Data.Dip_RapportoAutovetture)
                {
                    var entity = item.Id > 0 ? salvate.FirstOrDefault(x => x.Id == item.Id) : null;
                    if (entity == null)
                        entity = new Dip_RapportoAutovetture { IdDip_RapportoLavoro = model.Data.Id };
                    entity.Descrizione = item.Descrizione!.Trim();
                    entity.Targa = string.IsNullOrWhiteSpace(item.Targa) ? null : item.Targa.Trim().ToUpperInvariant();
                    entity.RimborsoKM = Math.Round(item.RimborsoKM, 4);
                    await _dip_RapportoAutovettureRepository.UpsertAsync(entity);
                }

                var reqGet = new GenericRequest<Dip_RapportoAutovetture_Get_InModel>();
                reqGet.Data.Id = model.Data.Id;
                var resGet = await Dip_RapportoAutovettureGet(reqGet, true);
                if (resGet.Success && resGet.Data != null)
                    retVal.Dip_RapportoAutovetture = resGet.Data.Dip_RapportoAutovetture;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_RapportoAutovetture_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_RapportoAutovetture_GetAll4User_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_RapportoAutovetture_GetAll4User_OutModel();

                var utente = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, false);
                if (utente?.dip_RapportoLavoro != null)
                {
                    var entity = _dip_RapportoAutovettureRepository.FindAll(x => x.IdDip_RapportoLavoro == utente.dip_RapportoLavoro.Id).OrderBy(x => x.Descrizione).ToList();
                    retVal.Dip_RapportoAutovetture = _mapper.Map<List<Dip_RapportoAutovettureModel>>(entity);
                }

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDip_RapportoAutovettureService : IServiceBase
    {
        Task<GenericResult<Dip_RapportoAutovetture_Get_OutModel>> Dip_RapportoAutovettureGet(GenericRequest<Dip_RapportoAutovetture_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_RapportoAutovetture_Put_OutModel>> Dip_RapportoAutovetturePut(GenericRequest<Dip_RapportoAutovetture_Put_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_RapportoAutovetture_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_RapportoAutovetture_GetAll4User_InModel> model, bool isSubProcess);
    }
}
