import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
}) 
export class RequestClockingUserPageComponent implements OnInit {



  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'RequestClockingUserPage';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }

}
