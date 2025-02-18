import { Component } from '@angular/core';
import { AccountService } from '../nvx/ClientServer-Service/Account/account.service';
import { AuthService } from '../nvx/Utility/auth.service';
import { UserNavigationService } from '../nvx/Utility/user-navigation.service';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  standalone: false,
})
export class AppComponent {

  public IconBack: string = "play-back";

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

  ];



  public appPages4CompanyAdmin = [

    { title: 'CompanyPowerAdmin', component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'ellipse' },
    { title: 'CompanyAdmin', component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'ellipse' },

  ];


  public appPages4User = [

    { title: 'Impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle' },
    { title: 'Home', component: "HomePageComponent", url: '/home', icon: 'home' },
    { title: 'Login', component: "LoginPageComponent", url: '/login', icon: 'enter' },
    { title: 'Logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit' },
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
              public userNavigationService: UserNavigationService
              )
  {
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
        return true;
        break;

      case 'LogoutPageComponent':
        if(this.authService.Token==null)
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
        return false;
        break;
    }

    
  }


}
