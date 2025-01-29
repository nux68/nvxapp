import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { UsersUtilityService } from '../../Utility/users-utility.service';


@Injectable({
  providedIn: 'root',
})
export class RoleGuard4Admin implements CanActivate {
  constructor(public usersUtilityService: UsersUtilityService,
    private router: Router) { }

  canActivate(): boolean {
    if (this.usersUtilityService.IsAdmin) {
      return true;
    }
    // Redireziona se l'utente non ha il ruolo richiesto
    this.router.navigate(['/home']);
    return false;
  }
}
