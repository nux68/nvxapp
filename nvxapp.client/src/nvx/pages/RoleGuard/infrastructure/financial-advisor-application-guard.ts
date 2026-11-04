import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';


// Consente l'accesso alle pagine lato studio di un applicativo solo se e' abilitato per lo studio
// dell'utente (AuthService.FinancialAdvisorApplications, da UserLoad); altrimenti rimanda alla home.
// Applicata automaticamente da RouteService alle rotte lato studio dell'applicativo.
// NB: e' solo un aiuto all'interfaccia: il blocco vero e' sul server.
export const financialAdvisorApplicationGuard = (application: ApplicationType): CanActivateFn => () => {
  const authService = inject(AuthService);
  if (authService.hasFinancialAdvisorApplication(application))
    return true;

  return inject(Router).createUrlTree(['/home']);
};
