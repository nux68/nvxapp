import { Injectable } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Infrastructure/Account/account.service';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Observable } from 'rxjs/internal/Observable';
import { RolesModel } from '../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

 

  constructor() {
  }

  private rolesSubject = new BehaviorSubject<RolesModel[]>([]);

  
  public get Roles$(): Observable<RolesModel[] | []> {
    return this.rolesSubject.asObservable();
  }

  // x forzare l'evento
  public forceRolesEmission(): void {
    this.rolesSubject.next(this.rolesSubject.getValue());
  }

  // applicativi attivi per l'azienda dell'utente corrente (da UserLoad)
  private activeApplicationsSubject = new BehaviorSubject<ApplicationType[]>([]);

  public get ActiveApplications$(): Observable<ApplicationType[]> {
    return this.activeApplicationsSubject.asObservable();
  }

  public setActiveApplications(applications: ApplicationType[] | null | undefined): void {
    this.activeApplicationsSubject.next(applications ?? []);
  }

  public hasApplication(application: ApplicationType): boolean {
    return this.activeApplicationsSubject.getValue().includes(application);
  }

  // voci di menu nascoste ai ruoli dell'azienda dell'utente corrente (da UserLoad)
  private menuRestrictionsSubject = new BehaviorSubject<{ url: string, role: string }[]>([]);

  public get MenuRestrictions$(): Observable<{ url: string, role: string }[]> {
    return this.menuRestrictionsSubject.asObservable();
  }

  public setMenuRestrictions(restrictions: { url: string, role: string }[] | null | undefined): void {
    this.menuRestrictionsSubject.next(restrictions ?? []);
  }

  /** La voce di menu (url) e' stata nascosta al ruolo dal CompanyPowerAdmin. */
  public isMenuRestricted(url: string, role: string): boolean {
    const key = AuthService.normalizeMenuUrl(url);
    return this.menuRestrictionsSubject.getValue().some(x => x.role === role && AuthService.normalizeMenuUrl(x.url) === key);
  }

  /** Url di una voce di menu senza parametri, in minuscolo e senza "/" finale (es. "/VacationPlan/?x=1" -> "/vacationplan"). */
  public static normalizeMenuUrl(url: string | null | undefined): string {
    const path = (url ?? '').split(/[?#;]/)[0].trim().toLowerCase();
    const trimmed = path.replace(/\/+$/, '');
    return trimmed.startsWith('/') ? trimmed : '/' + trimmed;
  }

  /** Nomi dei ruoli dell'utente corrente. */
  public get RoleNames(): string[] {
    return this.rolesSubject.getValue().map(x => x.name);
  }

  private _Token$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  public get Token$(): Observable<string | null> {
    return this._Token$.asObservable();
  }
  public set Token(value: string | null) {
    this._Token$.next(value);
    if (value == null) {
      this.setRole([]);
      this.setActiveApplications([]);
      this.setMenuRestrictions([]);
    }
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

  public get IsFinancialAdvisorPowerAdmin(): boolean {
    return this.hasRole('FinancialAdvisorPowerAdmin');
  }

  public get IsFinancialAdvisorAdmin(): boolean {
    return this.hasRole('FinancialAdvisorAdmin');
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

  public get IsInGroupFinancialAdvisorAdmin(): boolean {

    return (this.hasRole('FinancialAdvisorAdmin') || this.hasRole('FinancialAdvisorPowerAdmin'));
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
    this.setActiveApplications([]);
    this.setMenuRestrictions([]);
  }

}
