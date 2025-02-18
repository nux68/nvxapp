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

    { title: 'SuperUser', url: '/superuser', icon: 'triangle' },

  ];

  

  public appPages4Admin = [

    { title: 'PowerAdmin', url: '/poweradmin', icon: 'square' },
    { title: 'Admin', url: '/admin', icon: 'square' },

  ];

  public appPages4DealerAdmin = [

    { title: 'DealerPowerAdmin', url: '/dealerpoweradmin', icon: 'ellipse' },
    { title: 'DealerAdmin', url: '/dealeradmin', icon: 'ellipse' },

  ];



  public appPages4CompanyAdmin = [

    { title: 'CompanyPowerAdmin', url: '/companypoweradmin', icon: 'ellipse' },
    { title: 'CompanyAdmin', url: '/companyadmin', icon: 'ellipse' },

  ];


  public appPages4User = [

    { title: 'Impersonate', url: '/userimpersonate', icon: 'people-circle' },
    { title: 'Home', url: '/home', icon: 'home' },
    { title: 'Login', url: '/login', icon: 'enter' },
    { title: 'Logout', url: '/logout', icon: 'exit' },
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

  
  handleMenuClick() {
    this.userNavigationService.UserGoBack();
  }

  public showPage4User(title: string): boolean {
    switch (title) {
      case 'Home':
        return true;
        break;

      case 'Login':
        return true;
        break;

      case 'Logout':
        if(this.authService.Token==null)
          return false;
        else
          return true;
        break;

      case 'Impersonate':
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
