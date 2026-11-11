import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Risorse aziendali (titoli = chiavi di traduzione ast.menu.*). Gruppo 70: sezione propria, distinta da quelle degli
 altri applicativi. Le voci compaiono se l'applicativo e' attivo per l'azienda (vedi MainMenuService); i permessi veri li
 controlla il server.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuAssetsService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 700000, group: 70, title: 'ast.menu.section', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 700100, group: 70, title: 'ast.menu.settings', roles: ['CompanyPowerAdmin'], component: 'AstSettingsPageComponent', url: '/astsettings', icon: 'construct' },
    ];
  }

  public get Pages4User(): MainMenuItem[] { return []; }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 933, group: 0, title: 'ast.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/assets', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.Assets; }
}
