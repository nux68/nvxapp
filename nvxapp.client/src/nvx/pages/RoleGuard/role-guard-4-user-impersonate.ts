import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../../Utility/auth.service';
import { UserNavigationService } from '../../Utility/user-navigation.service';


@Injectable({
  providedIn: 'root',
})
export class RoleGuard4UserImpersonate implements CanActivate {
  constructor(public authService: AuthService,
              public userNavigationService: UserNavigationService,
              private router: Router) { }

  canActivate(): boolean {
    //if (
    //  //this.authService.IsSuperUser ||
    //    this.authService.IsInGroupDealerAdmin ||
    //    this.authService.IsInGroupCompanyAdmin ||
    //  this.authService.IsInGroupAdmin) {

      if (this.userNavigationService.UserCanGoBack)
        return true;
      else
        return false;
    //}
    // Redireziona se l'utente non ha il ruolo richiesto
    this.router.navigate(['/home']);
    return false;
  }
}
