import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Risorse Umane (titoli = chiavi di traduzione hr.menu.*). Gruppo 50: sezione propria, distinta da quelle degli
 altri applicativi. Le voci compaiono se l'applicativo e' attivo per l'azienda (vedi MainMenuService); i permessi veri li
 controlla il server.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuHumanResourcesService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 500000, group: 50, title: 'hr.menu.section', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 500100, group: 50, title: 'hr.menu.settings', roles: ['CompanyPowerAdmin'], component: 'HrSettingsPageComponent', url: '/hrsettings', icon: 'construct' },
    ];
  }

  public get Pages4User(): MainMenuItem[] { return []; }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 931, group: 0, title: 'hr.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/humanresources', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.HumanResources; }
}
