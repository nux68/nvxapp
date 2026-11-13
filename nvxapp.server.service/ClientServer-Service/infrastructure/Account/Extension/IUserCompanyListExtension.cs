using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Extension
{
    // Punto di estensione di AccountService.UserCompanyList: permette a un applicativo
    // di completare la lista utenti dell'azienda con dati propri.
    // Viene chiamato solo se l'applicativo e' attivo per l'azienda; con Application = Common
    // sempre (es. Cognome e Nome dell'anagrafica comune, CommonUserCompanyListExtension).
    // Per aggiungerne uno basta creare una classe che implementa l'interfaccia
    // (registrata automaticamente nella DI).
    public interface IUserCompanyListExtension
    {
        ApplicationType Application { get; }

        Task ExtendAsync(int IdCompany, List<UserCompanyModel> userCompanyList);
    }
}
