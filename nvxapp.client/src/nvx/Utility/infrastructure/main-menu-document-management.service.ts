import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu del documentale (titoli = chiavi di traduzione documents.menu.*). Gruppi 40-42: sezioni proprie, distinte
 da quelle degli altri applicativi. Le voci lato studio (Pages4FinancialAdvisorAdmin) arriveranno con la Fase 2.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuDocumentManagementService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 400000, group: 40, title: 'documents.menu.section', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 400100, group: 40, title: 'documents.menu.archive', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: 'DocListPageComponent', url: '/doclist', icon: 'documents' },
      { menuType: MenuType.MenuItem, zorder: 400200, group: 40, title: 'documents.menu.employees', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: 'DocEmployeeListPageComponent', url: '/docemployeelist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 400700, group: 40, title: 'documents.menu.log', roles: ['CompanyPowerAdmin'], component: 'DocLogPageComponent', url: '/doclog', icon: 'list' },
      { menuType: MenuType.MenuItem, zorder: 400800, group: 40, title: 'documents.menu.storage', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: 'DocStoragePageComponent', url: '/docstorage', icon: 'pie-chart' },
      // "Configurazione documentale" (gruppo 42, subito dopo la sezione principale come "Configurazione CRM"): solo CompanyPowerAdmin
      { menuType: MenuType.MenuHeader, zorder: 405000, group: 42, title: 'documents.menu.settingsSection', roles: ['CompanyPowerAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 405100, group: 42, title: 'documents.menu.types', roles: ['CompanyPowerAdmin'], component: 'DocTypeListPageComponent', url: '/doctypelist', icon: 'pricetags' },
      { menuType: MenuType.MenuItem, zorder: 405200, group: 42, title: 'documents.menu.folders', roles: ['CompanyPowerAdmin'], component: 'DocFolderListPageComponent', url: '/docfolderlist', icon: 'folder' },
      { menuType: MenuType.MenuItem, zorder: 405300, group: 42, title: 'documents.menu.permissions', roles: ['CompanyPowerAdmin'], component: 'DocPermissionPageComponent', url: '/docpermissions', icon: 'key' },
      { menuType: MenuType.MenuItem, zorder: 405400, group: 42, title: 'documents.menu.settings', roles: ['CompanyPowerAdmin'], component: 'DocSettingsPageComponent', url: '/docsettings', icon: 'construct' },
    ];
  }

  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 410000, group: 41, title: 'documents.menu.section', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 410100, group: 41, title: 'documents.menu.mine', component: 'DocMinePageComponent', url: '/docmine', icon: 'folder-open' },
    ];
  }

  // sezione "Assistenza": manuale del documentale in PDF (pagina /usermanual/documents di Infrastructure)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 930, group: 0, title: 'documents.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/documents', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.DocumentManagement; }
}
