import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

@Injectable({
  providedIn: 'root'
})
export class MainMenuInfrastructureService implements iMainMenuService{

  constructor() { }

  public GetMenuHeaderMenu(MainMenuItem: MainMenuItem[], group: number = 0): MainMenuItem | undefined {

    let menu = MainMenuItem.find(x => x.menuType == MenuType.MenuHeader && x.group == group);

    if (menu == null)
        menu = MainMenuItem.find(x => x.menuType == MenuType.MenuHeader && x.group == 0);

    return menu;
  }



  public get Pages4SuperUser(): MainMenuItem[] {
    return [
      //{ menuType: MenuType.MenuHeader, zorder: 0, title: 'TITOLO MENU', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'menu.superUser', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'menu.superUserPage', component: "SuperUserPageComponent", url: '/superuser', icon: 'triangle' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'menu.dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.users', component: "UserListPageComponent", url: '/userlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.companyMove', component: "CompanyMovePageComponent", url: '/companymove', icon: 'swap-horizontal' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.ticketTypes', component: "TicketTypeListPageComponent", url: '/tickettypelist', icon: 'pricetags' },
      { menuType: MenuType.MenuItem, zorder: 600, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' }
    ];
  }

  public get Pages4Admin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, group: 0, title: 'menu.admin', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, group: 0, title: 'menu.powerAdmin', component: "PowerAdminPageComponent", url: '/poweradmin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 200, group: 0, title: 'menu.admin', component: "AdminPageComponent", url: '/admin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.users', component: "UserListPageComponent", url: '/userlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' }
    ];
  }

  public get Pages4DealerAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, group: 0, title: 'menu.dealer'            , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, group: 0, title: 'menu.dealerPowerAdmin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 200, group: 0, title: 'menu.dealerAdmin'      , component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.dealerUsers'     , component: "UserDealerListPageComponent", url: '/userdealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.financialAdvisors'             , component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' }
    ];
  }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'menu.financialAdvisor'            , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem  , zorder: 100, group: 0, title: 'menu.financialAdvisorPowerAdmin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem  , zorder: 200, group: 0, title: 'menu.financialAdvisorAdmin'      , component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem  , zorder: 300, group: 0, title: 'menu.financialAdvisorUsers'     , component: "UserFinancialAdvisorListPageComponent", url: '/userfinancialadvisorlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem  , zorder: 400, group: 0, title: 'menu.companies'           , component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' }
    ];
  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:    0,  group: 0, title: 'menu.general'                                           , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder:  100,  group: 0, title: 'menu.companyPowerAdmin', roles: ['CompanyPowerAdmin']  , component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem,   zorder:  200,  group: 0, title: 'menu.companyAdmin'      , roles: ['CompanyAdmin']       , component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem  , zorder: 3000,  group: 0, title: 'menu.users'             , roles: ['CompanyPowerAdmin']  , component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 3500, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' },
      // nella sezione Parametri (gruppo 1, intestazione dichiarata dal menu presenze), dopo le altre voci
      { menuType: MenuType.MenuItem  , zorder: 208000, group: 1, title: 'menu.menuRestriction'      , roles: ['CompanyPowerAdmin']  , component: "CompanyMenuRestrictionPageComponent", url: '/companymenurestriction', icon: 'options' }
    ];
  }

  public get Pages4User(): MainMenuItem[] {

    
    //const roles = ['SuperUser', 'PowerAdmin', 'Admin', 'DealerPowerAdmin', 'DealerAdmin', 'FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin', 'CompanyPowerAdmin', 'CompanyAdmin', 'User'];

    return [
      { menuType: MenuType.MenuHeader, zorder: 0,   group: 0, title: 'menu.access', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'menu.impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'menu.dashboard', component: "DashboardPageComponent", url: '/dashboard', icon: 'home', nonConfigurabile: true },
      // ticket di assistenza per chi non ha un menu di ruolo (dipendente): nella seconda sezione se c'e' (gruppo 1,
      // es. Presenze), altrimenti nella prima (groupMenu sposta nella prima le voci di un gruppo senza intestazione)
      { menuType: MenuType.MenuItem,   zorder: 990000, group: 1, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' },
      { menuType: MenuType.MenuItem,   zorder: 300, group: 0, title: 'menu.login', component: "LoginPageComponent", url: '/login', icon: 'enter', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 400, group: 0, title: 'menu.logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 500, group: 0, title: 'menu.userData', component: "UserPageComponent", url: '/user', icon: 'person', nonConfigurabile: true }
    ];
  }

  public get GetWorkingMode(): WorkingMode {    return WorkingMode.Infrastructure;  }

}
