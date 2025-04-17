import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/user-navigation.service';

@Component({
  selector: 'app-time-sheet-admin-page',
  templateUrl: './time-sheet-admin-page.component.html',
  styleUrls: ['./time-sheet-admin-page.component.scss'],
  standalone:false
})
export class TimeSheetAdminPageComponent implements OnInit {

  

  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'TimeSheetAdmin';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }

}
