import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';


// Le pagine dello showcase (home pubblica, funzionalita', prezzi, ...) si vedono solo prima del login:
// dopo il login si va alla dashboard. Applicata da ShowcaseService a tutte le rotte dello showcase,
// quindi anche i rimandi a '/home' (guardie, logout scaduto, ...) portano l'utente collegato alla dashboard.
export const showcaseGuard: CanActivateFn = () => {
  if (inject(AuthService).Token == null)
    return true;

  return inject(Router).createUrlTree([SHOWCASE_AFTER_LOGIN]);
};

// Pagine riservate a chi ha fatto il login (es. la dashboard): senza login si torna allo showcase.
export const authenticatedGuard: CanActivateFn = () => {
  if (inject(AuthService).Token != null)
    return true;

  return inject(Router).createUrlTree(['/home']);
};

/** Pagina iniziale dopo il login (la home dello showcase e' riservata a chi non ha fatto il login). */
export const SHOWCASE_AFTER_LOGIN = '/dashboard';
