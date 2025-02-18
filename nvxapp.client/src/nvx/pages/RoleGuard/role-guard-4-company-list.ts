import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../../Utility/auth.service';


@Injectable({
  providedIn: 'root',
})
export class RoleGuard4CompanyList implements CanActivate {
  constructor(public authService: AuthService,
              private router: Router) { }

  canActivate(): boolean {
    
    if (this.authService.IsInGroupDealerAdmin) {
      return true;
    }

    // Redireziona se l'utente non ha il ruolo richiesto
    this.router.navigate(['/home']);
    return false;
  }
}
