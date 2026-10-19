import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MainMenuService } from '../../../Utility/infrastructure/main-menu.service';


// Blocca la navigazione verso le voci di menu che il CompanyPowerAdmin ha nascosto a tutti i ruoli
// dell'utente che le prevedono (pagina "Menu utenti"); altrimenti rimanda alla home.
// Applicata automaticamente da RouteService a tutte le rotte di tutti gli applicativi:
// le pagine che non sono voci di menu non sono toccate.
// NB: e' solo un aiuto all'interfaccia, le API del server restano protette dai ruoli.
export const menuRestrictionGuard: CanActivateFn = (route, state) => {
  if (inject(MainMenuService).UrlVisibile(state.url))
    return true;

  return inject(Router).createUrlTree(['/home']);
};
