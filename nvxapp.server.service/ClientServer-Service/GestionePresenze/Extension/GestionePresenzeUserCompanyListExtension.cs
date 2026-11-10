using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Extension;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Extension
{
    // Completa la lista utenti dell'azienda (UserCompanyList) con Cognome e Nome
    // dell'anagrafica dipendenti delle presenze (Dip_Anagrafica).
    public class GestionePresenzeUserCompanyListExtension : IUserCompanyListExtension
    {
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly ICommonDipendenteUtility _commonDipendenteUtility;

        public ApplicationType Application => ApplicationType.AttendanceTracking;

        public GestionePresenzeUserCompanyListExtension(IDip_AnagraficaRepository dip_AnagraficaRepository,
                                                        ICommonDipendenteUtility commonDipendenteUtility)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _commonDipendenteUtility = commonDipendenteUtility;
        }

        public async Task ExtendAsync(int IdCompany, List<UserCompanyModel> userCompanyList)
        {
            var idAspNetUsers = userCompanyList.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).ToList();

            // anagrafiche con Cognome/Nome provvisori: completate dall'inizializzazione unica (Infrastructure)
            await _commonDipendenteUtility.CompletaCognomeNomeAsync(IdCompany, idAspNetUsers);

            var anagrafiche = _dip_AnagraficaRepository.FindAll(x => idAspNetUsers.Contains(x.IdAspNetUsers))
                                                       .Select(x => new { x.IdAspNetUsers, x.Cognome, x.Nome })
                                                       .ToList()
                                                       .GroupBy(x => x.IdAspNetUsers)
                                                       .ToDictionary(g => g.Key, g => g.First());

            foreach (var item in userCompanyList)
            {
                if (item.IdAspNetUsers != null && anagrafiche.TryGetValue(item.IdAspNetUsers, out var ana))
                {
                    item.Cognome = ana.Cognome;
                    item.Nome = ana.Nome;
                }
            }
        }
    }
}
