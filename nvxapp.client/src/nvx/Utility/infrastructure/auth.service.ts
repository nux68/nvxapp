import { Injectable } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Infrastructure/Account/account.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { RolesModel } from '../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ApplicationType, LivelloLicenza, UserLicenseModel } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

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

  // applicativi abilitati per lo studio dell'utente corrente (da UserLoad): funzioni lato studio
  private financialAdvisorApplicationsSubject = new BehaviorSubject<ApplicationType[]>([]);

  public setFinancialAdvisorApplications(applications: ApplicationType[] | null | undefined): void {
    this.financialAdvisorApplicationsSubject.next(applications ?? []);
  }

  public hasFinancialAdvisorApplication(application: ApplicationType): boolean {
    return this.financialAdvisorApplicationsSubject.getValue().includes(application);
  }

  // licenze dell'azienda (o dello studio) dell'utente corrente (da UserLoad), ANALISI_LICENZE.md §8.1.1, §8.1.2.
  // Servono solo a mostrare/nascondere: il server ricontrolla sempre (ILicenseGuard).
  // Con il sistema licenze non attivo tutto risponde come Pro, utenti illimitati.
  private licenseSubject = new BehaviorSubject<UserLicenseModel | null>(null);

  public get License$(): Observable<UserLicenseModel | null> {
    return this.licenseSubject.asObservable();
  }

  public get License(): UserLicenseModel | null {
    return this.licenseSubject.getValue();
  }

  public setLicense(license: UserLicenseModel | null | undefined): void {
    this.licenseSubject.next(license ?? null);
  }

  /** Livello di licenza della funzione; null = nessuna licenza valida. Funzioni non soggette a licenza (Common, Moke): Pro. */
  public licenseLevel(application: ApplicationType): LivelloLicenza | null {
    const license = this.licenseSubject.getValue();
    if (!license || !license.enabled)
      return LivelloLicenza.Pro;
    const f = license.function.find(x => x.application === application);
    if (!f)
      return LivelloLicenza.Pro;
    return f.livello ?? null;
  }

  public isLicensed(application: ApplicationType): boolean { return this.licenseLevel(application) != null; }
  public isFree(application: ApplicationType): boolean { return this.licenseLevel(application) === LivelloLicenza.Free; }
  public isStandard(application: ApplicationType): boolean { return this.licenseLevel(application) === LivelloLicenza.Standard; }
  public isPro(application: ApplicationType): boolean { return this.licenseLevel(application) === LivelloLicenza.Pro; }

  /** La funzione ha almeno il livello indicato (es. "serve almeno Standard"). */
  public hasLicenseAtLeast(application: ApplicationType, minimo: LivelloLicenza): boolean {
    const livello = this.licenseLevel(application);
    return livello != null && livello >= minimo;
  }

  /** Limite utenti della funzione; null = illimitati. */
  public maxUsers(application: ApplicationType): number | null {
    const license = this.licenseSubject.getValue();
    if (!license || !license.enabled)
      return null;
    return license.function.find(x => x.application === application)?.maxUtenti ?? null;
  }

  /** Gli utenti attivi stanno entro il limite della funzione. */
  public isWithinUserLimit(application: ApplicationType): boolean {
    const max = this.maxUsers(application);
    return max == null || (this.licenseSubject.getValue()?.activeUsers ?? 0) <= max;
  }

  /** Limite clienti dell'azienda (licenza CRM); null = illimitati o CRM non coperto. */
  public clientLimit(): number | null {
    const license = this.licenseSubject.getValue();
    return !license || !license.enabled ? null : license.clientLimit;
  }

  /** Si puo' aggiungere (o riattivare) un cliente: limite della licenza CRM. Il server ricontrolla. */
  public canAddClient(): boolean {
    const license = this.licenseSubject.getValue();
    return !license || !license.enabled || license.canAddClient;
  }

  /** Spazio del Documentale dato dalla licenza, in MB; null = illimitato o non coperto. */
  public maxStorageMB(): number | null {
    const license = this.licenseSubject.getValue();
    if (!license || !license.enabled)
      return null;
    return license.function.find(x => x.application === ApplicationType.DocumentManagement)?.maxSpazioMB ?? null;
  }

  /** Si puo' aggiungere (o riabilitare) un utente: limite della licenza Presenze. */
  public canAddUser(): boolean {
    const license = this.licenseSubject.getValue();
    return !license || !license.enabled || license.canAddUser;
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

  // su questo browser e' stato fatto almeno un login: entrando nel sito si va al login invece che alla home
  // dello showcase (vedi paginaIngresso). Resta anche dopo il logout.
  private static readonly LoginEffettuatoKey = 'nvx-login-effettuato';

  public get LoginEffettuato(): boolean {
    try { return localStorage.getItem(AuthService.LoginEffettuatoKey) === '1'; } catch { return false; }
  }

  /** Entrando nel sito si va al login (con aperta la sezione del menu che lo contiene): non collegato, ma con un login gia' fatto. */
  public get IngressoAlLogin(): boolean {
    return this.Token == null && this.LoginEffettuato;
  }

  private segnaLoginEffettuato(): void {
    try { localStorage.setItem(AuthService.LoginEffettuatoKey, '1'); } catch { }
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
      this.setLicense(null);
    }
    else
      this.segnaLoginEffettuato();
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
    this.setLicense(null);
  }

}
