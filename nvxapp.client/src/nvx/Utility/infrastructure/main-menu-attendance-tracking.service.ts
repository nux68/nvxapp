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
      { title: 'Admin Time Sheet', component: "TimeSheetAdminPageComponent", url: '/admintimesheet', icon: 'ellipse' }
    ];

  }
  public get Pages4User(): MainMenuItem[] {
    return [
      { title: 'User Time Sheet', component: "TimeSheetUserPageComponent", url: '/usertimesheet', icon: 'person' },

      { title: 'Time Clock User', component: "TimeClockUserPageComponent", url: '/timeclockuser', icon: 'person' },

      { title: 'Request Justification User', component: "RequestJustificationUserPageComponent", url: '/requestjustificationuser', icon: 'person' },
      { title: 'Request Clocking User', component: "RequestClockingUserPageComponent", url: '/requestclockinguser', icon: 'person' },
    ];
  }

}
