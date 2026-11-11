import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { ApplicationType, LivelloLicenza } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';


// Consente l'accesso a una pagina solo se la licenza dell'applicativo ha almeno il livello indicato
// (AuthService.hasLicenseAtLeast, da UserLoad); altrimenti rimanda alla home.
// Da usare sulle rotte delle funzioni riservate a un piano (es. Pro), insieme a MainMenuItem.minLicense.
// NB: e' solo un aiuto all'interfaccia: il blocco vero e' sul server (ILicenseGuard.HasAtLeastAsync).
export const licenseLevelGuard = (application: ApplicationType, minimo: LivelloLicenza): CanActivateFn => () => {
  const authService = inject(AuthService);
  if (authService.hasLicenseAtLeast(application, minimo))
    return true;

  return inject(Router).createUrlTree(['/home']);
};
