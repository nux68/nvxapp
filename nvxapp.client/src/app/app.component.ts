import { Component, OnInit } from '@angular/core';
import { AccountService } from '../nvx/ClientServer-Service/Account/account.service';
import { AuthService } from '../nvx/Utility/auth.service';
import { UserNavigationService } from '../nvx/Utility/user-navigation.service';
import { SignalrService } from '../nvx/Utility/signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  standalone: false,
})
export class AppComponent implements OnInit {

  

  public appPages4SuperUser = [

    { title: 'SuperUser'  , component:"SuperUserPageComponent"  , url: '/superuser', icon: 'triangle' },
    { title: 'Dealer List', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },

  ];


  public appPages4Admin = [

    { title: 'PowerAdmin', component: "PowerAdminPageComponent", url: '/poweradmin', icon: 'square' },
    { title: 'Admin', component: "AdminPageComponent", url: '/admin', icon: 'square' },
    { title: 'Dealer List', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },

  ];

  public appPages4DealerAdmin = [

    { title: 'DealerPowerAdmin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
    { title: 'DealerAdmin', component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
    { title: 'FinancialAdvisor List', component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' },
  ];

  public appPages4FinancialAdvisorAdmin = [

    { title: 'FinancialAdvisorPowerAdmin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
    { title: 'FinancialAdvisorAdmin', component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
    { title: 'Company List', component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' },
  ];

  public appPages4CompanyAdmin = [

    { title: 'CompanyPowerAdmin', component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
    { title: 'CompanyAdmin', component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },
    { title: 'User List', component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'list-circle' },

  ];


  public appPages4User = [

    { title: 'Impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle' },
    { title: 'Home', component: "HomePageComponent", url: '/home', icon: 'home' },
    { title: 'Login', component: "LoginPageComponent", url: '/login', icon: 'enter' },
    { title: 'Logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit' },
    //soggette al login
    { title: 'User Data', component: "UserPageComponent", url: '/user', icon: 'person' },

  ];



  public appPages = [


    { title: 'Inbox', url: '/folder/inbox', icon: 'mail' },
    { title: 'Outbox', url: '/folder/outbox', icon: 'paper-plane' },
    { title: 'Favorites', url: '/folder/favorites', icon: 'heart' },
    { title: 'Archived', url: '/folder/archived', icon: 'archive' },
    { title: 'Trash', url: '/folder/trash', icon: 'trash' },
    { title: 'Spam', url: '/folder/spam', icon: 'warning' },
  ];
  public labels = ['Family', 'Friends', 'Notes', 'Work', 'Travel', 'Reminders'];
  constructor(public authService: AuthService,
              public userNavigationService: UserNavigationService,
              public signalrService: SignalrService
              )
  {
  }

  ngOnInit() {

    this.signalrService.IsConnect$.subscribe(res => {

      if (res == true) {
        // 🔹 Sottoscrizione agli eventi della chat
        this.signalrService.on('ReceiveMessage').subscribe((msg: string) => {
          console.log('🔄 Dati ricevuti:', msg);
        });

        // 🔹 Esempio: Sottoscrizione a un altro evento (es. aggiornamento dati)
        this.signalrService.on('UpdateData').subscribe((data) => {
          console.log('🔄 Dati aggiornati:', data);
        });
      }

    });

    


    

  }

  public showPage4SuperUser(component: string): boolean {
    return true;
  }

  public showPage4Admin(component: string): boolean {

    switch (component) {
      case 'PowerAdminPageComponent':
        return this.authService.IsPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupAdmin;
        break;
    }
  }

  public showPage4DealerAdmin(component: string): boolean {

    switch (component) {
      case 'DealerPowerAdminPageComponent':
        return this.authService.IsDealerPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupDealerAdmin;
        break;
    }
  }
  
  public showPage4FinancialAdvisorAdmin(component: string): boolean {

    switch (component) {
      case 'FinancialAdvisorPowerAdminPageComponent':
        return this.authService.IsFinancialAdvisorPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupFinancialAdvisorAdmin;
        break;
    }
  }

  public showPage4CompanyAdmin(component: string): boolean {

    switch (component) {
      case 'CompanyPowerAdminPageComponent':
        return this.authService.IsCompanyPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupCompanyAdmin;
        break;
    }
  }

  public showPage4User(component: string): boolean {


    

    switch (component) {

      case 'HomePageComponent':
        return true;
        break;

      case 'LoginPageComponent':
        if (this.authService.Token != null)
          return false;
        else
          return true;
        break;

      case 'LogoutPageComponent':
        if(this.authService.Token==null)
          return false;
        else
          if (this.userNavigationService.UserCanGoBack)
            return false;
          else
            return true;
        break;

      case 'UserImpersonatePageComponent':
        if (this.userNavigationService.UserCanGoBack)
          return true;
        else
          return false;
        break;


      default:

        if (this.authService.IsUser)
          return true;
        else
          return false;
        break;
    }

    
  }


}
