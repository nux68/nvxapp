import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';


// chi lavora in un'azienda (CompanyPowerAdmin, CompanyAdmin, User): scadenzario e prese visione (INF-SVC)
@Injectable({
  providedIn: 'root',
})
export class RoleGuard4CompanyUser implements CanActivate {
  constructor(public authService: AuthService,
              private router: Router) { }

  canActivate(): boolean {
    if (this.authService.IsInGroupCompanyAdmin || this.authService.IsUser) {
      return true;
    }
    // Redireziona se l'utente non ha il ruolo richiesto
    this.router.navigate(['/home']);
    return false;
  }
}
