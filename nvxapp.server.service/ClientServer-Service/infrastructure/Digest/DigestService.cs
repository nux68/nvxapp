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
using nvxapp.server.service.ClientServer_Service.Infrastructure.Digest.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest
{
    /*
     Pagina "Riepiloghi" (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3).
        - ogni utente vede i riepiloghi dell'azienda generati per lui (amministratori) e quelli degli studi in cui lavora
          (ruoli dello studio, collegamento non disabilitato)
        - "Genera ora": CompanyPowerAdmin genera i riepiloghi degli amministratori dell'azienda corrente,
          FinancialAdvisorPowerAdmin quello dello studio corrente (stessa generazione di quella settimanale, con invio
          della notifica; utile anche per le prove)
    */
    public class DigestService : ServiceBase, IDigestService
    {
        private const int Massimo = 200;
        private const int Anteprima = 300;

        private readonly IRiepilogoRepository _riepilogoRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly IDigestGenerator _generator;

        public DigestService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,

                             IRiepilogoRepository riepilogoRepository,
                             IUserCompanyRepository userCompanyRepository,
                             IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                             ICompanyRepository companyRepository,
                             IFinancialAdvisorRepository financialAdvisorRepository,
                             IDigestGenerator generator) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _riepilogoRepository = riepilogoRepository;
            _userCompanyRepository = userCompanyRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _companyRepository = companyRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _generator = generator;
        }

        public virtual async Task<GenericResult<DigestListOutModel>> DigestList(GenericRequest<DigestListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DigestListOutModel();
                var righe = await (await Visibili()).OrderByDescending(x => x.Id).Take(Massimo).ToListAsync();
                retVal.Digest = await Modelli(righe, false);
                retVal.PuoGenerare = await Generazione() != null;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DigestGetOutModel>> DigestGet(GenericRequest<DigestGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var riga = await (await Visibili()).Where(x => x.Id == model.Data.Id).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(InfrastructureMessages.Get("DigestNotFound"));
                var dettaglio = (DigestDetailModel)(await Modelli(new List<Riepilogo> { riga }, true)).First();
                dettaglio.Fatti = DigestWriter.Deserialize(riga.Fatti);
                return new DigestGetOutModel { Digest = dettaglio };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DigestGenerateOutModel>> DigestGenerate(GenericRequest<DigestGenerateInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DigestGenerateOutModel();
                var cosa = await Generazione() ?? throw new UserMessageException(InfrastructureMessages.Get("DigestGenerateNotAllowed"));

                var adesso = DateTime.Now;
                if (cosa.IdCompany != null)
                    retVal.Id = (await _generator.GenerateCompanyAsync(cosa.IdCompany.Value, adesso, true)).Select(x => x.Id).ToList();
                else
                {
                    var r = await _generator.GenerateStudioAsync(cosa.IdFinancialAdvisor!.Value, adesso, true);
                    if (r != null)
                        retVal.Id.Add(r.Id);
                }

                retVal.Messages.Add(retVal.Id.Count == 0
                    ? new Message(InfrastructureMessages.Get(cosa.IdCompany != null ? "DigestNothingCompany" : "DigestNothingStudio"), MessageType.Information)
                    : new Message(InfrastructureMessages.Get("DigestGenerated", retVal.Id.Count), MessageType.Information));
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        // riepiloghi dell'utente: quelli dell'azienda generati per lui e quelli dei suoi studi
        private async Task<IQueryable<Riepilogo>> Visibili()
        {
            var io = this.CurrentUserId;
            var studi = new List<int>();
            var user = await _userManager.FindByIdAsync(io);
            if (user != null && (await _userManager.GetRolesAsync(user)).Any(r => DigestGenerator.RuoliStudio.Contains(r)))
                studi = await _userFinancialAdvisorRepository.FindAll(x => x.IdAspNetUsers == io && !x.Disabled)
                                                             .Select(x => x.IdFinancialAdvisor).ToListAsync();
            return _riepilogoRepository.FindAll(x => (x.Tipo == TipoRiepilogo.Azienda && x.IdAspNetUsers == io)
                                                     || (x.Tipo == TipoRiepilogo.Studio && x.IdFinancialAdvisor != null && studi.Contains(x.IdFinancialAdvisor.Value)));
        }

        // cosa puo' generare l'utente con "Genera ora": l'azienda corrente (CompanyPowerAdmin) o lo studio corrente (FinancialAdvisorPowerAdmin)
        private async Task<(int? IdCompany, int? IdFinancialAdvisor)?> Generazione()
        {
            var io = this.CurrentUserId;
            var user = await _userManager.FindByIdAsync(io);
            if (user == null)
                return null;
            var ruoli = await _userManager.GetRolesAsync(user);

            if (ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin)) && int.TryParse(this.CurrentCompany, out int idCompany) && idCompany > 0
                && await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && x.IdAspNetUsers == io && !x.Disabled).AnyAsync())
                return (idCompany, null);

            if (ruoli.Contains(nameof(RoleCode.FinancialAdvisorPowerAdmin)) && int.TryParse(this.CurrentFinancialAdvisor, out int idStudio) && idStudio > 0
                && await _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == idStudio && x.IdAspNetUsers == io && !x.Disabled).AnyAsync())
                return (null, idStudio);

            return null;
        }

        private async Task<List<DigestModel>> Modelli(List<Riepilogo> righe, bool dettaglio)
        {
            var idAziende = righe.Where(x => x.IdCompany != null).Select(x => x.IdCompany!.Value).Distinct().ToList();
            var idStudi = righe.Where(x => x.IdFinancialAdvisor != null).Select(x => x.IdFinancialAdvisor!.Value).Distinct().ToList();
            var aziende = await _companyRepository.FindAll(x => idAziende.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Descrizione);
            var studi = await _financialAdvisorRepository.FindAll(x => idStudi.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Descrizione);

            return righe.Select(x =>
            {
                DigestModel m = dettaglio ? new DigestDetailModel() : new DigestModel();
                m.Id = x.Id;
                m.Tipo = x.Tipo;
                m.IdCompany = x.IdCompany;
                m.IdFinancialAdvisor = x.IdFinancialAdvisor;
                m.Soggetto = x.IdCompany != null && aziende.TryGetValue(x.IdCompany.Value, out var a) ? a
                           : x.IdFinancialAdvisor != null && studi.TryGetValue(x.IdFinancialAdvisor.Value, out var s) ? s : null;
                m.Dal = x.Dal;
                m.Al = x.Al;
                m.Titolo = x.Titolo;
                m.Testo = dettaglio || x.Testo.Length <= Anteprima ? x.Testo : x.Testo[..Anteprima].TrimEnd() + "...";
                m.GeneratoConAI = x.GeneratoConAI;
                m.Manuale = x.Manuale;
                m.Data = x.CreationDate;
                return m;
            }).ToList();
        }
    }

    public interface IDigestService : IServiceBase
    {
        Task<GenericResult<DigestListOutModel>> DigestList(GenericRequest<DigestListInModel> model, Boolean isSubProcess);
        Task<GenericResult<DigestGetOutModel>> DigestGet(GenericRequest<DigestGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<DigestGenerateOutModel>> DigestGenerate(GenericRequest<DigestGenerateInModel> model, Boolean isSubProcess);
    }
}
