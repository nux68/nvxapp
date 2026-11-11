using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement
{
    /*
     Pagina "Prese visione" dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6): le richieste rivolte
     a lui, in tutte le aziende in cui lavora. Con il canale base conferma qui (si registrano data, IP e browser come prova);
     con gli altri canali (es. firma del documentale) apre la pagina indicata e la conferma arriva dal canale.
    */
    public class AcknowledgementUserService : ServiceBase, IAcknowledgementUserService
    {
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly ICompanyRepository _companyRepository;

        public AcknowledgementUserService(IMapper mapper,
                                          UserManager<ApplicationUser> userManager,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IOptions<JwtParameter> jwtParameter,
                                          IHttpContextAccessor httpContextAccessor,
                                          IConfiguration configuration,

                                          IPresaVisioneRepository presaVisioneRepository,
                                          ICompanyRepository companyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _presaVisioneRepository = presaVisioneRepository;
            _companyRepository = companyRepository;
        }

        public virtual async Task<GenericResult<AcknowledgementListOutModel>> AcknowledgementList(GenericRequest<AcknowledgementListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new AcknowledgementListOutModel();
                var query = Mie();
                if (model.Data?.DaConfermare ?? true)
                    query = query.Where(x => x.DataConferma == null);
                retVal.Acknowledgement = await Modelli(query.OrderByDescending(x => x.Id).Take(500));
                retVal.DaConfermare = await DaConfermare();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<AcknowledgementConfirmOutModel>> AcknowledgementConfirm(GenericRequest<AcknowledgementConfirmInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new AcknowledgementConfirmOutModel();
                var riga = await Mie().Where(x => x.Id == model.Data.Id).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(InfrastructureMessages.Get("AcknowledgementNotFound"));
                if (riga.Canale != ConfirmationChannel.CodeConferma)
                    throw new UserMessageException(InfrastructureMessages.Get("AcknowledgementOtherChannel"));

                if (riga.DataConferma == null)
                {
                    var http = _httpContextAccessor.HttpContext;
                    riga.DataConferma = DateTime.Now;
                    riga.Ip = AcknowledgementService.Taglia(http?.Connection.RemoteIpAddress?.ToString(), 64);
                    riga.UserAgent = AcknowledgementService.Taglia(http?.Request.Headers.UserAgent.ToString(), 500);
                    await _presaVisioneRepository.UpdateAsync(riga);
                }

                retVal.Acknowledgement = (await Modelli(Mie().Where(x => x.Id == riga.Id))).First();
                retVal.DaConfermare = await DaConfermare();
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        private IQueryable<PresaVisione> Mie()
        {
            var io = this.CurrentUserId;
            return _presaVisioneRepository.FindAll(x => x.IdAspNetUsers == io && !x.Annullata);
        }

        private Task<int> DaConfermare() => Mie().CountAsync(x => x.DataConferma == null);

        private async Task<List<AcknowledgementModel>> Modelli(IQueryable<PresaVisione> query)
        {
            var righe = await query.ToListAsync();
            var idAziende = righe.Select(x => x.IdCompany).Distinct().ToList();
            var aziende = await _companyRepository.FindAll(x => idAziende.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Descrizione);
            return righe.Select(x => new AcknowledgementModel
            {
                Id = x.Id,
                IdCompany = x.IdCompany,
                Azienda = aziende.TryGetValue(x.IdCompany, out var a) ? a : null,
                Application = x.Application,
                Titolo = x.Titolo,
                Testo = x.Testo,
                Url = x.Url,
                UrlState = x.UrlState,
                Canale = x.Canale,
                ConfermabileQui = x.Canale == ConfirmationChannel.CodeConferma,
                DataRichiesta = x.CreationDate,
                DataConferma = x.DataConferma
            }).ToList();
        }
    }

    public interface IAcknowledgementUserService : IServiceBase
    {
        Task<GenericResult<AcknowledgementListOutModel>> AcknowledgementList(GenericRequest<AcknowledgementListInModel> model, Boolean isSubProcess);
        Task<GenericResult<AcknowledgementConfirmOutModel>> AcknowledgementConfirm(GenericRequest<AcknowledgementConfirmInModel> model, Boolean isSubProcess);
    }
}
