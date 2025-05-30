import { Component, OnInit } from '@angular/core';
import { AccountService } from '../nvx/ClientServer-Service/Infrastructure/Account/account.service';
import { AuthService } from '../nvx/Utility/infrastructure/auth.service';
import { UserNavigationService } from '../nvx/Utility/infrastructure/user-navigation.service';
import { SignalrService } from '../nvx/Utility/infrastructure/signalr.service';
import { environment } from '../environments/environment';
import { MainMenuItem, MainMenuService, MenuType } from '../nvx/Utility/infrastructure/main-menu.service';


@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  standalone: false,
})
export class AppComponent implements OnInit {

  public appPages4SuperUser: MainMenuItem[] = [];
  public appPages4Admin: MainMenuItem[] = [];
  public appPages4DealerAdmin: MainMenuItem[] = [];
  public appPages4FinancialAdvisorAdmin: MainMenuItem[] = [];
  public appPages4CompanyAdmin: MainMenuItem[] = [];
  public appPages4User: MainMenuItem[] = [];

  

  public appPages = [


    { menuType: MenuType.MenuItem, zorder:0 , title: 'Inbox', url: '/folder/inbox', icon: 'mail' },
    { title: 'Outbox', url: '/folder/outbox', icon: 'paper-plane' },
    { title: 'Favorites', url: '/folder/favorites', icon: 'heart' },
    { title: 'Archived', url: '/folder/archived', icon: 'archive' },
    { title: 'Trash', url: '/folder/trash', icon: 'trash' },
    { title: 'Spam', url: '/folder/spam', icon: 'warning' },
  ];
  public labels = ['Family', 'Friends', 'Notes', 'Work', 'Travel', 'Reminders'];
  constructor(public authService: AuthService,
              public userNavigationService: UserNavigationService,
              public signalrService: SignalrService,
              private mainMenuService: MainMenuService
              )
  {

    this.appPages4SuperUser = this.mainMenuService.Pages4SuperUser;
    this.appPages4Admin = this.mainMenuService.Pages4Admin;
    this.appPages4DealerAdmin = this.mainMenuService.Pages4DealerAdmin;
    this.appPages4FinancialAdvisorAdmin = this.mainMenuService.Pages4FinancialAdvisorAdmin;
    this.appPages4CompanyAdmin = this.mainMenuService.Pages4CompanyAdmin;
    this.appPages4User = this.mainMenuService.Pages4User;
  }

  ngOnInit() {

    if (environment.signalR.useSignalR) {
       // collego gli eventi solo dopo la connessione che avviene dopo il login
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

  public getMenuItem4User() {

    return this.appPages4User;

    let currRole = [];

    if (this.authService.IsUser) {
      currRole.push("User");
    }

    let retVal = this.appPages4User.filter(item =>
      !item.roles || item.roles.some(role => currRole.includes(role))
    );

  }


  public getMenuItem4CompanyAdmin() {

    let currRole = [];

    if (this.authService.IsUser) {
      currRole.push("User");
    }
    if (this.authService.IsCompanyAdmin) {
      currRole.push("CompanyAdmin");
    }
    if (this.authService.IsCompanyPowerAdmin) {
      currRole.push("CompanyPowerAdmin");
    }

    let retVal =this.appPages4CompanyAdmin.filter(item =>
      !item.roles || item.roles.some(role => currRole.includes(role))
    );

    //non permette di impersonarsi a un user che ha anche ruoli admin
    if (this.authService.IsUser) {
      return retVal.filter(x => x.component != "UserCompanyListPageComponent");
    }

    return retVal;

    
  }

  public getMenuItems(menu: MainMenuItem[]): MainMenuItem[] {
    return menu
      .filter(x => x.menuType === MenuType.MenuItem)
      .sort((a, b) => a.zorder - b.zorder);
  }

  public getMenuAll(menu: MainMenuItem[]): MainMenuItem[] {
    return menu.slice().sort((a, b) => a.zorder - b.zorder);
  }

  public getMenuHeaderTitle(menuItems: MainMenuItem[]): string {
    const header = menuItems.find(item => item.menuType === MenuType.MenuHeader);
    return header ? header.title : '';
  }

  public getMenuNoteTitle(menuItems: MainMenuItem[], noteIndex: number = 0): string {
    const notes = menuItems.filter(item => item.menuType === MenuType.MenuNote).sort((a, b) => a.zorder - b.zorder);
    return notes[noteIndex] ? notes[noteIndex].title : '';
  }

}
