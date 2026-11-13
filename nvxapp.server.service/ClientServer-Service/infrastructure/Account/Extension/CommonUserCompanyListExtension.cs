using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Extension
{
    // Completa la lista utenti dell'azienda (UserCompanyList) con Cognome e Nome dell'anagrafica dipendente
    // comune (Dip_Anagrafica, schema Common). L'anagrafica la crea l'inizializzatore utente comune per ogni
    // utente, qualunque siano gli applicativi attivi: Common (sempre presente) fa eseguire l'estensione sempre,
    // anche senza gestione presenze.
    public class CommonUserCompanyListExtension : IUserCompanyListExtension
    {
        private readonly IDip_AnagraficaCommonRepository _dip_AnagraficaRepository;
        private readonly ICommonDipendenteUtility _commonDipendenteUtility;
        private readonly ICommonCompanyUtility _commonCompanyUtility;

        public ApplicationType Application => ApplicationType.Common;

        public CommonUserCompanyListExtension(IDip_AnagraficaCommonRepository dip_AnagraficaRepository,
                                              ICommonDipendenteUtility commonDipendenteUtility,
                                              ICommonCompanyUtility commonCompanyUtility)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _commonDipendenteUtility = commonDipendenteUtility;
            _commonCompanyUtility = commonCompanyUtility;
        }

        public async Task ExtendAsync(int IdCompany, List<UserCompanyModel> userCompanyList)
        {
            var idAspNetUsers = userCompanyList.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).Distinct().ToList();
            if (idAspNetUsers.Count == 0)
                return;

            var az_Anagrafica = await _commonCompanyUtility.GetAz_AnagraficaAsync(IdCompany, false);
            if (az_Anagrafica == null)
                return;

            // anagrafiche con Cognome/Nome provvisori: completate dall'inizializzazione unica
            await _commonDipendenteUtility.CompletaCognomeNomeAsync(IdCompany, idAspNetUsers);

            var anagrafiche = (await _dip_AnagraficaRepository.FindAll(x => x.IdAz_Anagrafica == az_Anagrafica.Id &&
                                                                            x.IdAspNetUsers != null && idAspNetUsers.Contains(x.IdAspNetUsers))
                                                              .Select(x => new { x.IdAspNetUsers, x.Cognome, x.Nome })
                                                              .ToListAsync())
                              .GroupBy(x => x.IdAspNetUsers!)
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
