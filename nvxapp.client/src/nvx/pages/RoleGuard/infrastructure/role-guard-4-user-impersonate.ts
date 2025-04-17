import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';


@Injectable({
  providedIn: 'root',
})
export class RoleGuard4UserImpersonate implements CanActivate {
  constructor(public authService: AuthService,
              public userNavigationService: UserNavigationService,
              private router: Router) { }

  canActivate(): boolean {
    

    if (this.userNavigationService.UserCanGoBack)
      return true;
    else {
      this.router.navigate(['/home']);
      return false;

    }
    
  }
}
