import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Compliance (titoli = chiavi di traduzione cmp.menu.*). Gruppi 60-62: sezioni proprie, distinte da quelle degli
 altri applicativi. Le voci compaiono se l'applicativo e' attivo per l'azienda (vedi MainMenuService), quelle lato studio se
 e' abilitato per lo studio; i permessi veri li controlla il server.
    - amministratori dell'azienda: sicurezza sul lavoro, registro GDPR, i propri adempimenti, impostazioni (CompanyPowerAdmin)
    - dipendenti: i propri adempimenti (corsi, visite, DPI, nomine)
    - studio: registro GDPR e scadenze delle aziende dello studio
 Gli abilitati ai dati sanitari che non sono amministratori aprono le visite da /cmp81 (scelta provvisoria: nessuna voce propria).
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuComplianceService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {
    const studio = ['FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin'];
    return [
      { menuType: MenuType.MenuHeader, zorder: 620000, group: 62, title: 'cmp.menu.section', roles: studio, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 620100, group: 62, title: 'cmp.menu.studio', roles: studio, component: 'CmpListPageComponent', url: '/cmpstudio', icon: 'shield-checkmark' },
    ];
  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    const tutti = ['CompanyPowerAdmin', 'CompanyAdmin'];
    return [
      { menuType: MenuType.MenuHeader, zorder: 600000, group: 60, title: 'cmp.menu.section', roles: tutti, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 600050, group: 60, title: 'cmp.menu.safety', roles: tutti, component: 'CmpListPageComponent', url: '/cmp81', icon: 'medkit' },
      { menuType: MenuType.MenuItem, zorder: 600060, group: 60, title: 'cmp.menu.gdpr', roles: tutti, component: 'CmpListPageComponent', url: '/cmpgdpr', icon: 'lock-closed' },
      { menuType: MenuType.MenuItem, zorder: 600070, group: 60, title: 'cmp.menu.mine', roles: tutti, component: 'CmpMinePageComponent', url: '/cmpmine', icon: 'person-circle' },
      { menuType: MenuType.MenuItem, zorder: 600100, group: 60, title: 'cmp.menu.settings', roles: ['CompanyPowerAdmin'], component: 'CmpSettingsPageComponent', url: '/cmpsettings', icon: 'construct' },
    ];
  }

  // dipendenti: i propri corsi, visite (solo esito), DPI ricevuti e nomine
  public get Pages4User(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 610000, group: 61, title: 'cmp.menu.section', roles: ['User'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 610100, group: 61, title: 'cmp.menu.mine', component: 'CmpMinePageComponent', url: '/cmpmine', icon: 'person-circle' },
    ];
  }

  // sezione "Assistenza": manuale del modulo in PDF (solo con il modulo attivo, vedi MainMenuService.Pages4Support)
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuItem, zorder: 932, group: 0, title: 'cmp.menu.manual', component: 'UserManualPageComponent', url: '/usermanual/compliance', icon: 'book', nonConfigurabile: true },
    ];
  }

  public get GetWorkingMode(): WorkingMode { return WorkingMode.Compliance; }
}
