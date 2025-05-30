import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType } from './main-menu.service';

@Injectable({
  providedIn: 'root'
})
export class MainMenuAttendanceTrackingService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {

    return [
      { menuType: MenuType.MenuItem, zorder: 1000,   title: 'Richieste Admin'  , roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "JustificationListPageComponent", url: '/requestlistadmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 2000,   title: 'Calendario HR'    , roles: ['CompanyPowerAdmin']                , component: "TimeSheetPowerAdminPageComponent", url: '/poweradmintimesheet', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 2000,   title: 'Calendario'       , roles: ['CompanyAdmin']                     , component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 3000,   title: 'Utenti reparto'   , roles: ['CompanyAdmin']                     , component: "UserDepartmentListPageComponent", url: '/userdepartmentlist', icon: 'people' },

      { menuType: MenuType.MenuNote, zorder: 100000, title: 'Parametri'                                                      , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100100, title: 'Giustificativi' , roles: ['CompanyPowerAdmin']                  , component: "JustificationListPageComponent", url: '/justificationlist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 100200, title: 'Reparti'        , roles: ['CompanyPowerAdmin']                  , component: "DepartmentListPageComponent", url: '/departmentlist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 100300, title: 'Configurazione' , roles: ['CompanyPowerAdmin']                  , component: "CompanyCfgPageComponent", url: '/companycfgedit', icon: 'ellipse' },
      
    ];

  }
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 0,   title: 'Richieste', component: "RequestListUserPageComponent", url: '/requestlistuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'Calendario', component: "TimeSheetUserPageComponent", url: '/usertimesheet', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Tmbratura', component: "TimeClockUserPageComponent", url: '/timeclockuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 300, title: 'Richiesta giustificativo', component: "RequestJustificationUserPageComponent", url: '/requestjustificationuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 400, title: 'Richiesta timbratura', component: "RequestClockingUserPageComponent", url: '/requestclockinguser', icon: 'person' },
    ];
  }

}
