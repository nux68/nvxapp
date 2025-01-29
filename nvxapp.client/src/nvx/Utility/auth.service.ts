import { Injectable } from '@angular/core';
import { AccountService } from '../ClientServer-Service/Account/account.service';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor() {
  }

  public get IsSuperUser(): boolean {
    return this.hasRole('SuperUser');
  }

  public get IsPowerAdmin(): boolean {
    return this.hasRole('PowerAdmin');
  }

  public get IsAdmin(): boolean {
    return this.hasRole('Admin');
  }

  public get IsDomainPowerAdmin(): boolean {
    return this.hasRole('DomainPowerAdmin');
  }

  public get IsDomainAdmin(): boolean {
    return this.hasRole('DomainAdmin');
  }

  public get IsInGroupDomainAdmin(): boolean {
    return (this.hasRole('DomainAdmin') || this.hasRole('DomainPowerAdmin'));
  }

  public get IsInGroupAdmin(): boolean {
    return (this.hasRole('Admin') || this.hasRole('PowerAdmin'));
  }

  public setRole(roles: string[]): void {
    this.rolesSubject.next(roles);
  }

  public hasRole(role: string): boolean {
    const roles = this.rolesSubject.getValue();
    return roles.includes(role);
  }

}
