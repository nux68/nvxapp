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
        { title: 'Dealer Power Admin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
        { title: 'Dealer Admin', component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
        { title: 'Users Dealer', component: "UserDealerListPageComponent", url: '/userdealerlist', icon: 'list-circle' },
        { title: 'Financial Advisors', component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' },
        
      ];

    }
    public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {

      return [
        { title: 'Financial Advisor Power Admin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
        { title: 'Financial Advisor Admin', component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
        { title: 'Users Financial Advisor', component: "UserFinancialAdvisorListPageComponent", url: '/userfinancialadvisorlist', icon: 'list-circle' },
        { title: 'Companies', component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' },
      ];

    }
    public get Pages4CompanyAdmin(): MainMenuItem[] {

      

      return [
        { title: 'Company Power Admin', roles: ['CompanyPowerAdmin', 'CompanyAdmin'],component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
        { title: 'Company Admin'      , roles: ['CompanyPowerAdmin', 'CompanyAdmin'],component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },
        { title: 'Users'              , roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'list-circle' },
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
