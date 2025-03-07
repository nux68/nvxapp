import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-admin-page',
  templateUrl: './admin-page.component.html',
  styleUrls: ['./admin-page.component.scss'],
  standalone: false
})
export class AdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'AdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
