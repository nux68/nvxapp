import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Compliance (titoli = chiavi di traduzione cmp.menu.*). Gruppo 60: sezione propria, distinta da quelle degli
 altri applicativi. Le voci compaiono se l'applicativo e' attivo per l'azienda (vedi MainMenuService); i permessi veri li
 controlla il server.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuComplianceService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 600000, group: 60, title: 'cmp.menu.section', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 600100, group: 60, title: 'cmp.menu.settings', roles: ['CompanyPowerAdmin'], component: 'CmpSettingsPageComponent', url: '/cmpsettings', icon: 'construct' },
    ];
  }

  public get Pages4User(): MainMenuItem[] { return []; }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 932, group: 0, title: 'cmp.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/compliance', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.Compliance; }
}
