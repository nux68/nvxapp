import { Component } from '@angular/core';
import { AccountService } from '../nvx/ClientServer-Service/Account/account.service';
@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  standalone: false,
})
export class AppComponent {

  public appPages4User = [

    { title: 'Home', url: '/home', icon: 'home' },
    { title: 'Login', url: '/login', icon: 'enter' },
    { title: 'Logout', url: '/logout', icon: 'exit' },

  ];

  public appPages4Admin = [

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
  constructor( public accountService:AccountService) {}
}
