using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /*
     Controllo di accesso comune alle API delle formule e dei profili formule:
       - ruolo CompanyPowerAdmin (le voci stanno nel menu Parametri, riservato a lui);
       - piano Pro della licenza Presenze (con il sistema licenze spento ILicenseGuard risponde come Pro).
     Restituisce l'azienda (Az_Anagrafica.Id) dell'utente; in caso di esito negativo UserMessageException.
    */
    public class FormuleAccesso : IFormuleAccesso
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILicenseGuard _licenseGuard;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public FormuleAccesso(UserManager<ApplicationUser> userManager, ILicenseGuard licenseGuard, IGestionePresenzeUserUtility gestionePresenzeUserUtility)
        {
            _userManager = userManager;
            _licenseGuard = licenseGuard;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public async Task<int> VerificaAsync(string? currentUserId, string? currentCompany)
        {
            var user = string.IsNullOrEmpty(currentUserId) ? null : await _userManager.FindByIdAsync(currentUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, nameof(RoleCode.CompanyPowerAdmin)))
                throw new UserMessageException(AttendanceTrackingMessages.Get("FormulaSoloPowerAdmin"));

            int.TryParse(currentCompany, out var idCompany);
            if (!await _licenseGuard.HasAtLeastAsync(idCompany, ApplicationType.AttendanceTracking, LivelloLicenza.Pro))
                throw new UserMessageException(AttendanceTrackingMessages.Get("FormulaPianoPro"));

            var company = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
            return company?.az_Anagrafica?.Id
                   ?? throw new UserMessageException(AttendanceTrackingMessages.Get("FormulaSoloPowerAdmin"));
        }
    }

    public interface IFormuleAccesso
    {
        Task<int> VerificaAsync(string? currentUserId, string? currentCompany);
    }
}
