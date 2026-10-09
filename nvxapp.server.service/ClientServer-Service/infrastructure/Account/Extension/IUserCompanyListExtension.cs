using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Extension
{
    // Punto di estensione di AccountService.UserCompanyList: permette a un applicativo
    // di completare la lista utenti dell'azienda con dati propri (es. Cognome e Nome
    // dall'anagrafica dipendenti delle presenze).
    // Viene chiamato solo se l'applicativo e' attivo per l'azienda.
    // Per aggiungerne uno basta creare una classe che implementa l'interfaccia
    // (registrata automaticamente nella DI).
    public interface IUserCompanyListExtension
    {
        ApplicationType Application { get; }

        Task ExtendAsync(int IdCompany, List<UserCompanyModel> userCompanyList);
    }
}
