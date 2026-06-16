import { Injectable } from '@angular/core';
import { MainMenuAttendanceTrackingService } from './main-menu-attendance-tracking.service';
import { MainMenuInfrastructureService } from './main-menu-infrastructure.service';

@Injectable({
  providedIn: 'root'
})
export class MainMenuService implements iMainMenuService {

  constructor(private mainMenuInfrastructureService: MainMenuInfrastructureService,
              private mainMenuAttendanceTrackingService: MainMenuAttendanceTrackingService) { }

  

  public get Pages4SuperUser(): MainMenuItem[]  {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4SuperUser.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4SuperUser.forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;
    
  }

  public get Pages4Admin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4Admin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4Admin.forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4DealerAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4DealerAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4DealerAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4CompanyAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4CompanyAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4FinancialAdvisorAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4FinancialAdvisorAdmin.forEach((item) => {
      mainMenuItem.push(item);
    });

    return mainMenuItem;

  }

  public get Pages4User(): MainMenuItem[] {

    let mainMenuItem: MainMenuItem[] = [];

    this.mainMenuInfrastructureService.Pages4User.forEach((item) => {
      mainMenuItem.push(item);
    });
    this.mainMenuAttendanceTrackingService.Pages4User.forEach((item) => {
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
        case 'xx2':
          return 'xx2';
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
