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
          return 'usercompanyedit';
          break;
        case 'usercompanywizard':
          return 'usercompanywizard';
          break;
        case 'companywizard':
          return 'companywizard';
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


export enum MenuType {

  MenuHeader = 1,
  MenuNote = 2,
  MenuItem = 0,
  

}


export enum WorkingMode {

  Infrastructure = 0,
  AttendanceTracking = 1

}
