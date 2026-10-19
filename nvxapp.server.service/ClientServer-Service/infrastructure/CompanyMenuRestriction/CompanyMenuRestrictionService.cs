using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction
{
    /*
     Voci di menu nascoste ai ruoli CompanyAdmin e User dell'azienda corrente, configurate dal CompanyPowerAdmin.
     Si salvano solo le esclusioni (url della voce + ruolo): le voci senza esclusioni restano visibili
     come previsto dal codice, quindi quelle aggiunte in futuro dagli applicativi sono visibili di default.
     Il client applica le esclusioni al menu e blocca la navigazione verso le pagine escluse.
    */
    public class CompanyMenuRestrictionService : ServiceBase, ICompanyMenuRestrictionService
    {
        // ruoli configurabili: il CompanyPowerAdmin vede sempre tutto
        public static readonly string[] RuoliConfigurabili = { "CompanyAdmin", "User" };
        private const string RuoloConfiguratore = "CompanyPowerAdmin";

        private readonly ICompanyMenuRestrictionRepository _companyMenuRestrictionRepository;

        public CompanyMenuRestrictionService(IMapper mapper,
                                             UserManager<ApplicationUser> userManager,
                                             IAspNetUsersRepository aspNetUsersRepository,
                                             IOptions<JwtParameter> jwtParameter,
                                             IHttpContextAccessor httpContextAccessor,
                                             IConfiguration configuration,

                                             ICompanyMenuRestrictionRepository companyMenuRestrictionRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _companyMenuRestrictionRepository = companyMenuRestrictionRepository;
        }

        public virtual async Task<GenericResult<CompanyMenuRestrictionListOutModel>> CompanyMenuRestrictionList(GenericRequest<CompanyMenuRestrictionListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new CompanyMenuRestrictionListOutModel();

                int idCompany = await AziendaDelConfiguratore();
                retVal.CompanyMenuRestriction = Restrictions(idCompany);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyMenuRestrictionPutOutModel>> CompanyMenuRestrictionPut(GenericRequest<CompanyMenuRestrictionPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new CompanyMenuRestrictionPutOutModel();

                int idCompany = await AziendaDelConfiguratore();

                // esclusioni richieste: solo ruoli configurabili e url validi, senza doppioni
                var richieste = (model.Data.CompanyMenuRestriction ?? new List<CompanyMenuRestrictionModel>())
                    .Select(x => (Url: (x.Url ?? string.Empty).Trim(), Role: (x.Role ?? string.Empty).Trim()))
                    .Distinct()
                    .ToList();

                foreach (var r in richieste)
                {
                    if (!RuoliConfigurabili.Contains(r.Role))
                        throw new Exception($"Ruolo '{r.Role}' non configurabile.");
                    if (!r.Url.StartsWith("/") || r.Url.Length > 200)
                        throw new Exception($"Voce di menu '{r.Url}' non valida.");
                }

                // sostituisce l'elenco: toglie le esclusioni non piu' richieste, aggiunge le nuove
                var esistenti = _companyMenuRestrictionRepository.Restrictions(idCompany);
                foreach (var e in esistenti.Where(e => !richieste.Contains((e.Url, e.Role))))
                    await _companyMenuRestrictionRepository.DeleteAsync(e);

                foreach (var r in richieste.Where(r => !esistenti.Any(e => e.Url == r.Url && e.Role == r.Role)))
                    await _companyMenuRestrictionRepository.UpsertAsync(new data.Entities.Public.CompanyMenuRestriction
                    {
                        IdCompany = idCompany,
                        Url = r.Url,
                        Role = r.Role
                    });

                retVal.CompanyMenuRestriction = Restrictions(idCompany);

                return retVal;
            }, isSubProcess);
        }

        // azienda corrente, solo se l'utente e' CompanyPowerAdmin
        private async Task<int> AziendaDelConfiguratore()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany))
                throw new Exception("Nessuna azienda selezionata.");

            var user = await _userManager.FindByIdAsync(this.CurrentUserId)
                       ?? throw new Exception("Utente non trovato.");
            if (!await _userManager.IsInRoleAsync(user, RuoloConfiguratore))
                throw new Exception("Configurazione del menu riservata al CompanyPowerAdmin.");

            return idCompany;
        }

        private List<CompanyMenuRestrictionModel> Restrictions(int idCompany)
            => _companyMenuRestrictionRepository.Restrictions(idCompany)
                   .Select(x => new CompanyMenuRestrictionModel { Url = x.Url, Role = x.Role })
                   .ToList();
    }

    public interface ICompanyMenuRestrictionService : IServiceBase
    {
        Task<GenericResult<CompanyMenuRestrictionListOutModel>> CompanyMenuRestrictionList(GenericRequest<CompanyMenuRestrictionListInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyMenuRestrictionPutOutModel>> CompanyMenuRestrictionPut(GenericRequest<CompanyMenuRestrictionPutInModel> model, Boolean isSubProcess);
    }
}
