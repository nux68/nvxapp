import { Injectable } from '@angular/core';
import { AccountService } from '../ClientServer-Service/Account/account.service';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Observable } from 'rxjs/internal/Observable';
import { RolesModel } from '../ClientServer-Service/Account/Models/user-roles-model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private rolesSubject = new BehaviorSubject<RolesModel[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor() {
  }

  //private _UserName$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  //public get UserName$(): Observable<string | null> {
  //  return this._UserName$.asObservable();
  //}
  //public set UserName(value: string | null) {
  //  this._UserName$.next(value);
  //  if (value == null)
  //    this.setRole([]);
  //}
  //public get UserName(): string | null {
  //  const us = this._UserName$.getValue();
  //  return us;

  //}

  private _Token$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  public get Token$(): Observable<string | null> {
    return this._Token$.asObservable();
  }
  public set Token(value: string | null) {
    this._Token$.next(value);
    if (value == null)
      this.setRole([]);
  }
  public get Token(): string | null {
    const us = this._Token$.getValue();
    return us;

  }

  public get IsUser(): boolean {
    return this.hasRole('User');
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

  public get IsCompanyPowerAdmin(): boolean {
    return this.hasRole('CompanyPowerAdmin');
  }

  public get IsCompanyAdmin(): boolean {
    return this.hasRole('CompanyAdmin');
  }
    
  public get IsDealerPowerAdmin(): boolean {
    return this.hasRole('DealerPowerAdmin');
  }

  public get IsDealerAdmin(): boolean {
    return this.hasRole('DealerAdmin');
  }

  public get IsInGroupDealerAdmin(): boolean {
    return (this.hasRole('DealerAdmin') || this.hasRole('DealerPowerAdmin'));
  }

  public get IsInGroupCompanyAdmin(): boolean {

    return (this.hasRole('CompanyAdmin') || this.hasRole('CompanyPowerAdmin'));
  }

  public get IsInGroupAdmin(): boolean {
    return (this.hasRole('Admin') || this.hasRole('PowerAdmin'));
  }

  public setRole(roles: RolesModel[]): void {
    this.rolesSubject.next(roles);
  }

  public hasRole(role: string): boolean {
    const roles = this.rolesSubject.getValue();
    return (roles.findIndex(x => x.name == role) >= 0);
  }

  public LogOut(): void {
    this.Token = null;
    this.setRole([]);
  }

}
