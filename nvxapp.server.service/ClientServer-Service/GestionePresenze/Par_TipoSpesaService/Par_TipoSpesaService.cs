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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService
{
    // Tipi di spesa delle note spese (parametro dell'azienda)
    public class Par_TipoSpesaService : ServiceBase, IPar_TipoSpesaService
    {
        private readonly IPar_TipoSpesaRepository _par_TipoSpesaRepository;
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_TipoSpesaService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,

                                    IPar_TipoSpesaRepository par_TipoSpesaRepository,
                                    IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository,
                                    IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_TipoSpesaRepository = par_TipoSpesaRepository;
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        private async Task<int?> IdAzienda()
        {
            int.TryParse(this.CurrentCompany, out int idCompany);
            var company = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
            return company?.az_Anagrafica?.Id;
        }

        public virtual async Task<GenericResult<Par_TipoSpesaOutModel>> GetAll(GenericRequest<Par_TipoSpesaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_TipoSpesaOutModel();

                var idAzienda = await IdAzienda();
                if (idAzienda != null)
                {
                    var tipi = _par_TipoSpesaRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda.Value).OrderBy(x => x.Descrizione).ToList();
                    retVal.Par_TipoSpesa = _mapper.Map<List<Par_TipoSpesaModel>>(tipi);
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_TipoSpesaGetOutModel>> Par_TipoSpesaGet(GenericRequest<Par_TipoSpesaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_TipoSpesaGetOutModel();

                var tipo = await _par_TipoSpesaRepository.FindByIdAsync(model.Data.Id);
                retVal.Par_TipoSpesa = tipo != null ? _mapper.Map<Par_TipoSpesaModel>(tipo) : new Par_TipoSpesaModel();

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_TipoSpesaPutOutModel>> Par_TipoSpesaPut(GenericRequest<Par_TipoSpesaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_TipoSpesaPutOutModel();
                retVal.Par_TipoSpesa = model.Data.Par_TipoSpesa;

                var idAzienda = await IdAzienda();
                if (idAzienda != null)
                {
                    var tipo = _mapper.Map<Par_TipoSpesa>(model.Data.Par_TipoSpesa);
                    tipo.IdAz_Anagrafica = idAzienda.Value;

                    tipo = await _par_TipoSpesaRepository.UpsertAsync(tipo);
                    retVal.Par_TipoSpesa = _mapper.Map<Par_TipoSpesaModel>(tipo);
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_TipoSpesa_DeleteOutModel>> Par_TipoSpesaDelete(GenericRequest<Par_TipoSpesa_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_TipoSpesa_DeleteOutModel();

                // un tipo gia' usato nelle note spese resta: le spese devono continuare a dire di che tipo sono
                int usato = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdPar_TipoSpesa == model.Data.Id).Count();
                if (usato > 0)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExpenseTypeInUse", usato), MessageType.Exception));
                    return retVal;
                }

                var entity = await _par_TipoSpesaRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _par_TipoSpesaRepository.DeleteAsync(entity);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_TipoSpesaService : IServiceBase
    {
        Task<GenericResult<Par_TipoSpesaOutModel>> GetAll(GenericRequest<Par_TipoSpesaInModel> model, bool isSubProcess);
        Task<GenericResult<Par_TipoSpesaGetOutModel>> Par_TipoSpesaGet(GenericRequest<Par_TipoSpesaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_TipoSpesaPutOutModel>> Par_TipoSpesaPut(GenericRequest<Par_TipoSpesaPutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_TipoSpesa_DeleteOutModel>> Par_TipoSpesaDelete(GenericRequest<Par_TipoSpesa_DeleteInModel> model, bool isSubProcess);
    }
}
