import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

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
      { menuType: MenuType.MenuItem, zorder: 2100,   title: 'Richieste'        , roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "JustificationListPageComponent", url: '/requestlistadmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 2200,   title: 'Calendario HR'    , roles: ['CompanyPowerAdmin']                , component: "TimeSheetPowerAdminPageComponent", url: '/poweradmintimesheet', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 2300,   title: 'Calendario'       , roles: ['CompanyAdmin']                     , component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 3000,   title: 'Utenti '          , roles: ['CompanyAdmin']                     , component: "UserDepartmentListPageComponent", url: '/userdepartmentlist', icon: 'people' },

      { menuType: MenuType.MenuNote, zorder: 201000, title: 'Parametri'        , roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 202000, title: 'Giustificativi'   , roles: ['CompanyPowerAdmin'], component: "JustificationListPageComponent", url: '/justificationlist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 203000, title: 'Causali'          , roles: ['CompanyPowerAdmin'], component: "CausaliListPageComponent", url: '/causalilist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 204000, title: 'Configurazione'   , roles: ['CompanyPowerAdmin'], component: "CompanyCfgPageComponent", url: '/companycfgedit', icon: 'ellipse' },

      { menuType: MenuType.MenuNote, zorder: 205000, title: 'Gestione Commesse' , roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 205100, title: 'Commesse'          , roles: ['CompanyPowerAdmin'], component: "CommessaListPageComponent", url: '/commessalist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 205200, title: 'Clienti'           , roles: ['CompanyPowerAdmin'], component: "CustomerListPageComponent", url: '/customerlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 205300, title: 'Reparti'           , roles: ['CompanyPowerAdmin'], component: "DepartmentListPageComponent", url: '/departmentlist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 205400, title: 'Sedi'              , roles: ['CompanyPowerAdmin'], component: "AzSediListPageComponent", url: '/azsedilist', icon: 'business' },
      { menuType: MenuType.MenuItem, zorder: 205500, title: 'Attività'          , roles: ['CompanyPowerAdmin'], component: "ActivityListPageComponent", url: '/activitylist', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 205600, title: 'Competenze'        , roles: ['CompanyPowerAdmin'], component: "CompetenceListPageComponent", url: '/competencelist', icon: 'ellipse' },

      //{ menuType: MenuType.MenuItem, zorder: 205500, title: 'MyTemplate1', roles: ['CompanyPowerAdmin'], component: "MyTemplate1ListPageComponent", url: '/mytemplate1list', icon: 'ellipse' },
      
      

    ];

  }
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuNote, zorder: 1000, title: 'Presenze', roles: ['User'] , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 2000, title: 'Timbratura'                , component: "TimeClockUserPageComponent", url: '/timeclockuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 3000, title: 'Richieste'                 , component: "RequestListUserPageComponent", url: '/requestlistuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 4000, title: 'Calendario'                , component: "TimeSheetUserPageComponent", url: '/usertimesheet', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 5000, title: 'Richiesta giustificativo'  , component: "RequestJustificationUserPageComponent", url: '/requestjustificationuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 6000, title: 'Richiesta timbratura'      , component: "RequestClockingUserPageComponent", url: '/requestclockinguser', icon: 'person' },
    ];
  }
  public get GetWorkingMode(): WorkingMode { return WorkingMode.AttendanceTracking; }
}
