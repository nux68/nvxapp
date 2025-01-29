import { Injectable } from '@angular/core';
import { AccountService } from '../ClientServer-Service/Account/account.service';

@Injectable({
  providedIn: 'root'
})
export class UsersUtilityService {

  constructor(private accountService: AccountService) {
  }

  public get IsSuperUser(): boolean {
    return this.accountService.hasRole('SuperUser');
  }

  public get IsPowerAdmin(): boolean {
    return this.accountService.hasRole('PowerAdmin');
  }

  public get IsAdmin(): boolean {
    return this.accountService.hasRole('Admin');
  }

  public get IsDomainPowerAdmin(): boolean {
    return this.accountService.hasRole('DomainPowerAdmin');
  }

  public get IsDomainAdmin(): boolean {
    return this.accountService.hasRole('DomainAdmin');
  }

  public get IsInGroupDomainAdmin(): boolean {
    return (this.accountService.hasRole('DomainAdmin') || this.accountService.hasRole('DomainPowerAdmin'));
  }

  public get IsInGroupAdmin(): boolean {
    return (this.accountService.hasRole('Admin') || this.accountService.hasRole('PowerAdmin'));
  }

}
