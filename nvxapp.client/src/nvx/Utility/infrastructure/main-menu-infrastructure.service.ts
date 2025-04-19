import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem } from './main-menu.service';

@Injectable({
  providedIn: 'root'
})
export class MainMenuInfrastructureService implements iMainMenuService{

  constructor() { }

    public get Pages4SuperUser(): MainMenuItem[] {

      return [
        { title: 'SuperUser', component: "SuperUserPageComponent", url: '/superuser', icon: 'triangle' },
        { title: 'Dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
        { title: 'Users', component: "UserListPageComponent", url: '/userlist', icon: 'list-circle' }
      ];

  }
    public get Pages4Admin(): MainMenuItem[] {

    return [
      { title: 'PowerAdmin', component: "PowerAdminPageComponent", url: '/poweradmin', icon: 'square' },
      { title: 'Admin', component: "AdminPageComponent", url: '/admin', icon: 'square' },
      { title: 'Users', component: "UserListPageComponent", url: '/userlist', icon: 'list-circle' },
      { title: 'Dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
    ];

  }
    public get Pages4DealerAdmin(): MainMenuItem[] {

      return [
        { title: 'DealerPowerAdmin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
        { title: 'DealerAdmin', component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
        { title: 'Users Dealer', component: "UserDealerListPageComponent", url: '/userdealerlist', icon: 'list-circle' },
        { title: 'FinancialAdvisors', component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' },
        
      ];

    }
    public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {

      return [
        { title: 'FinancialAdvisorPowerAdmin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
        { title: 'FinancialAdvisorAdmin', component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
        { title: 'Users FinancialAdvisor', component: "UserFinancialAdvisorListPageComponent", url: '/userfinancialadvisorlist', icon: 'list-circle' },
        { title: 'Company', component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' },
      ];

    }
    public get Pages4CompanyAdmin(): MainMenuItem[] {

      return [
        { title: 'CompanyPowerAdmin', component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
        { title: 'CompanyAdmin', component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },
        { title: 'Users', component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'list-circle' },
      ];

    }
    public get Pages4User(): MainMenuItem[] {

      return [
        { title: 'Impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle' },
        { title: 'Home', component: "HomePageComponent", url: '/home', icon: 'home' },
        { title: 'Login', component: "LoginPageComponent", url: '/login', icon: 'enter' },
        { title: 'Logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit' },
        //soggette al login
        { title: 'User Data', component: "UserPageComponent", url: '/user', icon: 'person' },
      ];

    }

}
