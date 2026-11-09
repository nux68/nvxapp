using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /// <summary>Chi sottoscrive: azienda o studio, con il centro a cui appartiene.</summary>
    public class LicenseSubject
    {
        public TipoSoggettoLicenza Tipo { get; set; }
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int? IdDealer { get; set; }
        /// <summary>studio dell'azienda (solo per Tipo = Company)</summary>
        public int? IdFinancialAdvisor { get; set; }
        public string? Studio { get; set; }
    }

    /*
     Quale listino vede un soggetto (ANALISI_LICENZE.md §4.1), per ogni ambito:
        1. il centro del soggetto (azienda -> studio -> centro)
        2. se il centro ha PuoGestireListini ed esiste un suo listino attivo per (tipo soggetto, ambito): quello
        3. altrimenti il listino globale del SuperUser (IdDealer null)
     Regola unica: si usa sempre questo servizio, mai duplicata.
    */
    public interface ILicenseCatalogResolver
    {
        Task<LicenseSubject?> CompanySubjectAsync(int idCompany);
        Task<LicenseSubject?> StudioSubjectAsync(int idFinancialAdvisor);
        /// <summary>listino risolto, con i piani; null = nessun listino attivo</summary>
        Task<Lic_Listino?> ResolveListinoAsync(LicenseSubject soggetto, AmbitoLicenza ambito);
    }

    public class LicenseCatalogResolver : ILicenseCatalogResolver
    {
        private readonly ILic_ListinoRepository _listinoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;

        public LicenseCatalogResolver(ILic_ListinoRepository listinoRepository,
                                      ICompanyRepository companyRepository,
                                      IFinancialAdvisorRepository financialAdvisorRepository)
        {
            _listinoRepository = listinoRepository;
            _companyRepository = companyRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
        }

        public async Task<LicenseSubject?> CompanySubjectAsync(int idCompany)
            => await _companyRepository.FindAll(x => x.Id == idCompany)
                    .Select(x => new LicenseSubject
                    {
                        Tipo = TipoSoggettoLicenza.Company,
                        Id = x.Id,
                        Descrizione = x.Descrizione ?? string.Empty,
                        IdDealer = x.FinancialAdvisorNavigation!.IdDealer,
                        IdFinancialAdvisor = x.IdFinancialAdvisor,
                        Studio = x.FinancialAdvisorNavigation.Descrizione
                    })
                    .FirstOrDefaultAsync();

        public async Task<LicenseSubject?> StudioSubjectAsync(int idFinancialAdvisor)
            => await _financialAdvisorRepository.FindAll(x => x.Id == idFinancialAdvisor)
                    .Select(x => new LicenseSubject
                    {
                        Tipo = TipoSoggettoLicenza.FinancialAdvisor,
                        Id = x.Id,
                        Descrizione = x.Descrizione ?? string.Empty,
                        IdDealer = x.IdDealer
                    })
                    .FirstOrDefaultAsync();

        public async Task<Lic_Listino?> ResolveListinoAsync(LicenseSubject soggetto, AmbitoLicenza ambito)
        {
            if (soggetto.IdDealer != null)
            {
                var delCentro = await _listinoRepository
                    .FindAll(x => x.Attivo && x.IdDealer == soggetto.IdDealer && x.DealerNavigation!.PuoGestireListini
                                  && x.TipoSoggetto == soggetto.Tipo && x.Ambito == ambito)
                    .Include(x => x.Lic_Piano)
                    .FirstOrDefaultAsync();
                if (delCentro != null)
                    return delCentro;
            }

            return await _listinoRepository
                .FindAll(x => x.Attivo && x.IdDealer == null && x.TipoSoggetto == soggetto.Tipo && x.Ambito == ambito)
                .Include(x => x.Lic_Piano)
                .FirstOrDefaultAsync();
        }
    }
}
