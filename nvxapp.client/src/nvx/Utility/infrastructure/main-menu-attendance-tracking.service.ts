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
      // tutti e due
      { title: 'Request List Admin', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "JustificationListPageComponent", url: '/requestlistadmin', icon: 'ellipse' },
      //solo CompanyPowerAdmin
      { title: 'Power Admin Time Sheet', roles: ['CompanyPowerAdmin'], component: "TimeSheetPowerAdminPageComponent", url: '/poweradmintimesheet', icon: 'ellipse' },
      { title: 'Justifications', roles: ['CompanyPowerAdmin'], component: "JustificationListPageComponent", url: '/justificationlist', icon: 'ellipse' },
      { title: 'Departments', roles: ['CompanyPowerAdmin'], component: "DepartmentListPageComponent", url: '/departmentlist', icon: 'ellipse' },
      
      { title: 'Company cfg', roles: ['CompanyPowerAdmin'], component: "CompanyCfgPageComponent", url: '/companycfgedit', icon: 'ellipse' },
      
      //solo CompanyAdmin
      { title: 'Admin Time Sheet', roles: ['CompanyAdmin'], component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' },
      { title: 'User Department List', roles: ['CompanyAdmin'], component: "UserDepartmentListPageComponent", url: '/userdepartmentlist', icon: 'people' },
      
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
