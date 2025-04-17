import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { environment } from '../../../../environments/environment';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';







@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false
}) 
export class UserPageComponent implements OnInit {

  public title!: string;

  constructor(private signalrService: SignalrService,
              public userNavigationService: UserNavigationService) {
    this.title = 'UserPage';
  }

  ionViewWillEnter() {
    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
  }


  ngOnInit() { }

}
