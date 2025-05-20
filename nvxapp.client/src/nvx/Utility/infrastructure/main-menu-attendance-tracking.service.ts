import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem } from './main-menu.service';

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
      { title: 'Request List Admin', component: "JustificationListPageComponent", url: '/requestlistadmin', icon: 'ellipse' },
      { title: 'Admin Time Sheet', component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' },
      { title: 'Power Admin Time Sheet', component: "TimeSheetPowerAdminPageComponent", url: '/poweradmintimesheet', icon: 'ellipse' },
      { title: 'Justifications', component: "JustificationListPageComponent", url: '/justificationlist', icon: 'ellipse' },
      { title: 'Departments', component: "DepartmentListPageComponent", url: '/departmentlist', icon: 'ellipse' },
      { title: 'Company cfg', component: "CompanyCfgPageComponent", url: '/companycfgedit', icon: 'ellipse' },
    ];

  }
  public get Pages4User(): MainMenuItem[] {
    return [
      { title: 'Request List User', component: "RequestListUserPageComponent", url: '/requestlistuser', icon: 'person' },
      { title: 'User Time Sheet', component: "TimeSheetUserPageComponent", url: '/usertimesheet', icon: 'person' },
      { title: 'Time Clock User', component: "TimeClockUserPageComponent", url: '/timeclockuser', icon: 'person' },
      { title: 'Request Justification User', component: "RequestJustificationUserPageComponent", url: '/requestjustificationuser', icon: 'person' },
      { title: 'Request Clocking User', component: "RequestClockingUserPageComponent", url: '/requestclockinguser', icon: 'person' },
      
    ];
  }

}
