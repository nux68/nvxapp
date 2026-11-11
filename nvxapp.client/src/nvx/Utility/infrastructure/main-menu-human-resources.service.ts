import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Risorse Umane (titoli = chiavi di traduzione hr.menu.*). Gruppo 50: sezione propria, distinta da quelle degli
 altri applicativi. Le voci compaiono se l'applicativo e' attivo per l'azienda (vedi MainMenuService); quelle dello studio
 (Pages4FinancialAdvisorAdmin) se l'applicativo e' abilitato per lo studio. I permessi veri li controlla il server.
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuHumanResourcesService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }

  // studio paghe: caricamento e verifica dei cedolini delle aziende dello studio (HR-CED)
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 500000, group: 50, title: 'hr.menu.section', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 500050, group: 50, title: 'hr.menu.studioPayslips', component: 'HrPayslipListPageComponent', url: '/hrstudiopayslips', icon: 'documents' },
    ];
  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    const tutti = ['CompanyPowerAdmin', 'CompanyAdmin'];
    return [
      { menuType: MenuType.MenuHeader, zorder: 500000, group: 50, title: 'hr.menu.section', roles: tutti, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 500010, group: 50, title: 'hr.menu.employees', roles: tutti, component: 'HrEmployeeListPageComponent', url: '/hremployeelist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 500020, group: 50, title: 'hr.menu.payslips', roles: tutti, component: 'HrPayslipListPageComponent', url: '/hrpayslips', icon: 'documents' },
      { menuType: MenuType.MenuItem, zorder: 500030, group: 50, title: 'hr.menu.courses', roles: tutti, component: 'HrCourseListPageComponent', url: '/hrcourselist', icon: 'school' },
      { menuType: MenuType.MenuItem, zorder: 500040, group: 50, title: 'hr.menu.plans', roles: tutti, component: 'HrPlanListPageComponent', url: '/hrplanlist', icon: 'clipboard' },
      { menuType: MenuType.MenuItem, zorder: 500050, group: 50, title: 'hr.menu.checklists', roles: tutti, component: 'HrChecklistListPageComponent', url: '/hrchecklistlist', icon: 'checkbox' },
      { menuType: MenuType.MenuItem, zorder: 500060, group: 50, title: 'hr.menu.templates', roles: ['CompanyPowerAdmin'], component: 'HrChecklistTemplateListPageComponent', url: '/hrchecklisttemplatelist', icon: 'list' },
      { menuType: MenuType.MenuItem, zorder: 500100, group: 50, title: 'hr.menu.settings', roles: ['CompanyPowerAdmin'], component: 'HrSettingsPageComponent', url: '/hrsettings', icon: 'construct' },
    ];
  }

  // area del dipendente: i propri cedolini e CU (senza dipendente collegato la pagina lo dice)
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 500500, group: 50, title: 'hr.menu.myPayslips', component: 'HrMyPayslipsPageComponent', url: '/hrmypayslips', icon: 'wallet' },
    ];
  }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 931, group: 0, title: 'hr.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/humanresources', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.HumanResources; }
}
