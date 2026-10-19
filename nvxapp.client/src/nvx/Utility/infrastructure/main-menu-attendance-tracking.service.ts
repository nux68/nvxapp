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
      { menuType: MenuType.MenuItem, zorder: 2100, group: 0, title: 'Richieste', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "JustificationListPageComponent", url: '/requestlistadmin', icon: 'create' },
      { menuType: MenuType.MenuItem, zorder: 2200, group: 0, title: 'Calendario HR', roles: ['CompanyPowerAdmin'], component: "TimeSheetPowerAdminPageComponent", url: '/poweradmintimesheet', icon: 'calendar-number' },
      { menuType: MenuType.MenuItem, zorder: 2250, group: 0, title: 'Quadratura mensile', roles: ['CompanyPowerAdmin'], component: "QuadraturaMensilePageComponent", url: '/quadraturamensile', icon: 'checkmark-done-circle' },

      { menuType: MenuType.MenuItem, zorder: 2400, group: 0, title: 'Export causali', roles: ['CompanyPowerAdmin'], component: "ExportCausaliPageComponent", url: '/exportcausali', icon: 'cloud-download' },
      { menuType: MenuType.MenuItem, zorder: 2500, group: 0, title: 'Statistiche attività', roles: ['CompanyPowerAdmin'], component: "ActivityStatisticsPageComponent", url: '/activitystatistics', icon: 'bar-chart' },

      { menuType: MenuType.MenuItem, zorder: 2600, group: 0, title: 'Personale presente', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "PresentStaffPageComponent", url: '/presentstaff', icon: 'people-circle' },
      { menuType: MenuType.MenuItem, zorder: 2700, group: 0, title: 'Piano ferie', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "VacationPlanPageComponent", url: '/vacationplan', icon: 'calendar-clear' },


      { menuType: MenuType.MenuItem, zorder: 2300, group: 0, title: 'Calendario', roles: ['CompanyAdmin'], component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 3000, group: 0, title: 'Utenti ', roles: ['CompanyAdmin'], component: "UserDepartmentListPageComponent", url: '/userdepartmentlist', icon: 'people' },

      // Parametri: ultima sezione del menu (le sezioni seguono lo zorder dell'intestazione)
      { menuType: MenuType.MenuHeader, zorder: 900000, group: 1, title: 'Parametri', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 202000, group: 1, title: 'Giustificativi', roles: ['CompanyPowerAdmin'], component: "JustificationListPageComponent", url: '/justificationlist', icon: 'reader' },
      { menuType: MenuType.MenuItem, zorder: 203000, group: 1, title: 'Causali', roles: ['CompanyPowerAdmin'], component: "CausaliListPageComponent", url: '/causalilist', icon: 'receipt' },
      { menuType: MenuType.MenuItem, zorder: 204000, group: 1, title: 'Configurazione', roles: ['CompanyPowerAdmin'], component: "CompanyCfgPageComponent", url: '/companycfgedit', icon: 'construct' },
      { menuType: MenuType.MenuItem, zorder: 205000, group: 1, title: 'Orari', roles: ['CompanyPowerAdmin'], component: "OrariListPageComponent", url: '/orarilist', icon: 'time' },
      { menuType: MenuType.MenuItem, zorder: 206000, group: 1, title: 'Profili Orari', roles: ['CompanyPowerAdmin'], component: "ProfiloOrarioListPageComponent", url: '/profiliorarilist', icon: 'timer' },
      { menuType: MenuType.MenuItem, zorder: 206500, group: 1, title: 'Festività', roles: ['CompanyPowerAdmin'], component: "FestivitaListPageComponent", url: '/festivitalist', icon: 'flag' },
      { menuType: MenuType.MenuItem, zorder: 207000, group: 1, title: 'Modelli Export', roles: ['CompanyPowerAdmin'], component: "ExportCauListPageComponent", url: '/exportcaulist', icon: 'hammer' },
      { menuType: MenuType.MenuItem, zorder: 207500, group: 1, title: 'Tipi spesa', roles: ['CompanyPowerAdmin'], component: "TipoSpesaListPageComponent", url: '/tipospesalist', icon: 'pricetags' },

      { menuType: MenuType.MenuHeader, zorder: 300000, group: 3, title: 'Nota spese', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 301000, group: 3, title: 'Esame spese', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "NotaSpeseAdminPageComponent", url: '/notaspeseadmin', icon: 'receipt' },


      { menuType: MenuType.MenuHeader, zorder: 205000, group: 2, title: 'Gestione Commesse', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 205100, group: 2, title: 'Commesse', roles: ['CompanyPowerAdmin'], component: "CommessaListPageComponent", url: '/commessalist', icon: 'layers' },
      { menuType: MenuType.MenuItem, zorder: 205200, group: 2, title: 'Clienti', roles: ['CompanyPowerAdmin'], component: "CustomerListPageComponent", url: '/customerlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 205300, group: 2, title: 'Reparti', roles: ['CompanyPowerAdmin'], component: "DepartmentListPageComponent", url: '/departmentlist', icon: 'grid' },
      { menuType: MenuType.MenuItem, zorder: 205400, group: 2, title: 'Sedi', roles: ['CompanyPowerAdmin'], component: "AzSediListPageComponent", url: '/azsedilist', icon: 'business' },
      { menuType: MenuType.MenuItem, zorder: 205500, group: 2, title: 'Attività', roles: ['CompanyPowerAdmin'], component: "ActivityListPageComponent", url: '/activitylist', icon: 'file-tray-full' },
      { menuType: MenuType.MenuItem, zorder: 205600, group: 2, title: 'Competenze', roles: ['CompanyPowerAdmin'], component: "CompetenceListPageComponent", url: '/competencelist', icon: 'file-tray-stacked' },

      //{ menuType: MenuType.MenuItem, zorder: 205500, title: 'MyTemplate1', roles: ['CompanyPowerAdmin'], component: "MyTemplate1ListPageComponent", url: '/mytemplate1list', icon: 'ellipse' },



    ];

  }
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 1000, group: 1, title: 'Presenze', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 2000, group: 1, title: 'Timbratura', component: "TimeClockUserPageComponent", url: '/timeclockuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 3000, group: 1, title: 'Richieste', component: "RequestListUserPageComponent", url: '/requestlistuser', icon: 'create' },
      { menuType: MenuType.MenuItem, zorder: 4000, group: 1, title: 'Calendario', component: "TimeSheetUserPageComponent", url: '/usertimesheet', icon: 'calendar-number' },
      { menuType: MenuType.MenuItem, zorder: 5000, group: 1, title: 'Richiesta giustificativo', component: "RequestJustificationUserPageComponent", url: '/requestjustificationuser', icon: 'person' },
      { menuType: MenuType.MenuItem, zorder: 6000, group: 1, title: 'Richiesta timbratura', component: "RequestClockingUserPageComponent", url: '/requestclockinguser', icon: 'person' },

      { menuType: MenuType.MenuHeader, zorder: 7000, group: 2, title: 'Nota spese', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 7100, group: 2, title: 'Richiesta rimborso', component: "NotaSpesaRequestUserPageComponent", url: '/notaspesarequestuser', icon: 'wallet' },
      { menuType: MenuType.MenuItem, zorder: 7200, group: 2, title: 'Le mie note spese', component: "NotaSpeseUserPageComponent", url: '/notaspeseuser', icon: 'receipt' },
    ];
  }
  public get GetWorkingMode(): WorkingMode { return WorkingMode.AttendanceTracking; }
}
