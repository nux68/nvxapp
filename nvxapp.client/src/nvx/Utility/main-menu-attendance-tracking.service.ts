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
  public get Pages4CompanyAdmin(): MainMenuItem[] { return []; }
  public get Pages4User(): MainMenuItem[] { return []; }

}
