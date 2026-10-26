import { Injectable, Type, inject } from '@angular/core';
import { MainMenuAttendanceTrackingService } from './main-menu-attendance-tracking.service';
import { MainMenuInfrastructureService } from './main-menu-infrastructure.service';
import { AuthService } from './auth.service';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { DIALOG_REDEFINITION } from './dialog-redefinition';

@Injectable({
  providedIn: 'root'
})
export class MainMenuService implements iMainMenuService {

  // ridefinizioni dei dialog registrate dagli applicativi (vedi dialog-redefinition.ts)
  private readonly dialogRedefinitions = inject(DIALOG_REDEFINITION, { optional: true }) ?? [];

  constructor(private mainMenuInfrastructureService: MainMenuInfrastructureService,
              private mainMenuAttendanceTrackingService: MainMenuAttendanceTrackingService,
              private authService: AuthService) { }

  // Le voci di menu di un applicativo compaiono solo se l'applicativo e' attivo per l'azienda
  // dell'utente (AuthService.ActiveApplications, da UserLoad). Le voci dell'infrastruttura sempre.
  private forApplication(items: MainMenuItem[], application: ApplicationType): MainMenuItem[] {
    return this.authService.hasApplication(application) ? items : [];
  }

  

  public get Pages4SuperUser(): MainMenuItem[]  {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4SuperUser.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4SuperUser, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;
    
  }

  public get Pages4Admin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4Admin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4Admin, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4DealerAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4DealerAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4DealerAdmin, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4CompanyAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4CompanyAdmin, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4FinancialAdvisorAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4FinancialAdvisorAdmin, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4User(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4User.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.forApplication(this.mainMenuAttendanceTrackingService.Pages4User, ApplicationType.AttendanceTracking).forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get GetMode(): WorkingMode {

    return Math.max(
      this.mainMenuInfrastructureService.GetWorkingMode,
      this.mainMenuAttendanceTrackingService.GetWorkingMode
    );

  }

  // ---------------------------------------------------------------------------
  // Voci di menu nascoste ai ruoli CompanyAdmin e User (pagina "Menu utenti").
  // Le voci si leggono dagli elenchi uniti degli applicativi attivi, quindi le voci aggiunte
  // in un ramo applicativo (es. presenze) sono configurabili senza toccare Infrastructure.
  // Si puo' solo togliere: una voce e' configurabile per un ruolo solo se il codice gliela concede.
  // ---------------------------------------------------------------------------

  /** Ruoli per cui il codice prevede la voce, nell'elenco in cui e' dichiarata. */
  private ruoliDaCodice(item: MainMenuItem, lista: ListaMenu): string[] {
    if (item.roles?.length)
      return item.roles;
    return lista === 'user' ? ['User'] : ['CompanyPowerAdmin', 'CompanyAdmin'];
  }

  /** Ruoli dell'utente che vedono la voce per codice. */
  private ruoliChePrevedono(item: MainMenuItem, lista: ListaMenu, ruoliUtente: string[]): string[] {
    return this.ruoliDaCodice(item, lista).filter(r => ruoliUtente.includes(r));
  }

  /**
   * La voce resta nel menu per l'utente con questi ruoli: si toglie solo se tutti i suoi ruoli che la
   * prevedono l'hanno esclusa. Intestazioni, voci non configurabili e ruoli non configurabili
   * (es. CompanyPowerAdmin) non sono mai toccati; gli altri filtri per ruolo restano quelli di sempre.
   */
  public VoceVisibile(item: MainMenuItem, lista: ListaMenu, ruoliUtente: string[]): boolean {
    if (item.menuType !== MenuType.MenuItem || item.nonConfigurabile || !item.url)
      return true;
    // chi configura il menu vede sempre tutto, anche con altri ruoli (es. CompanyPowerAdmin + User)
    if (ruoliUtente.includes(RUOLO_CONFIGURATORE_MENU))
      return true;
    const ruoli = this.ruoliChePrevedono(item, lista, ruoliUtente);
    if (ruoli.length === 0 || ruoli.some(r => !RUOLI_MENU_CONFIGURABILI.includes(r)))
      return true;
    return ruoli.some(r => !this.authService.isMenuRestricted(item.url, r));
  }

  /** Navigazione verso un url: bloccata solo se e' una voce di menu nascosta a tutti i ruoli dell'utente che la prevedono. */
  public UrlVisibile(url: string): boolean {
    const key = AuthService.normalizeMenuUrl(url);
    const ruoliUtente = this.authService.RoleNames;
    const voci = this.vociPerLista()
      .filter(v => v.item.menuType === MenuType.MenuItem && v.item.url && AuthService.normalizeMenuUrl(v.item.url) === key)
      .filter(v => this.ruoliChePrevedono(v.item, v.lista, ruoliUtente).length > 0);
    return voci.length === 0 || voci.some(v => this.VoceVisibile(v.item, v.lista, ruoliUtente));
  }

  /** Voci configurabili dal CompanyPowerAdmin, una per url, con i ruoli a cui si possono togliere. */
  public get VociMenuConfigurabili(): VoceMenuConfigurabile[] {
    const voci: VoceMenuConfigurabile[] = [];
    for (const { item, lista, elenco } of this.vociPerLista()) {
      if (item.menuType !== MenuType.MenuItem || item.nonConfigurabile || !item.url)
        continue;
      const ruoli = this.ruoliDaCodice(item, lista).filter(r => RUOLI_MENU_CONFIGURABILI.includes(r));
      if (ruoli.length === 0)
        continue;

      const url = AuthService.normalizeMenuUrl(item.url);
      const esistente = voci.find(v => v.url === url);
      if (esistente) {
        ruoli.filter(r => !esistente.ruoli.includes(r)).forEach(r => esistente.ruoli.push(r));
        continue;
      }
      const sezione = elenco.find(x => x.menuType === MenuType.MenuHeader && x.group === item.group)?.title ?? '';
      voci.push({ url, title: item.title.trim(), icon: item.icon, sezione, zorder: item.zorder, ruoli: [...ruoli] });
    }
    return voci;
  }

  // voci degli elenchi dei ruoli dell'azienda (CompanyAdmin e User), con l'elenco di provenienza
  private vociPerLista(): { item: MainMenuItem, lista: ListaMenu, elenco: MainMenuItem[] }[] {
    const company = this.Pages4CompanyAdmin;
    const user = this.Pages4User;
    return [
      ...company.map(item => ({ item, lista: 'company' as ListaMenu, elenco: company })),
      ...user.map(item => ({ item, lista: 'user' as ListaMenu, elenco: user })),
    ];
  }

  /**
   * Dialog che un applicativo puo' sostituire (vedi dialog-redefinition.ts): si interrogano le ridefinizioni
   * con tutti gli applicativi attivi per l'azienda, dalla priorita' piu' alta; senza ridefinizioni si apre quello indicato.
   */
  public async RedefineDialog(dialogName: string, defaultDialog: Type<any>): Promise<Type<any>> {
    const candidate = this.dialogRedefinitions
      .filter(r => r.applications.length > 0 && r.applications.every(a => this.authService.hasApplication(a)))
      .sort((a, b) => b.priority - a.priority);
    for (const redefinition of candidate) {
      const component = await redefinition.redefineDialog(dialogName);
      if (component)
        return component;
    }
    return defaultDialog;
  }

  public RedefineNameOfPages(pageName: string): string {

    if (this.GetMode == WorkingMode.Infrastructure) {
      return pageName;
    }
    else {
      switch (pageName) {
        case 'usercompanyedit':
          return 'userdepartmentedit';
          break;
        case 'usercompanywizard':
          return 'userdepartmentwizard';
          break;
        case 'companywizard':
          return 'companyattendancewizard';
          break;
        case 'xx3':
          return 'xx3';
          break;
        case 'xx4':
          return 'xx4';
          break;


        default:
          return pageName;
          break;
      }
    }



  }

}

export interface iMainMenuService {

  get Pages4SuperUser(): MainMenuItem[];
  get Pages4Admin(): MainMenuItem[];
  get Pages4DealerAdmin(): MainMenuItem[];
  get Pages4FinancialAdvisorAdmin(): MainMenuItem[];
  get Pages4CompanyAdmin(): MainMenuItem[];
  get Pages4User(): MainMenuItem[];
  
}



export class MainMenuItem {

  menuType: MenuType;
  zorder: number;
  group: number; 

  title: string;
  component: string;
  url: string;
  icon: string;
  roles?: string[]; // aggiunta proprietà opzionale per i ruoli
  nonConfigurabile?: boolean; // voce di sistema: il CompanyPowerAdmin non puo' nasconderla (pagina "Menu utenti")
  badge?: () => number; // numero mostrato accanto alla voce (es. un signal degli applicativi), 0 = nessuno

  constructor(menuType: MenuType, zorder: number, group: number,title: string, component: string, url: string, icon: string, roles?: string[]) {
    this.menuType = menuType;
    this.zorder = zorder;
    this.group = zorder;
    this.title = title;
    this.component = component;
    this.url = url;
    this.icon = icon;
    this.roles = roles;
  }
}


/** Ruoli dell'azienda a cui il CompanyPowerAdmin puo' nascondere le voci di menu (lui vede sempre tutto). */
export const RUOLI_MENU_CONFIGURABILI = ['CompanyAdmin', 'User'];

/** Ruolo che configura il menu dell'azienda: vede sempre tutte le voci. */
export const RUOLO_CONFIGURATORE_MENU = 'CompanyPowerAdmin';

/** Elenco di menu in cui e' dichiarata una voce: Pages4CompanyAdmin o Pages4User. */
export type ListaMenu = 'company' | 'user';

/** Voce di menu configurabile nella pagina "Menu utenti". */
export interface VoceMenuConfigurabile {
  url: string;
  title: string;
  icon: string;
  sezione: string;
  zorder: number;
  /** ruoli configurabili a cui il codice concede la voce */
  ruoli: string[];
}

export enum MenuType {

  MenuHeader = 1,
  MenuNote = 2,
  MenuItem = 0,
  

}


export enum WorkingMode {

  Infrastructure = 0,
  AttendanceTracking = 1

}
