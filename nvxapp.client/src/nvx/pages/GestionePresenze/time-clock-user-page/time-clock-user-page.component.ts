import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-time-clock-user-page',
  templateUrl: './time-clock-user-page.component.html',
  styleUrls: ['./time-clock-user-page.component.scss'],
  standalone:false
}) 
export class TimeClockUserPageComponent implements OnInit {



  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'TimeClockUserPage';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }

}
