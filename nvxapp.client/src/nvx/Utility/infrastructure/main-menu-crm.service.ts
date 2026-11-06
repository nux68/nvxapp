import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu del CRM (titoli = chiavi di traduzione crm.menu.*). Gruppi 30-32: sezioni proprie, distinte da quelle degli
 altri applicativi. I permessi veri (ruoli e visibilita' dei commerciali) li controlla il server.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuCrmService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    const tutti = ['CompanyPowerAdmin', 'CompanyAdmin'];
    return [
      { menuType: MenuType.MenuHeader, zorder: 300000, group: 30, title: 'crm.menu.section', roles: tutti, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 300100, group: 30, title: 'crm.menu.customers', roles: tutti, component: 'CrmCustomerListPageComponent', url: '/crmcustomerlist', icon: 'business' },
      { menuType: MenuType.MenuItem, zorder: 300200, group: 30, title: 'crm.menu.leads', roles: tutti, component: 'CrmLeadListPageComponent', url: '/crmleadlist', icon: 'person-add' },
      { menuType: MenuType.MenuItem, zorder: 300300, group: 30, title: 'crm.menu.pipeline', roles: tutti, component: 'CrmPipelinePageComponent', url: '/crmpipeline', icon: 'git-network' },
      { menuType: MenuType.MenuItem, zorder: 300400, group: 30, title: 'crm.menu.activities', roles: tutti, component: 'CrmActivityListPageComponent', url: '/crmactivitylist', icon: 'calendar' },
      { menuType: MenuType.MenuItem, zorder: 300500, group: 30, title: 'crm.menu.offers', roles: tutti, component: 'CrmOfferListPageComponent', url: '/crmofferlist', icon: 'document-text' },
      { menuType: MenuType.MenuItem, zorder: 300600, group: 30, title: 'crm.menu.report', roles: tutti, component: 'CrmReportPageComponent', url: '/crmreport', icon: 'bar-chart' },
      { menuType: MenuType.MenuHeader, zorder: 310000, group: 31, title: 'crm.menu.settingsSection', roles: tutti, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 310100, group: 31, title: 'crm.menu.tables', roles: ['CompanyPowerAdmin'], component: 'CrmParListPageComponent', url: '/crmparlist', icon: 'list' },
      { menuType: MenuType.MenuItem, zorder: 310200, group: 31, title: 'crm.menu.settings', roles: ['CompanyPowerAdmin'], component: 'CrmSettingsPageComponent', url: '/crmsettings', icon: 'construct' },
      { menuType: MenuType.MenuItem, zorder: 310300, group: 31, title: 'crm.menu.reassign', roles: tutti, component: 'CrmReassignPageComponent', url: '/crmreassign', icon: 'swap-horizontal' },
    ];
  }

  // commerciali: lavorano sui clienti che vedono (regola di visibilita' del CRM)
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 320000, group: 32, title: 'crm.menu.section', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 320100, group: 32, title: 'crm.menu.activities', component: 'CrmActivityListPageComponent', url: '/crmactivitylist', icon: 'calendar' },
      { menuType: MenuType.MenuItem, zorder: 320200, group: 32, title: 'crm.menu.customers', component: 'CrmCustomerListPageComponent', url: '/crmcustomerlist', icon: 'business' },
      { menuType: MenuType.MenuItem, zorder: 320300, group: 32, title: 'crm.menu.leads', component: 'CrmLeadListPageComponent', url: '/crmleadlist', icon: 'person-add' },
      { menuType: MenuType.MenuItem, zorder: 320400, group: 32, title: 'crm.menu.pipeline', component: 'CrmPipelinePageComponent', url: '/crmpipeline', icon: 'git-network' },
      { menuType: MenuType.MenuItem, zorder: 320500, group: 32, title: 'crm.menu.offers', component: 'CrmOfferListPageComponent', url: '/crmofferlist', icon: 'document-text' },
      { menuType: MenuType.MenuItem, zorder: 320600, group: 32, title: 'crm.menu.report', component: 'CrmReportPageComponent', url: '/crmreport', icon: 'bar-chart' },
    ];
  }

  // sezione "Assistenza": manuale del CRM in PDF (solo con il CRM attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 920, group: 0, title: 'crm.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/crm', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.Crm; }
}
