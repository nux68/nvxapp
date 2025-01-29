import { Injectable } from '@angular/core';
import { AccountService } from '../ClientServer-Service/Account/account.service';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor() {
  }

  private _UserName$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  public get UserName$(): Observable<string | null> {
    return this._UserName$.asObservable();
  }
  public set UserName(value: string | null) {
    this._UserName$.next(value);
    if (value == null)
      this.setRole([]);
  }
  public get UserName(): string | null {
    const us = this._UserName$.getValue();
    return us;

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
