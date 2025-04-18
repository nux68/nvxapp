import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-request-justification-user-page',
  templateUrl: './request-justification-user-page.component.html',
  styleUrls: ['./request-justification-user-page.component.scss'],
  standalone:false
}) 
export class RequestJustificationUserPageComponent implements OnInit {



  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'RequestJustificationUserPage';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }

}
