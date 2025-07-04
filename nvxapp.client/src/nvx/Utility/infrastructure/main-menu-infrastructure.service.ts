import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';

@Injectable({
  providedIn: 'root'
})
export class MainMenuInfrastructureService implements iMainMenuService{

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, title: 'TITOLO MENU', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'SuperUser', component: "SuperUserPageComponent", url: '/superuser', icon: 'triangle' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 300, title: 'Users', component: "UserListPageComponent", url: '/userlist', icon: 'list-circle' }
    ];
  }

  public get Pages4Admin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, title: 'Admin', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'PowerAdmin', component: "PowerAdminPageComponent", url: '/poweradmin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Admin', component: "AdminPageComponent", url: '/admin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 300, title: 'Utenti', component: "UserListPageComponent", url: '/userlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 400, title: 'Centri', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' }
    ];
  }

  public get Pages4DealerAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, title: 'Centro', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'Centro Power Admin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Centro Admin', component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 300, title: 'Utenti Centro', component: "UserDealerListPageComponent", url: '/userdealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 400, title: 'Studi', component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' }
    ];
  }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, title: 'Studio', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'Studio Power Admin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Studio Admin', component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 300, title: 'Utenti Studio', component: "UserFinancialAdvisorListPageComponent", url: '/userfinancialadvisorlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 400, title: 'Aziende', component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' }
    ];
  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, title: 'Azienda'                                            , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, title: 'Power Admin', roles: ['CompanyPowerAdmin']  , component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 200, title: 'Admin'      , roles: ['CompanyAdmin']       , component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 3000, title: 'Utenti'            , roles: ['CompanyPowerAdmin' ] , component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'list-circle' }
    ];
  }

  public get Pages4User(): MainMenuItem[] {

    
    //const roles = ['SuperUser', 'PowerAdmin', 'Admin', 'DealerPowerAdmin', 'DealerAdmin', 'FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin', 'CompanyPowerAdmin', 'CompanyAdmin', 'User'];

    return [
      { menuType: MenuType.MenuItem, zorder: 100,  title: 'Impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle' },
      { menuType: MenuType.MenuItem, zorder: 200,  title: 'Home', component: "HomePageComponent", url: '/home', icon: 'home' },
      { menuType: MenuType.MenuItem, zorder: 300,  title: 'Login', component: "LoginPageComponent", url: '/login', icon: 'enter' },
      { menuType: MenuType.MenuItem, zorder: 400,  title: 'Logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit' },
      { menuType: MenuType.MenuItem, zorder: 500,  title: 'User Data', component: "UserPageComponent", url: '/user', icon: 'person' }
    ];
  }

  public get GetWorkingMode(): WorkingMode {    return WorkingMode.Infrastructure;  }

}
