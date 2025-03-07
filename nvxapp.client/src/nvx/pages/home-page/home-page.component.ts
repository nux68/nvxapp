import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../Utility/user-navigation.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
  standalone: false
})
export class HomePageComponent  implements OnInit {

  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'Home';
  }

  ionViewWillEnter() {
  }
  

  ngOnInit() {}

}
