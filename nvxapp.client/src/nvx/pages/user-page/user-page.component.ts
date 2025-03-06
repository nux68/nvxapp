import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../Utility/signalr.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false
})
export class UserPageComponent  implements OnInit {

  public title!: string;

  constructor(private signalrService: SignalrService) {
    this.title = 'UserPage';
  }

  ngOnInit() {

    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
    

  }

}
