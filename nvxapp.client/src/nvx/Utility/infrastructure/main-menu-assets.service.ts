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
    const tutti = ['CompanyPowerAdmin', 'CompanyAdmin'];
    return [
      { menuType: MenuType.MenuHeader, zorder: 700000, group: 70, title: 'ast.menu.section', roles: tutti, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 700100, group: 70, title: 'ast.menu.vehicles', roles: tutti, component: 'AstVehicleListPageComponent', url: '/astvehicles', icon: 'car' },
      { menuType: MenuType.MenuItem, zorder: 700200, group: 70, title: 'ast.menu.goods', roles: tutti, component: 'AstGoodsListPageComponent', url: '/astgoods', icon: 'cube' },
      { menuType: MenuType.MenuItem, zorder: 700800, group: 70, title: 'ast.menu.mine', roles: tutti, component: 'AstMinePageComponent', url: '/astmine', icon: 'person-circle' },
      { menuType: MenuType.MenuItem, zorder: 700900, group: 70, title: 'ast.menu.settings', roles: ['CompanyPowerAdmin'], component: 'AstSettingsPageComponent', url: '/astsettings', icon: 'construct' },
    ];
  }

  // dipendenti: le proprie risorse (beni e verbali, veicolo, multe)
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 710000, group: 71, title: 'ast.menu.section', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 710100, group: 71, title: 'ast.menu.mine', component: 'AstMinePageComponent', url: '/astmine', icon: 'person-circle' },
    ];
  }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 933, group: 0, title: 'ast.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/assets', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.Assets; }
}
