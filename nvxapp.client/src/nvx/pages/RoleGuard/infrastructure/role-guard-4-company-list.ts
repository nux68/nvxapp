import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';


@Injectable({
  providedIn: 'root',
})
export class RoleGuard4CompanyList implements CanActivate {
  constructor(public authService: AuthService,
              private router: Router) { }

  canActivate(): boolean {
    
    if (this.authService.IsInGroupFinancialAdvisorAdmin) {
      return true;
    }

    // Redireziona se l'utente non ha il ruolo richiesto
    this.router.navigate(['/home']);
    return false;
  }
}
