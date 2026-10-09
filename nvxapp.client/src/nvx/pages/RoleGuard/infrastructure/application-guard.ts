import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';


// Consente l'accesso alle pagine di un applicativo solo se e' attivo per l'azienda
// dell'utente (AuthService.ActiveApplications, da UserLoad); altrimenti rimanda alla home.
// Applicata automaticamente da RouteService a tutte le rotte del modulo dell'applicativo.
// NB: e' solo un aiuto all'interfaccia: il blocco vero e' sul server.
export const applicationGuard = (application: ApplicationType): CanActivateFn => () => {
  const authService = inject(AuthService);
  if (authService.hasApplication(application))
    return true;

  return inject(Router).createUrlTree(['/home']);
};
